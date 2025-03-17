using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RecursiveGenerator : MonoBehaviour
{
    public int width, height;
    public Vector2Int roomNumRange;
    public int numRooms, roomWidth, roomHeight;
    public GameObject roomPrefab;
    public GameObject startRoomPrefab;
    public GameObject finalRoomPrefab;

    private Dictionary<Vector2Int, Room> roomMap = new Dictionary<Vector2Int, Room>(); // Map to store rooms by position

    [Header("Tiles")]
    public TileBase wallTile;
    public TileBase floorTile;
    public TileBase platformTile;
    public TileBase vertDoorTile;
    public TileBase horiDoorTile;

    public GameObject doorLightPrefab;
    public GameObject doorParticles;

    private Camera mCamera;

    void Awake()
    {
        mCamera = Camera.main;
    }

    void Start()
    {
        //     // Randomize room count
        //     numRooms = Random.Range(4, 7);
        // Generate();
        //     GenAgain();
        //     mCamera.GetComponent<CameraController>().UpdateCameraPositions();
    }

    public void DebugRoomMap()
    {
        foreach (KeyValuePair<Vector2Int, Room> entry in roomMap)
        {
            Vector2Int position = entry.Key;
            Room room = entry.Value;

            Debug.Log($"Position: {position}, Room: {room}");
        }
    }

    public Dictionary<Vector2Int, Room> GetRoomMap()
    {
        return this.roomMap;
    }

    public void Generate()
    {
        // Randomize room count
        numRooms = Random.Range(roomNumRange.x, roomNumRange.y);

        Room startRoom = new Room(0, new Vector2Int(0, 0));
        startRoom.exitPoint = new Vector2Int(1, 0); // Fix exit to right
        startRoom.exitPointOffset = OffsetEntryOrExit(startRoom.exitPoint);
        roomMap[new Vector2Int(0, 0)] = startRoom;

        GeneratePath(startRoom, startRoom.exitPoint.x, startRoom.exitPoint.y, 1); // Start recursive path generation
        GenAgain();
        StartCoroutine(WaitAndUpdateCamera());
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
        currentRoom.entryPointOffset = previousRoom.exitPointOffset;    //OffsetEntryOrExit(currentRoom.entryPoint);
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
                // if (roomsCreated != numRooms-1)
                currentRoom.exitPoint = direction; // Ignore check for next room add exitPoint to Final Room.

                currentRoom.exitPointOffset = OffsetEntryOrExit(currentRoom.exitPoint);

                // Recurse to generate the next room in the path
                if (GeneratePath(currentRoom, nextX, nextY, roomsCreated + 1))
                {
                    return true;
                }
            }
        }
        return false; // Return false if no valid room placement is found
    }    

    public int OffsetEntryOrExit(Vector2Int roomEntryOrExit)
    {
        int offset;
        // Create exit point offset based on direction 
            // Horizontal exit has more possible exit options
        if (roomEntryOrExit.x == 0) // Up and down options
        {
            offset = Random.Range(0, 9);
        }
        else // Right exit option
        {
            offset = Random.Range(0, 5);
        }
        return offset;
    }

    private void GenAgain()
    {
        foreach (Room room in roomMap.Values)
        {
            GameObject roomPrefabInstance;

            if (room.prevRoom == null) // Start room
            {
                roomPrefabInstance = Instantiate(startRoomPrefab, new Vector2(room.position.x * roomWidth, room.position.y * roomHeight), Quaternion.identity, transform);
                room.SetInstance(roomPrefabInstance);
                Tilemap tilemap = roomPrefabInstance.GetComponentInChildren<Tilemap>();
                AddTilesToRoom(room, tilemap, wallTile, floorTile);
                AddExitWallsToRoom(room, tilemap, wallTile, floorTile);
            }
            else if (room.nextRoom == null) // Final room
            {
                roomPrefabInstance = Instantiate(finalRoomPrefab, new Vector2(room.position.x * roomWidth, room.position.y * roomHeight), Quaternion.identity, transform);
                room.SetInstance(roomPrefabInstance);
                Tilemap tilemap = roomPrefabInstance.GetComponentInChildren<Tilemap>();
                AddTilesToRoom(room, tilemap, wallTile, floorTile);
                AddExitWallsToRoom(room, tilemap, wallTile, floorTile);
            }
            else // Middle rooms
            {
                roomPrefabInstance = Instantiate(roomPrefab, new Vector2(room.position.x * roomWidth, room.position.y * roomHeight), Quaternion.identity, transform);
                roomPrefabInstance.GetComponent<MazeGenerator>().ActivateMaze(room, room.entryPoint, room.exitPoint);
                room.SetInstance(roomPrefabInstance);
                Tilemap tilemap = roomPrefabInstance.GetComponentInChildren<Tilemap>();
                AddTilesToRoom(room, tilemap, wallTile, floorTile);
                AddExitWallsToRoom(room, tilemap, wallTile, floorTile);
            }

            
            Debug.Log(room);
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


    // TODO: Modify to only draw on exits, find hasLeftExit not hasLeftWall, etc.
    private void AddExitWallsToRoom(Room room, Tilemap tilemap, TileBase wallTile, TileBase floorTile)
    {
        // bool hasLeftExit= mazeExit == new Vector2Int(-1, 0);
        bool hasRightExit = room.exitPoint == new Vector2Int(1, 0);
        bool hasBottomExit = room.exitPoint == new Vector2Int(0, -1);
        bool hasTopExit = room.exitPoint == new Vector2Int(0, 1);

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
                if (hasRightExit && x == roomWidth - 1)
                {
                    if (y <= (room.exitPointOffset * 4) || y >= ((room.exitPointOffset * 4) + 4))
                    {
                        tilemap.SetTile(new Vector3Int(tileX, tileY, 0), wallTile);
                    }
                    else
                    {
                        tilemap.SetTile(new Vector3Int(tileX, tileY, 0), vertDoorTile);
                        // Check if the current cell is the center of the door vertically
                        if (y % 4 == 2)
                            Instantiate(doorLightPrefab, new Vector2(room.instance.transform.position.x + tileX, room.instance.transform.position.y + tileY), Quaternion.identity, room.instance.transform);
                    }
                }

                // Bottom exit
                if (hasBottomExit && y == 0)
                {
                    if (x <= (room.exitPointOffset * 4) || x >= ((room.exitPointOffset * 4) + 4))
                    {
                        tilemap.SetTile(new Vector3Int(tileX, tileY, 0), floorTile);
                    }
                    else
                    {
                        tilemap.SetTile(new Vector3Int(tileX, tileY, 0), horiDoorTile);
                        // Check if the current cell is the center of the door vertically
                        if (x % 4 == 2)
                            Instantiate(doorLightPrefab, new Vector2(room.instance.transform.position.x + tileX, room.instance.transform.position.y + tileY), Quaternion.identity, room.instance.transform);
                    }
                }

                // Top exit
                if (hasTopExit && y == roomHeight - 1)
                {
                    if (x <= (room.exitPointOffset * 4) || x >= ((room.exitPointOffset * 4) + 4))
                    {
                        tilemap.SetTile(new Vector3Int(tileX, tileY, 0), floorTile);
                    }
                    else
                    {
                        tilemap.SetTile(new Vector3Int(tileX, tileY, 0), horiDoorTile);
                        // Check if the current cell is the center of the door vertically
                        if (x % 4 == 2)
                            Instantiate(doorLightPrefab, new Vector2(room.instance.transform.position.x + tileX, room.instance.transform.position.y + tileY), Quaternion.identity, room.instance.transform);
                    }
                }
            }
        }
    }
    
    public void ClearRoomExitDoor(Vector2Int roomPosition)
    {
        Room room = roomMap[roomPosition];
        Tilemap tilemap = room.instance.GetComponentInChildren<Tilemap>();

        // bool hasLeftExit= mazeExit == new Vector2Int(-1, 0);
        bool hasRightExit = room.exitPoint == new Vector2Int(1, 0);
        bool hasBottomExit = room.exitPoint == new Vector2Int(0, -1);
        bool hasTopExit = room.exitPoint == new Vector2Int(0, 1);

        StartCoroutine(AnimateExitDoor(room, tilemap, hasRightExit, hasBottomExit, hasTopExit));
    }

    public IEnumerator AnimateExitDoor(Room room, Tilemap tilemap, bool hasRightExit, bool hasBottomExit, bool hasTopExit)
    {
        // Vertical Door tiles (flip x and y for horizontal doors)
        // Top: (roomWidth - 1, room.exitPointOffset * 4 + 1)
        // Center: (roomWidth - 1, room.exitPointOffset * 4 + 2)
        // Bottom: (roomWidth - 1, room.exitPointOffset * 4 + 3)

        // Offset to move the tiles to the bottom-left corner
        Vector2Int offset = new Vector2Int(-roomWidth / 2, -roomHeight / 2);

        // Adjust the position with the offset
        float tileX = 0.0f;
        float tileY = 0.0f;
        if (hasRightExit)
        {
            tileX = (roomWidth - 0.775f) + offset.x;
            tileY = ((room.exitPointOffset * 4) + 4.5f) + offset.y;
        }
        if (hasBottomExit)
        {
            tileX = ((room.exitPointOffset * 4) + 4) + offset.x;
            tileY = offset.y;
        }
        if (hasTopExit)
        {
            tileX = ((room.exitPointOffset * 4) + 4) + offset.x;
            tileY = roomHeight + offset.y;
        }
        Instantiate(doorParticles, 
                new Vector3(room.instance.transform.position.x + tileX, 
                            room.instance.transform.position.y + tileY, 
                            room.instance.transform.position.z), 
                Quaternion.Euler(90, 0, 0),
                room.instance.transform);

        for (int i = 1; i < 4; i++)
        {
            if (hasRightExit)
            {
                tilemap.SetTile(new Vector3Int((roomWidth - 1) + offset.x, ((room.exitPointOffset * 4) + i) + offset.y, 0), null);  // Clear door tiles
                yield return new WaitForSeconds(0.55f);
            }
            if (hasBottomExit)
            {
                tilemap.SetTile(new Vector3Int(((room.exitPointOffset * 4) + i) + offset.x, offset.y, 0), null);  // Clear door tiles
                yield return new WaitForSeconds(0.55f);
            }
            if (hasTopExit)
            {
                tilemap.SetTile(new Vector3Int(((room.exitPointOffset * 4) + i) + offset.x, (roomHeight - 1) + offset.y, 0), null);  // Clear door tiles
                yield return new WaitForSeconds(0.5f);
            }
        }
        yield return new WaitForSeconds(0.1f);
    }

    public void ClearRooms()
    {
        // Loop through all children except the last one and destroy them
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // Reset the last child's position
        Transform lastChild = transform.GetChild(transform.childCount - 1);
        lastChild.localPosition = Vector3.zero;
    }

    public void ClearRoomMap()
    {
        // Clear roomMap
        Dictionary<Vector2Int, Room> newMap = new Dictionary<Vector2Int, Room>(); // Map to store rooms by position
        foreach (KeyValuePair<Vector2Int, Room> kvp in roomMap)
        {
            Vector2Int position = kvp.Key;
            Room room = kvp.Value;
            if (room.nextRoom == null)   // Final room
            {
                Debug.Log($"Dont remove final room: {room}");
                room.position = new Vector2Int(0, 0);
                newMap[new Vector2Int(0, 0)] = room; // Only add previous final room to new room map at 0, 0
            }
        }
        roomMap = newMap;
    }

    public void Regenerate()
    {
        ClearRooms();

        // Get the previous final room
        Room startRoom = null;
        foreach (Room room in roomMap.Values)
        {
            if (room.nextRoom == null) // Final room
            {
                startRoom = room;
                break;
            }
        }

        if (startRoom == null) return;

        // Calculate the new starting position based on the exitPoint of the previous final room
        Vector2Int newStartPosition = startRoom.position + startRoom.exitPoint;

        // Update the room's position and reset connections
        startRoom.position = newStartPosition;
        startRoom.prevRoom = null;
        startRoom.nextRoom = null;

        ClearRoomMap();

        // Randomize room count
        numRooms = Random.Range(4, 7);

        Debug.Log($"New start room: {startRoom}");

        // GeneratePath(startRoom, startRoom.exitPoint.x, startRoom.exitPoint.y, 1); // Start recursive path generation
        GeneratePath(startRoom, newStartPosition.x, newStartPosition.y, 1);
        GenAgain();
        StartCoroutine(WaitAndUpdateCamera());
    }

    public IEnumerator WaitAndUpdateCamera()
    {
        yield return new WaitForSeconds(0.01f);
        mCamera.GetComponent<CameraController>().ClearCameraPositions();
        mCamera.GetComponent<CameraController>().UpdateCameraPositions();
    }

}
