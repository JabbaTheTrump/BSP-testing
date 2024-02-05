using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSPmap
{
    private int minRoomLength;
    private int minRoomHeight;
    private int minPadding;
    private int testCount = 0;

    private int minSectionLength;
    private int minSectionHeight;

    private List<Room> roomsList = new List<Room>();


    public BSPmap(int mapWidth, int mapHeight, int minRoomLength, int minRoomHeight, int minPadding)
    {
        this.minRoomHeight = minRoomHeight;
        this.minRoomLength = minRoomLength;
        this.minPadding = minPadding;

        minSectionHeight = minRoomHeight + 2 * minPadding;
        minSectionLength = minRoomLength + 2 * minPadding;

        SplitSection(new BoundsInt(0, 0, 0, mapWidth, mapHeight, 1)); // Generates the rooms
    }


    private void SplitSection(BoundsInt section) // recursivly splits rooms in a tree format until the leafs are below a threshold
    {
        testCount++;
        if (testCount > 300) // checks for a memory leak
        {
            Debug.Log("Memory leak detected");
            return;
        }

        if (section.size.y >= 2 * minSectionHeight || section.size.x >= 2 * minSectionLength) //If the room is big enough to split..
        {
            bool splitHorizontally;

            if (section.size.y >= 2 * minSectionHeight && section.size.x >= 2 * minSectionLength) //If both the width and height are big enough to split
            {
                if (section.size.x / section.size.y >= 2) splitHorizontally = false;  //If the width is much larger than the height split vertically
                else if (section.size.y / section.size.x >= 2) splitHorizontally = true;///If the height is much larger than the width split horizontally
                else { splitHorizontally = Random.Range(0, 1) > 0.5; } /// otherwise, split randomly
            }
            else
            {
                if (section.size.y >= 2 * minSectionHeight) splitHorizontally = true;
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
            roomsList.Add(CreateRoomFromSection(section, minPadding)); //Generate an actual room,
        }
    }

    private Room CreateRoomFromSection(BoundsInt section, int padding) //Generates new bounds contained within a BoundsInt parameter
    {

        int newPosX = Random.Range(section.x + padding, section.xMax - minRoomLength - padding);
        int newPosY = Random.Range(section.y + padding, section.yMax - minRoomHeight - padding);

        int newWidth = Random.Range(minRoomLength, section.xMax - newPosX - padding);
        int newHeight = Random.Range(minRoomHeight, section.yMax - newPosY - padding);

        //Debug.Log($"({newPosX}:{newPosY}), width: {newWidth}, height: {newHeight}"); for debug purposes

        BoundsInt cutRoom = new BoundsInt(newPosX, newPosY, section.z, newWidth, newHeight, section.size.z);
        return new Room(cutRoom);
    }

    private void SplitHorizontally(BoundsInt room)
    {
        int splitPos = Random.Range(minSectionHeight, room.size.y - minSectionHeight); // picks a random position on the y axis to split
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
        int splitPos = Random.Range(minSectionLength, room.size.x - minSectionLength);
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

    public List<Room> GetRoomsList()
    {
        return roomsList;
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