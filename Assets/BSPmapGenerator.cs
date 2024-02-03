using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BSPmapGenerator : MonoBehaviour
{
    [SerializeField] int MapHeight = 100;
    [SerializeField] int MapWidth = 100;
    [SerializeField] int minRoomSize = 5;
    [SerializeField] int minPadding = 1;

    [SerializeField] List<Room> roomsList;


    private List<GameObject> activeRoomList;
    private List<GameObject> activeCorridorList;

    public GameObject point;
    public GameObject corridorMesh;

    int testCount = 0; // a debug count to prevent memory leaks

    int minSectionSize;


    private void Start()
    {
        minSectionSize = minRoomSize + 2 * minPadding;
        roomsList = new List<Room>();
    }

    public void Generate() //Generates a new map
    {
        testCount = 0;
        if (activeRoomList != null) // Resets the map
        {
            DestroyMapVisual(activeRoomList);

            if (activeCorridorList != null)
            {
                DestroyMapVisual(activeCorridorList);
            }
        }

        roomsList.Clear();

        SplitSection(new BoundsInt(0, 0, 0, MapWidth, MapHeight, 1)); // Generates the rooms

        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (Room room in roomsList)
        {
            roomCenters.Add(Vector2Int.RoundToInt(room.area.center));
        }

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        activeCorridorList = DrawCorridors(corridors);

        activeRoomList = DrawMap(roomsList); //Spawns and saves a list of all the room's floors
    }

   
    #region Section Splitting
    private void SplitSection(BoundsInt section) // recursivly splits rooms in a tree format until the leafs are below a threshold
    {
        testCount++;
        if (testCount > 300) // checks for a memory leak
        {
            Debug.Log("Memory leak detected");
            return;
        }

        if (section.size.y >= 2 * minSectionSize || section.size.x >= 2 * minSectionSize) //If the room is big enough to split..
        {
            bool splitHorizontally;

            if (section.size.y >= 2 * minSectionSize && section.size.x >= 2 * minSectionSize) //If both the width and height are big enough to split
            {
                if (section.size.x / section.size.y >= 2) splitHorizontally = false;  //If the width is much larger than the height split vertically
                else if (section.size.y / section.size.x >= 2) splitHorizontally = true;///If the height is much larger than the width split horizontally
                else { splitHorizontally = Random.Range(0, 1) > 0.5; } /// otherwise, split randomly
            }
            else 
            {
                if (section.size.y >= 2 * minSectionSize) splitHorizontally = true;
                else splitHorizontally = false;
            }


            if (splitHorizontally)
            {
                SplitHorizontally(section);
            }
            else
            {
                SplitVertically(section);
            }

        }
        else //If it's not big enough to split (AKA it's a leaf)
        {
            //Debug.Log($"I'm a leaf! pos: {room.xMin}:{room.y}, maxPos: {room.xMax}:{room.yMax}"); For debug purposes
            roomsList.Add(CreateRoomFromSection(section, minRoomSize, minPadding)); //Generate an actual room,
        }
    }

    private Room CreateRoomFromSection(BoundsInt section, int minRoomSize, int padding) //Generates new bounds contained within a BoundsInt parameter
    {

        int newPosX = Random.Range(section.x + padding, section.xMax - minRoomSize - padding);
        int newPosY = Random.Range(section.y + padding, section.yMax - minRoomSize - padding);

        int newWidth = Random.Range(minRoomSize, section.xMax - newPosX- padding);
        int newHeight = Random.Range(minRoomSize, section.yMax - newPosY - padding);

        //Debug.Log($"({newPosX}:{newPosY}), width: {newWidth}, height: {newHeight}"); for debug purposes

        BoundsInt cutRoom = new BoundsInt(newPosX, newPosY, section.z, newWidth, newHeight, section.size.z);
        return new Room(cutRoom);
    }

    private void SplitHorizontally(BoundsInt room)
    {
        int splitPos = Random.Range(minSectionSize , room.size.y - minSectionSize); // picks a random position on the y axis to split
        Debug.Log("splitPos: " + splitPos);

        //NOTE - height and width DO NOT take into account the room's position. height = maxY - minY

        int height = room.size.y; 
        int width = room.size.x;

        //Room A
        BoundsInt roomA = new BoundsInt(room.x, room.y, room.z, width, splitPos, room.size.z);
        //Debug.Log($"pos: {roomA.x} : {roomA.y}, maxPos: {roomA.xMax} : {roomA.yMax}, size: {roomA.size.x}:{roomA.size.y}"); For debug purposes

        SplitSection(roomA);

        //Room B
        BoundsInt roomB = new BoundsInt(room.x, room.y + splitPos, room.z, width, height - splitPos, room.size.z);
        //Debug.Log($"pos: {roomB.x} : {roomB.y}, maxPos: {roomB.xMax} : {roomB.yMax}"); For debug purposes

        SplitSection(roomB); 
    }

    private void SplitVertically(BoundsInt room)
    {
        int splitPos = Random.Range(minSectionSize, room.size.x - minSectionSize);
        int height = room.size.y;
        int width = room.size.x;

        //Room A
        BoundsInt roomA = new BoundsInt(room.x, room.y, room.z, splitPos, height, room.size.z);
        //Debug.Log($"pos: {roomA.x} : {roomA.y}, maxPos: {roomA.xMax} : {roomA.yMax}, size: {roomA.size.x}:{roomA.size.y}"); For debug purposes

        SplitSection(roomA);

        //Room B
        BoundsInt roomB = new BoundsInt(room.x + splitPos, room.y, room.z, width - splitPos, height, room.size.z);
        //Debug.Log($"pos: {roomB.x} : {roomB.y}, maxPos: {roomB.xMax} : {roomB.yMax}"); For debug purposes

        SplitSection(roomB);
    }
    #endregion

    private List<GameObject> DrawMap(List<Room> map) //Draws and returns a list of instantiated rooms
    {
        List<GameObject> list = new List<GameObject>();
        foreach (Room room in map)
        {
            list.Add(DrawRoom(room.area,false));
        }
        return list;
    }


    private List<GameObject> DrawCorridors(HashSet<Vector2Int> corr)
    {
        List<GameObject> tiles = new List<GameObject>();
        foreach(Vector2Int corridorTilePosition in corr)
        {
            GameObject tile = Instantiate(point, (Vector2)corridorTilePosition, Quaternion.identity);
            tiles.Add(tile);
        }

        return tiles;
    }

    private GameObject DrawRoom(BoundsInt room,bool isSection)  //Draws and returns an instantiated room
    {
        GameObject tile = Instantiate(point, room.center, Quaternion.identity);
        tile.transform.localScale = new Vector2(room.size.x, room.size.y);
        SpriteRenderer spr = tile.GetComponent<SpriteRenderer>();

        if (isSection)
        {
            spr.color = Random.ColorHSV();
        }
        else
        {
            spr.color = Color.red;
            spr.sortingOrder = 1;
        }
        return tile;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }

        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destination.y)
        {
            if (destination.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if (destination.y < position.y)
            {
                position += Vector2Int.down;
            }
            corridor.Add(position);
        }
        while (position.x != destination.x)
        {
            if (destination.x > position.x)
            {
                position += Vector2Int.right;
            }
            else if (destination.x < position.x)
            {
                position += Vector2Int.left;
            }
            corridor.Add(position);
        }
        return corridor;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;

        foreach(var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    private void DestroyMapVisual(List<GameObject> map) //Destroys all the objects within the list
    {
        foreach (GameObject tile in map)
        {
            Destroy(tile);
        }
    }
}

public class Room
{
    public BoundsInt area { get; private set; }
    public List<Room> adjacenedRooms = new List<Room>();  
    
    public Room(BoundsInt area)
    {
        SetArea(area);
    }

    private void SetArea(BoundsInt area)
    {
        this.area = area;
    }
}

