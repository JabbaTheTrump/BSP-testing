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

    public Tilemap floorTileMap;
    public Tilemap wallTileMap;

    public GameObject point;
    public GameObject corridorMesh;

    public void Generate() //Generates a new map
    {
        floorTileMap.ClearAllTiles();
        wallTileMap.ClearAllTiles();

        List<Room> roomsList = new BSPmap(MapWidth, MapHeight, minRoomLength, minRoomHeight, minPadding).GetRoomsList();

        List<Vector2Int> roomCenters = new List<Vector2Int>();

        foreach (Room room in roomsList)
        {
            roomCenters.Add(Vector2Int.RoundToInt(room.area.center));
            DrawRoom(room.area);
        }

        HashSet<Vector2Int> corridors = new CorridorMapGenerator(roomCenters, wallTileMap, floorTileMap).ConnectRooms();
        DrawCorridors(corridors);
    }


    private void DrawCorridors(HashSet<Vector2Int> corr)
    {
        foreach(Vector2Int corridorTilePosition in corr)
        {
            floorTileMap.SetTile((Vector3Int)corridorTilePosition, floorTile);
        }

        foreach(Vector2Int corridorTilePosition in corr)
        {
            Vector3Int corPosition = (Vector3Int)corridorTilePosition;

            if (!floorTileMap.HasTile(corPosition + Vector3Int.up)) wallTileMap.SetTile(corPosition + Vector3Int.up, wallTile);
            if (!floorTileMap.HasTile(corPosition + Vector3Int.down)) wallTileMap.SetTile(corPosition + Vector3Int.down, wallTile);
            if (!floorTileMap.HasTile(corPosition + Vector3Int.left)) wallTileMap.SetTile(corPosition + Vector3Int.left, wallTile);
            if (!floorTileMap.HasTile(corPosition + Vector3Int.right)) wallTileMap.SetTile(corPosition + Vector3Int.right, wallTile);

            Vector3Int newpos = corPosition + Vector3Int.up + Vector3Int.left;
            if (!floorTileMap.HasTile(newpos)) wallTileMap.SetTile(newpos, wallTile);

            newpos = corPosition + Vector3Int.up + Vector3Int.right;
            if (!floorTileMap.HasTile(newpos)) wallTileMap.SetTile(newpos, wallTile);

            newpos = corPosition + Vector3Int.down + Vector3Int.left;
            if (!floorTileMap.HasTile(newpos)) wallTileMap.SetTile(newpos, wallTile);

            newpos = corPosition + Vector3Int.down + Vector3Int.right;
            if (!floorTileMap.HasTile(newpos)) wallTileMap.SetTile(newpos, wallTile);
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
                    wallTileMap.SetTile(new Vector3Int(x, y), wallTile);
                }
                else
                {
                    floorTileMap.SetTile(new Vector3Int(x, y, 0), floorTile);
                }
            }
        }
    }
}

