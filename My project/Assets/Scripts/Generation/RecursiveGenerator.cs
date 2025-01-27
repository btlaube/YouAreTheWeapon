using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RecursiveGenerator : MonoBehaviour
{
    [SerializeField] int width, height;
    [SerializeField] int numRooms, roomWidth, roomHeight;
    [SerializeField] GameObject roomPrefab;
    [SerializeField] GameObject startRoomPrefab;
    [SerializeField] GameObject finalRoomPrefab;

    [SerializeField] private Dictionary<Vector2Int, Room> roomMap = new Dictionary<Vector2Int, Room>(); // Map to store rooms by position

    [Header("Tiles")]
    public TileBase wallTile;
    public TileBase floorTile;
    public TileBase platformTile;

    private Camera mCamera;

    void Awake()
    {
        mCamera = Camera.main;
    }

    void Start()
    {
        // Randomize room count
        numRooms = Random.Range(4, 7);
        Generate();
        GenAgain();
        mCamera.GetComponent<CameraController>().UpdateCameraPositions();
    }

    private void Generate()
    {

        // Start the recursive room generation from the starting room
        int startX = 0;
        int startY = 0;

        Room startRoom = new Room(0, new Vector2Int(0, 0));
        startRoom.exitPoint = new Vector2Int(1, 0);
        roomMap[new Vector2Int(0, 0)] = startRoom;

        GeneratePath(startRoom, startX + startRoom.exitPoint.x, startY + startRoom.exitPoint.y, 1); // Start recursive path generation
    }

    private bool GeneratePath(Room previousRoom, int x, int y, int roomsCreated)
    {
        // Base case: stop if we've created enough rooms
        if (roomsCreated >= numRooms)
            return true;

        // Create a new room
        Vector2Int position = new Vector2Int(x, y);
        Room currentRoom = new Room(roomsCreated, position);
        currentRoom.prevRoom = previousRoom;
        currentRoom.entryPoint = previousRoom.exitPoint * new Vector2Int(-1, -1);
        previousRoom.nextRoom = currentRoom;
        roomMap[new Vector2Int(x, y)] = currentRoom;
        
        // Define the possible directions (up, right, down)
        List<Vector2Int> directions = new List<Vector2Int>
        {
            new Vector2Int(0, 1),  // Up
            new Vector2Int(1, 0),  // Right
            new Vector2Int(0, -1)  // Down
        };

        // Randomize the order of directions
        directions.Sort((a, b) => Random.Range(-1, 2));

        // Iterate through randomized directions
        foreach (Vector2Int direction in directions)
        {
            int nextX = x + direction.x;
            int nextY = y + direction.y;
            Vector2Int nextPosition = position + direction;

            // Check if the next position is valid and unvisited
            if (nextX >= 0 && nextX < width && nextY >= 0 && nextY < height && !roomMap.ContainsKey(nextPosition))
            {
                // if next room, set exit point
                if (roomsCreated != numRooms-1)
                    currentRoom.exitPoint = direction;
                // Recur to generate the next room in the path
                if (GeneratePath(currentRoom, nextX, nextY, roomsCreated + 1))
                {
                    return true;
                }
            }
        }
        return false; // Return false if no valid room placement is found
    }    

    private void GenAgain()
    {
        foreach (Room room in roomMap.Values)
        {
            GameObject roomPrefabInstance;

            if (room.prevRoom == null) // Start room
            {
                roomPrefabInstance = Instantiate(startRoomPrefab, new Vector2(room.position.x * roomWidth, room.position.y * roomHeight), Quaternion.identity, transform);
                Tilemap tilemap = roomPrefabInstance.GetComponentInChildren<Tilemap>();
                AddTilesToRoom(room, tilemap, wallTile, floorTile);
                AddExitWallsToRoom(room, tilemap, wallTile, floorTile);
            }
            else if (room.nextRoom == null) // Final room
            {
                roomPrefabInstance = Instantiate(finalRoomPrefab, new Vector2(room.position.x * roomWidth, room.position.y * roomHeight), Quaternion.identity, transform);
                Tilemap tilemap = roomPrefabInstance.GetComponentInChildren<Tilemap>();
                AddTilesToRoom(room, tilemap, wallTile, floorTile);
            }
            else // Middle rooms
            {
                roomPrefabInstance = Instantiate(roomPrefab, new Vector2(room.position.x * roomWidth, room.position.y * roomHeight), Quaternion.identity, transform);
                roomPrefabInstance.GetComponent<MazeGenerator>().ActivateMaze(room.entryPoint, room.exitPoint);
                Tilemap tilemap = roomPrefabInstance.GetComponentInChildren<Tilemap>();
                AddTilesToRoom(room, tilemap, wallTile, floorTile);
                AddExitWallsToRoom(room, tilemap, wallTile, floorTile);
            }

            
            // Debug.Log(room);
        }
    }

    private void AddTilesToRoom(Room room, Tilemap tilemap, TileBase wallTile, TileBase floorTile)
    {
        // Determine which walls, ceiling, and floor should have tiles
        bool hasLeftWall = room.entryPoint != new Vector2Int(-1, 0) && room.exitPoint != new Vector2Int(-1, 0);
        bool hasRightWall = room.entryPoint != new Vector2Int(1, 0) && room.exitPoint != new Vector2Int(1, 0);
        bool hasBottomWall = room.entryPoint != new Vector2Int(0, -1) && room.exitPoint != new Vector2Int(0, -1);
        bool hasTopWall = room.entryPoint != new Vector2Int(0, 1) && room.exitPoint != new Vector2Int(0, 1);

        // Offset to move the tiles to the bottom-left corner
        int offsetX = -roomWidth / 2;
        int offsetY = -roomHeight / 2;

        // Add tiles to the tilemap based on the presence of walls/floors/ceilings
        for (int x = 0; x <= roomWidth; x++)
        {
            for (int y = 0; y <= roomHeight; y++)
            {
                // Adjust the position with the offset
                int tileX = x + offsetX;
                int tileY = y + offsetY;

                // Left wall
                if (hasLeftWall && x == 0)
                    tilemap.SetTile(new Vector3Int(tileX, tileY, 0), wallTile);

                // Right wall
                if (hasRightWall && x == roomWidth - 1)
                    tilemap.SetTile(new Vector3Int(tileX, tileY, 0), wallTile);

                // Bottom wall (floor)
                if (hasBottomWall && y == 0)
                    tilemap.SetTile(new Vector3Int(tileX, tileY, 0), floorTile);

                // Top wall (ceiling)
                if (hasTopWall && y == roomHeight - 1)
                    tilemap.SetTile(new Vector3Int(tileX, tileY, 0), floorTile);
            }
        }
    }

    private void AddExitWallsToRoom(Room room, Tilemap tilemap, TileBase wallTile, TileBase floorTile)
    {
        // Determine which walls, ceiling, and floor should have tiles
        bool hasLeftWall = room.entryPoint != new Vector2Int(-1, 0) && room.exitPoint != new Vector2Int(-1, 0);
        bool hasRightWall = room.entryPoint != new Vector2Int(1, 0) && room.exitPoint != new Vector2Int(1, 0);
        bool hasBottomWall = room.entryPoint != new Vector2Int(0, -1) && room.exitPoint != new Vector2Int(0, -1);
        bool hasTopWall = room.entryPoint != new Vector2Int(0, 1) && room.exitPoint != new Vector2Int(0, 1);

        // Offset to move the tiles to the bottom-left corner
        int offsetX = -roomWidth / 2;
        int offsetY = -roomHeight / 2;

        // Add tiles to the tilemap based on the presence of walls/floors/ceilings
        for (int x = 0; x <= roomWidth; x++)
        {
            for (int y = 0; y <= roomHeight; y++)
            {
                // Adjust the position with the offset
                int tileX = x + offsetX;
                int tileY = y + offsetY;

                // Right exit
                if (!hasRightWall && x == roomWidth - 1 && y > 3)
                    tilemap.SetTile(new Vector3Int(tileX, tileY, 0), wallTile);

                // Bottom exit
                if (!hasBottomWall && y == 0 && x < roomWidth - 4)
                    tilemap.SetTile(new Vector3Int(tileX, tileY, 0), floorTile);

                // Top exit
                if (!hasTopWall && y == roomHeight - 1 && x < roomWidth - 4)
                    tilemap.SetTile(new Vector3Int(tileX, tileY, 0), floorTile);
            }
        }
    }

    public void ClearRooms()
    {

    }

    public void Regenerate()
    {
        // Make previous final room new starting room
            // Generate new maze starting from previous final room
            // Don't generate a StartRoom prefab (skip to Middle Room)
        // Possibly center new maaze.
    }

}
