# BSP Map Generator (Unity)

This project is a procedural map generator built with Unity that uses the **Binary Space Partitioning (BSP)** algorithm to create dungeon-style rooms and corridors. It is designed for use in 2D tile-based games, such as roguelikes or dungeon crawlers.

---

## Features

- Recursive BSP algorithm to split the map into smaller sections
- Dynamic room generation with customizable dimensions and padding
- Automatic corridor generation that connects all rooms
- Visualization using Unity Tilemaps (floor and wall tiles)
- Support for customizable tile assets and corridor mesh rendering

---

## How It Works

1. **BSP Split:**

   - The map starts as a single `BoundsInt` section.
   - It is recursively split horizontally or vertically based on its dimensions.
   - Splitting stops when the resulting sections are too small to split further.

2. **Room Creation**:

   - A room is carved randomly within each final section (leaf node), with padding applied.
   - The resulting room data is stored in a `Room` class with a `BoundsInt` area.

3. **Corridor Generation**:

   - The center of each room is stored.
   - Corridors are generated to connect all room centers, ensuring full connectivity.

4. **Tilemap Drawing**:

   - Floor and wall tiles are drawn to Unity `Tilemap` components.
   - Walls are added around room and corridor perimeters.

---

## Configuration Parameters

These are exposed in the inspector via `BSPmapGenerator.cs`:

- `MapWidth` and `MapHeight`: Total size of the map
- `minRoomLength` and `minRoomHeight`: Minimum room dimensions
- `minPadding`: Distance between rooms and section edges

---

## Project Structure

- `BSPmap.cs`: Core logic for splitting the map and generating rooms
- `Room.cs`: Data structure for a single room
- `BSPmapGenerator.cs`: Unity MonoBehaviour for triggering generation and drawing tiles
- `CorridorMapGenerator.cs`:  Responsible for creating corridor paths
---
## Demo:

![2025-04-2214-34-33online-video-cutter com-ezgif com-optimize](https://github.com/user-attachments/assets/1ac6143f-7a57-4ab8-b548-12c8ecf25e2f)
