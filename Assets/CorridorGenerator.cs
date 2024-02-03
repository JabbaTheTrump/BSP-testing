using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorridorGenerator
{
    public int minCorridorSize;
    public int maxCorridorSize;

    public static List<Vector2Int> GenerateCorridor(BoundsInt room1, BoundsInt room2)
    {
        List<Vector2Int> corridorTiles = new List<Vector2Int>();

        // Get center points of both rooms
        Vector2Int center1 = new Vector2Int((int)room1.center.x, (int)room1.center.y);
        Vector2Int center2 = new Vector2Int((int)room2.center.x, (int)room2.center.y);

        // Start from the center of room1
        Vector2Int currentPos = center1;

        while (currentPos != center2)
        {
            // Move horizontally towards the center of room2
            if (currentPos.x != center2.x)
            {
                int stepX = Mathf.Clamp(center2.x - currentPos.x, -1, 1);
                currentPos.x += stepX;
            }
            // Move vertically towards the center of room2
            else if (currentPos.y != center2.y)
            {
                int stepY = Mathf.Clamp(center2.y - currentPos.y, -1, 1);
                currentPos.y += stepY;
            }

            // Add current position to corridor
            corridorTiles.Add(currentPos);
        }

        return corridorTiles;
    }
}
