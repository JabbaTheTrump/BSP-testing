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
    [SerializeField] int maxRoomSize = 5;

    [SerializeField] List<BoundsInt> roomsList;

    private List<GameObject> currentMap;

    public GameObject point;

    int testCount = 0;

    private void Start()
    {
        roomsList = new List<BoundsInt>();
    }

    public void Generate()
    {
        if (currentMap != null)
        {
            DestroyMapVisual(currentMap);
        }

        SplitRoom(new BoundsInt(0, 0, 0, MapWidth, MapHeight, 1), minRoomSize);
        currentMap = DrawMap(roomsList);
    }

    private void SplitRoom(BoundsInt room, int minRoomSize)
    {
        testCount++;
        if (testCount > 100)
        {
            Debug.Log("Memory leak detected");
            return;
        }
        if (room.size.y >= 2 * minRoomSize && room.size.x >= 2 * minRoomSize) //If the room is big enough to split..
        {
            bool splitHorizontally;

            if (room.size.x / room.size.y >= 1.25) splitHorizontally = false;  //If the width is much larger than the height split vertically
            else if (room.size.y / room.size.x >= 1.25) splitHorizontally = true;///If the height is much larger than the width split horizontally
            else { splitHorizontally = Random.Range(0, 1) > 0.5; } /// otherwise, split randomly

            if (splitHorizontally)
            {
                SplitHorizontally(room, minRoomSize);
            }
            else
            {
                SplitVertically(room, minRoomSize);
            }

        }
        else
        {
            Debug.Log($"I'm a leaf! pos: {room.xMin}:{room.y}, maxPos: {room.xMax}:{room.yMax}");
            roomsList.Add(room);
        }
    }

    private void SplitHorizontally(BoundsInt room, int minRoomSize)
    {
        int splitPos = Random.Range(minRoomSize , room.size.y - minRoomSize);
        Debug.Log("splitPos: " + splitPos);

        int height = room.size.y;
        int width = room.size.x;

        //Room A
        BoundsInt roomA = new BoundsInt(room.x, room.y, room.z, width, splitPos, room.size.z);
        Debug.Log($"pos: {roomA.x} : {roomA.y}, maxPos: {roomA.xMax} : {roomA.yMax}, size: {roomA.size.x}:{roomA.size.y}");

        SplitRoom(roomA, minRoomSize);

        //Room B
        BoundsInt roomB = new BoundsInt(room.x, room.y + splitPos, room.z, width, height - splitPos, room.size.z);
        Debug.Log($"pos: {roomB.x} : {roomB.y}, maxPos: {roomB.xMax} : {roomB.yMax}");

        SplitRoom(roomB, minRoomSize);
    }

    private void SplitVertically(BoundsInt room, int minRoomSize)
    {
        int splitPos = Random.Range(minRoomSize, room.size.y - minRoomSize);
        Debug.Log("splitPos: " + splitPos);

        int height = room.size.y;
        int width = room.size.x;

        //Room A
        BoundsInt roomA = new BoundsInt(room.x, room.y, room.z, splitPos, height, room.size.z);
        Debug.Log($"pos: {roomA.x} : {roomA.y}, maxPos: {roomA.xMax} : {roomA.yMax}, size: {roomA.size.x}:{roomA.size.y}");

        SplitRoom(roomA, minRoomSize);

        //Room B
        BoundsInt roomB = new BoundsInt(splitPos, room.y + splitPos, room.z, width - splitPos, height, room.size.z);
        Debug.Log($"pos: {roomB.x} : {roomB.y}, maxPos: {roomB.xMax} : {roomB.yMax}");

        SplitRoom(roomB, minRoomSize);
    }

    private List<GameObject> DrawMap(List<BoundsInt> map)
    {
        List<GameObject> list = new List<GameObject>();
        foreach (BoundsInt room in map)
        {
            list.Add(DrawRoom(room));
        }
        return list;
    }

    private GameObject DrawRoom(BoundsInt room)
    {
        GameObject tile = Instantiate(point, new Vector3(room.x + 0.5f * room.xMax, room.y + 0.5f * room.yMax), Quaternion.identity);
        tile.transform.localScale = new Vector2(room.size.x, room.size.y);
        return tile;
    }

    private void DestroyMapVisual(List<GameObject> map)
    {
        foreach (GameObject tile in map)
        {
            Destroy(tile);
        }
    }

}
