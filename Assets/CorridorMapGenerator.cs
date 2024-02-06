using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CorridorMapGenerator
{
    public Tilemap wallTileMap;
    public Tilemap floorTileMap;
    public List<Vector2Int> roomCenters;

    public CorridorMapGenerator(List<Vector2Int> roomCenters, Tilemap wallTileMap, Tilemap floorTileMap)
    {
        this.floorTileMap = floorTileMap;
        this.wallTileMap = wallTileMap;
        this.roomCenters = roomCenters;
    }

    public HashSet<Vector2Int> ConnectRooms()
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);

            floorTileMap.SetTileFlags((Vector3Int)closest, TileFlags.None);
            floorTileMap.SetColor((Vector3Int)currentRoomCenter, Color.red);


            currentRoomCenter = closest;

            corridors.UnionWith(newCorridor);
        }

        foreach(Vector2Int corridor in corridors)
        {
            Vector3Int newCor = (Vector3Int)corridor;

            //floorTileMap.SetTileFlags(newCor, TileFlags.None);
            //floorTileMap.SetColor(newCor, Color.red);
        }

        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        int test = 0;

        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);

        while (position.y != destination.y) //While the corridor hasn't reached the destination's y value
        {
            test++;
            if (test > 500)
            {
                Debug.Log("memory leak detected");
                return null;
            }
                
            if (destination.y > position.y) //If the destination is above the corridor
            {
                position = TryMoveVertical(position, Vector2Int.up, destination);
            }
            else if (destination.y < position.y) //While the corridor is above the room center
            {
                position = TryMoveVertical(position, Vector2Int.down, destination);
            }
            corridor.Add(position);
        }
        while (position.x != destination.x)
        {
            if (destination.x > position.x)
            {
                position = TryMoveHorizontal(position, Vector2Int.right);
            }
            else if (destination.x < position.x)
            {
                position = TryMoveHorizontal(position, Vector2Int.left);
            }
            corridor.Add(position);
        }
        return corridor;
    }

    private Vector2Int TryMoveVertical(Vector2Int position, Vector2Int direction, Vector2Int destination)
    {
        Vector3Int newPos = (Vector3Int)position + (Vector3Int)direction; //Take the tile above the corridor

        if (!wallTileMap.HasTile((Vector3Int)newPos)) //If the tile is empty
        {
            return (Vector2Int)newPos;
        }
        else //If there is a wall there
        {
            if (wallTileMap.HasTile(newPos + Vector3Int.left) && wallTileMap.HasTile(newPos + Vector3Int.right)) //If it has adjacened walls
            {
                wallTileMap.SetTile(newPos, null); //Create a door
                return (Vector2Int)newPos;
            }
            else
            {
                if (position.x < destination.x) //If the desitnation is to the right of the position
                {
                    newPos = (Vector3Int)position + Vector3Int.right; //Take the tile to the right of the corridor
                    return (Vector2Int)newPos;
                }
                else  //If the destination is to the left of the position
                {
                    newPos = (Vector3Int)position + Vector3Int.left; //Take the tile to the left of the corridor
                    return (Vector2Int)newPos;
                }

            }
        }
    }

    private Vector2Int TryMoveHorizontal(Vector2Int position, Vector2Int direction)
    {
        Vector3Int newPos = (Vector3Int)position + (Vector3Int)direction; //Take the tile above the corridor

        if (!wallTileMap.HasTile((Vector3Int)newPos)) //If the tile is empty
        {
            return (Vector2Int)newPos;
        }
        else //If there is a wall there
        {
            if (wallTileMap.HasTile(newPos + Vector3Int.up) && wallTileMap.HasTile(newPos + Vector3Int.down)) //If it has adjacened walls
            {
                wallTileMap.SetTile(newPos, null); //Create a door
                return (Vector2Int)newPos;
            }
            else
            {
                newPos = (Vector3Int)position + Vector3Int.up; //Take the tile to the left of the corridor
                return (Vector2Int)newPos;
            }
        }
    }

    //private static Vector2Int TryMoveTowards(Vector2Int position, Vector2Int destination)
    //{

    //}

    private static Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;

        foreach (var position in roomCenters)
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
}
