using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class BSPmapGenerator : MonoBehaviour
{
    [SerializeField] int MapHeight = 100;
    [SerializeField] int MapWidth = 100;
    [SerializeField] int minRoomSize = 5;
    [SerializeField] int minPadding = 1;

    [SerializeField] List<BoundsInt> roomsList;

    private List<GameObject> currentMap;

    public GameObject point;

    int testCount = 0; // a debug count to prevent memory leaks

    int minSectionSize;


    private void Start()
    {
        minSectionSize = minRoomSize + 2 * minPadding;
        roomsList = new List<BoundsInt>();
    }

    public void Generate()
    {
        testCount = 0;
        if (currentMap != null)
        {
            DestroyMapVisual(currentMap);
        }

        SplitSection(new BoundsInt(0, 0, 0, MapWidth, MapHeight, 1));
        currentMap = DrawMap(roomsList);
        roomsList.Clear();
    }

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

    private BoundsInt CreateRoomFromSection(BoundsInt section, int minRoomSize, int padding) //Generates new bounds contained within a BoundsInt parameter
    {

        int newPosX = Random.Range(section.x + padding, section.xMax - minRoomSize - padding);
        int newPosY = Random.Range(section.y + padding, section.yMax - minRoomSize - padding);

        int newWidth = Random.Range(minRoomSize, section.xMax - newPosX- padding);
        int newHeight = Random.Range(minRoomSize, section.yMax - newPosY - padding);

        //Debug.Log($"({newPosX}:{newPosY}), width: {newWidth}, height: {newHeight}"); for debug purposes

        BoundsInt cutRoom = new BoundsInt(newPosX, newPosY, section.z, newWidth, newHeight, section.size.z);
        return cutRoom;
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
        Debug.Log("splitPos: " + splitPos);

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

    private List<GameObject> DrawMap(List<BoundsInt> map) //Draws and returns a list of instantiated rooms
    {
        List<GameObject> list = new List<GameObject>();
        foreach (BoundsInt room in map)
        {
            list.Add(DrawRoom(room,false));
        }
        return list;
    }

    private GameObject DrawRoom(BoundsInt room,bool isSection)  //Draws and returns an instantiated room
    {
        GameObject tile = Instantiate(point, new Vector3(room.x + 0.5f * room.size.x, room.y + 0.5f * room.size.y), Quaternion.identity);
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

    private void DestroyMapVisual(List<GameObject> map) //Destroys all the objects within the list
    {
        foreach (GameObject tile in map)
        {
            Destroy(tile);
        }
    }

}
