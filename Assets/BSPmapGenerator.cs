using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BSPmapGenerator : MonoBehaviour
{
    [SerializeField] int MapHeight = 100;
    [SerializeField] int MapWidth = 100;
    [SerializeField] int minRoomLength = 5;
    [SerializeField] int minRoomHeight = 5;
    [SerializeField] int minPadding = 1;

    public Tile floorTile;
    public Tile wallTile;

    public Tilemap floorTiles;
    public Tilemap wallTiles;

    public GameObject point;
    public GameObject corridorMesh;

    public void Generate() //Generates a new map
    {
        floorTiles.ClearAllTiles();
        wallTiles.ClearAllTiles();

        List<Room> roomsList = new BSPmap(MapWidth, MapHeight, minRoomLength, minRoomHeight, minPadding).GetRoomsList();

        List<Vector2Int> roomCenters = new List<Vector2Int>();

        foreach (Room room in roomsList)
        {
            roomCenters.Add(Vector2Int.RoundToInt(room.area.center));
            DrawRoom(room.area);
        }

        HashSet<Vector2Int> corridors = CorridorMapGenerator.ConnectRooms(roomCenters);
        DrawCorridors(corridors);
    }


    private void DrawCorridors(HashSet<Vector2Int> corr)
    {
        foreach(Vector2Int corridorTilePosition in corr)
        {
            floorTiles.SetTile((Vector3Int)corridorTilePosition, floorTile);
        }
    }

    private void DrawRoom(BoundsInt room) 
    {
        for (int x = room.x; x <= room.xMax; x++)
        {
            for (int y = room.y; y <= room.yMax; y++)
            {
                if (x == room.x || x == room.xMax || y == room.y || y == room.yMax)
                {
                    wallTiles.SetTile(new Vector3Int(x, y), wallTile);
                }
                else
                {
                    floorTiles.SetTile(new Vector3Int(x, y, 0), floorTile);
                }
            }
        }
    }


}

