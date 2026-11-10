using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace MazeDungeon
{
    /// <summary>
    /// Generates a chain of connected rooms (start room, maze rooms, final room) on a grid using
    /// recursive backtracking. The path only moves up, right or down.
    /// Call <see cref="Generate"/> for the first level and <see cref="Regenerate"/> to continue from the final room.
    /// </summary>
    public class RecursiveGenerator : MonoBehaviour
    {
        [Header("World Grid (in rooms)")]
        [Min(1)] public int width = 10;
        [Min(1)] public int height = 10;
        [Tooltip("Rooms per generated path. X = minimum (inclusive), Y = maximum (exclusive).")]
        public Vector2Int roomNumRange = new Vector2Int(4, 7);
        [Tooltip("How many times to retry if a random path gets boxed in.")]
        [Min(1)] public int maxGenerationAttempts = 25;

        [Header("Room Prefabs")]
        [Tooltip("Middle rooms. Must have a MazeGenerator component; its cols/rows define the room size.")]
        public GameObject roomPrefab;
        public GameObject startRoomPrefab;
        public GameObject finalRoomPrefab;

        [Header("Tiles")]
        public TileBase wallTile;
        public TileBase floorTile;
        public TileBase vertDoorTile;
        public TileBase horiDoorTile;

        [Header("Door Effects (optional)")]
        public GameObject doorLightPrefab;
        public GameObject doorParticles;
        [Tooltip("Seconds between each door tile disappearing.")]
        [Min(0f)] public float doorOpenStepDelay = 0.55f;

        [Header("Events")]
        [Tooltip("Fires after all rooms are spawned and every maze has finished populating.")]
        public UnityEvent onGenerationComplete;

        /// <summary>Code-friendly version of <see cref="onGenerationComplete"/>.</summary>
        public event System.Action GenerationComplete;

        /// <summary>Number of rooms in the current path.</summary>
        public int NumRooms { get; private set; }

        /// <summary>True while rooms are being spawned and mazes are still populating.</summary>
        public bool IsGenerating { get; private set; }

        /// <summary>Room size in tiles, taken from the middle room's MazeGenerator.</summary>
        public int RoomWidth => Template != null ? Template.TileWidth : 0;
        public int RoomHeight => Template != null ? Template.TileHeight : 0;

        private Dictionary<Vector2Int, Room> roomMap = new Dictionary<Vector2Int, Room>();
        private readonly List<GameObject> spawnedRooms = new List<GameObject>();
        private readonly List<MazeGenerator> activeMazes = new List<MazeGenerator>();

        // Lower-left corner of the allowed room window for the current path.
        private Vector2Int boundsMin;
        private MazeGenerator template;

        private MazeGenerator Template
        {
            get
            {
                if (template == null && roomPrefab != null)
                    template = roomPrefab.GetComponent<MazeGenerator>();
                return template;
            }
        }

        private void OnValidate()
        {
            template = null;
        }

        // ---------------------------------------------------------------- Public API

        public Dictionary<Vector2Int, Room> GetRoomMap()
        {
            return roomMap;
        }

        [ContextMenu("Log Room Map")]
        public void DebugRoomMap()
        {
            foreach (KeyValuePair<Vector2Int, Room> entry in roomMap)
                Debug.Log($"Position: {entry.Key}, {entry.Value}", this);
        }

        /// <summary>Destroys everything and generates a fresh path starting at the grid origin.</summary>
        public void Generate()
        {
            if (!Validate()) return;

            Clear();

            var startRoom = new Room(0, Vector2Int.zero) { exitPoint = Vector2Int.right };
            startRoom.exitPointOffset = RandomPortalOffset(startRoom.exitPoint);
            boundsMin = Vector2Int.zero;

            if (!TryGeneratePath(startRoom)) return;

            SpawnRooms();
            BeginCompletionWatch();
        }

        /// <summary>
        /// Builds a new path that continues from the current final room. The old final room is kept
        /// alive (so the player isn't left without geometry) and becomes the new start; every
        /// other old room is destroyed and the coordinate system is re-centred on the carried-over room.
        /// Falls back to <see cref="Generate"/> if nothing has been generated yet.
        /// </summary>
        public void Regenerate()
        {
            Room carried = GetFinalRoom();
            if (carried == null)
            {
                Generate();
                return;
            }
            if (!Validate()) return;

            // Snapshot so a failed attempt leaves the current level untouched.
            Dictionary<Vector2Int, Room> oldMap = roomMap;
            Vector2Int oldPosition = carried.position;
            Room oldPrev = carried.prevRoom;

            roomMap = new Dictionary<Vector2Int, Room>();
            carried.position = Vector2Int.zero;
            carried.prevRoom = null;
            carried.nextRoom = null;
            boundsMin = ComputeBoundsMin(carried.exitPoint);

            if (!TryGeneratePath(carried))
            {
                roomMap = oldMap;
                carried.position = oldPosition;
                carried.prevRoom = oldPrev;
                return;
            }

            StopAllCoroutines();

            GameObject keep = carried.instance;
            DestroyRoomsExcept(keep);
            if (keep != null) keep.transform.localPosition = Vector3.zero;

            SpawnRooms();
            BeginCompletionWatch();
        }

        /// <summary>Destroys every spawned room and forgets the current path.</summary>
        public void Clear()
        {
            StopAllCoroutines();
            IsGenerating = false;

            foreach (GameObject go in spawnedRooms)
            {
                if (go != null) Destroy(go);
            }
            spawnedRooms.Clear();
            activeMazes.Clear();
            roomMap.Clear();
        }

        /// <summary>Opens the exit door of the room at the given grid position.</summary>
        public void ClearRoomExitDoor(Vector2Int roomPosition)
        {
            if (roomMap.TryGetValue(roomPosition, out Room room))
                ClearRoomExitDoor(room);
            else
                Debug.LogWarning($"RecursiveGenerator: no room at {roomPosition}.", this);
        }

        /// <summary>Opens the exit door of the given room.</summary>
        public void ClearRoomExitDoor(Room room)
        {
            if (room == null || room.instance == null || !room.HasExit) return;

            Tilemap tilemap = GetWallTilemap(room.instance);
            if (tilemap == null) return;

            StartCoroutine(AnimateExitDoor(room, tilemap));
        }

        // ---------------------------------------------------------------- Path generation

        private bool Validate()
        {
            if (roomPrefab == null || Template == null)
            {
                Debug.LogError("RecursiveGenerator: 'roomPrefab' must be assigned and have a MazeGenerator component.", this);
                return false;
            }
            if (startRoomPrefab == null || finalRoomPrefab == null)
            {
                Debug.LogError("RecursiveGenerator: 'startRoomPrefab' and 'finalRoomPrefab' must be assigned.", this);
                return false;
            }
            return true;
        }

        private bool TryGeneratePath(Room startRoom)
        {
            for (int attempt = 0; attempt < maxGenerationAttempts; attempt++)
            {
                roomMap.Clear();
                startRoom.nextRoom = null;
                roomMap[startRoom.position] = startRoom;

                NumRooms = Mathf.Max(2, Random.Range(roomNumRange.x, Mathf.Max(roomNumRange.x, roomNumRange.y)));

                if (GeneratePath(startRoom, startRoom.position + startRoom.exitPoint, 1))
                    return true;
            }

            roomMap.Clear();
            Debug.LogWarning($"RecursiveGenerator: couldn't fit a path after {maxGenerationAttempts} attempts. " +
                             "Try a larger grid or fewer rooms.", this);
            return false;
        }

        private bool GeneratePath(Room previousRoom, Vector2Int position, int roomsCreated)
        {
            if (roomsCreated >= NumRooms) return true;

            var currentRoom = new Room(roomsCreated, position)
            {
                prevRoom = previousRoom,
                entryPoint = new Vector2Int(-previousRoom.exitPoint.x, -previousRoom.exitPoint.y),
                entryPointOffset = previousRoom.exitPointOffset
            };
            previousRoom.nextRoom = currentRoom;
            roomMap[position] = currentRoom;

            var directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.right, Vector2Int.down };
            directions.Shuffle();

            foreach (Vector2Int direction in directions)
            {
                Vector2Int nextPosition = position + direction;
                if (!InBounds(nextPosition) || roomMap.ContainsKey(nextPosition)) continue;

                // The final room also gets an exit; it points at the spot the next level will start from.
                currentRoom.exitPoint = direction;
                currentRoom.exitPointOffset = RandomPortalOffset(direction);

                if (GeneratePath(currentRoom, nextPosition, roomsCreated + 1))
                    return true;
            }

            // Dead end: undo this room so failed branches don't leave ghosts behind.
            roomMap.Remove(position);
            previousRoom.nextRoom = null;
            return false;
        }

        private bool InBounds(Vector2Int p)
        {
            return p.x >= boundsMin.x && p.x < boundsMin.x + width
                && p.y >= boundsMin.y && p.y < boundsMin.y + height;
        }

        // Places the room window so the first new room (carried room at origin + exit direction) is inside it.
        private Vector2Int ComputeBoundsMin(Vector2Int exitDirection)
        {
            int minX = exitDirection.x != 0 ? 0 : -(width / 2);
            int minY = exitDirection.y > 0 ? 0 : exitDirection.y < 0 ? -(height - 1) : -(height / 2);
            return new Vector2Int(minX, minY);
        }

        // Vertical exits can sit on any of the maze's columns, horizontal exits on any of its rows.
        private int RandomPortalOffset(Vector2Int direction)
        {
            int cells = direction.x == 0 ? Template.cols : Template.rows;
            return Random.Range(0, cells);
        }

        private Room GetFinalRoom()
        {
            foreach (Room room in roomMap.Values)
            {
                if (room.IsFinalRoom && !room.IsStartRoom) return room;
            }
            return null;
        }

        // ---------------------------------------------------------------- Spawning

        private void SpawnRooms()
        {
            IsGenerating = true;
            activeMazes.Clear();

            int roomWidth = RoomWidth;
            int roomHeight = RoomHeight;

            foreach (Room room in roomMap.Values)
            {
                bool isMiddle = !room.IsStartRoom && !room.IsFinalRoom;
                GameObject prefab = room.IsStartRoom ? startRoomPrefab : room.IsFinalRoom ? finalRoomPrefab : roomPrefab;

                GameObject instance = Instantiate(prefab, transform);
                instance.transform.localPosition = new Vector3(room.position.x * roomWidth, room.position.y * roomHeight, 0f);
                room.SetInstance(instance);
                spawnedRooms.Add(instance);

                if (isMiddle)
                {
                    MazeGenerator maze = instance.GetComponent<MazeGenerator>();
                    if (maze == null)
                    {
                        Debug.LogError("RecursiveGenerator: roomPrefab instance has no MazeGenerator.", instance);
                        continue;
                    }
                    maze.ActivateMaze(room);
                    activeMazes.Add(maze);
                }

                Tilemap tilemap = GetWallTilemap(instance);
                if (tilemap == null)
                {
                    Debug.LogError($"RecursiveGenerator: no Tilemap found on {instance.name}.", instance);
                    continue;
                }

                AddBorderTiles(room, tilemap);
                AddExitWalls(room, tilemap);
            }
        }

        private void DestroyRoomsExcept(GameObject keep)
        {
            for (int i = spawnedRooms.Count - 1; i >= 0; i--)
            {
                GameObject go = spawnedRooms[i];
                if (go == keep) continue;

                if (go != null) Destroy(go);
                spawnedRooms.RemoveAt(i);
            }
        }

        private static Tilemap GetWallTilemap(GameObject roomInstance)
        {
            MazeGenerator maze = roomInstance.GetComponent<MazeGenerator>();
            if (maze != null && maze.wallTilemap != null) return maze.wallTilemap;
            return roomInstance.GetComponentInChildren<Tilemap>();
        }

        private void BeginCompletionWatch()
        {
            StartCoroutine(NotifyWhenReady());
        }

        private IEnumerator NotifyWhenReady()
        {
            yield return null;
            while (!AllMazesPopulated()) yield return null;

            IsGenerating = false;
            onGenerationComplete?.Invoke();
            GenerationComplete?.Invoke();
        }

        private bool AllMazesPopulated()
        {
            foreach (MazeGenerator maze in activeMazes)
            {
                if (maze != null && !maze.IsPopulated) return false;
            }
            return true;
        }

        // ---------------------------------------------------------------- Tiles & doors

        private Vector2Int TileOrigin => new Vector2Int(-RoomWidth / 2, -RoomHeight / 2);

        // Tilemap cell on the given border at a position along it (direction is relative to the room).
        private Vector3Int BorderCell(Vector2Int direction, int along)
        {
            Vector2Int origin = TileOrigin;
            int x = direction.x > 0 ? RoomWidth - 1 : direction.x < 0 ? 0 : along;
            int y = direction.y > 0 ? RoomHeight - 1 : direction.y < 0 ? 0 : along;
            return new Vector3Int(x + origin.x, y + origin.y, 0);
        }

        // Solid walls on every side that has no entry or exit.
        private void AddBorderTiles(Room room, Tilemap tilemap)
        {
            bool hasLeftWall = room.entryPoint != Vector2Int.left && room.exitPoint != Vector2Int.left;
            bool hasRightWall = room.entryPoint != Vector2Int.right && room.exitPoint != Vector2Int.right;
            bool hasBottomWall = room.entryPoint != Vector2Int.down && room.exitPoint != Vector2Int.down;
            bool hasTopWall = room.entryPoint != Vector2Int.up && room.exitPoint != Vector2Int.up;

            int w = RoomWidth;
            int h = RoomHeight;
            Vector2Int origin = TileOrigin;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (x != 0 && x != w - 1 && y != 0 && y != h - 1) continue; // interior

                    var pos = new Vector3Int(x + origin.x, y + origin.y, 0);

                    if (hasLeftWall && x == 0) tilemap.SetTile(pos, wallTile);
                    if (hasRightWall && x == w - 1) tilemap.SetTile(pos, wallTile);
                    if (hasBottomWall && y == 0) tilemap.SetTile(pos, floorTile);
                    if (hasTopWall && y == h - 1) tilemap.SetTile(pos, floorTile);
                }
            }
        }

        // Walls along the exit side with a closed 3-tile door in the gap.
        private void AddExitWalls(Room room, Tilemap tilemap)
        {
            Vector2Int direction = room.exitPoint;
            if (direction == Vector2Int.zero) return;

            bool vertical = direction.x != 0;
            int length = vertical ? RoomHeight : RoomWidth;
            int gapStart = room.exitPointOffset * MazeGenerator.CellStride;
            TileBase solidTile = vertical ? wallTile : floorTile;
            TileBase doorTile = vertical ? vertDoorTile : horiDoorTile;

            for (int along = 0; along < length; along++)
            {
                bool inGap = along > gapStart && along < gapStart + MazeGenerator.CellStride;
                Vector3Int cell = BorderCell(direction, along);
                tilemap.SetTile(cell, inGap ? doorTile : solidTile);

                // One light at the centre of the door.
                if (inGap && doorLightPrefab != null && along % MazeGenerator.CellStride == 2)
                {
                    Vector3 worldPos = room.instance.transform.position + new Vector3(cell.x, cell.y, 0f);
                    Instantiate(doorLightPrefab, worldPos, Quaternion.identity, room.instance.transform);
                }
            }
        }

        private IEnumerator AnimateExitDoor(Room room, Tilemap tilemap)
        {
            Vector2Int direction = room.exitPoint;

            if (doorParticles != null)
            {
                Vector2 local = GetDoorParticleLocalPosition(room);
                Vector3 worldPos = room.instance.transform.position + new Vector3(local.x, local.y, 0f);
                Instantiate(doorParticles, worldPos, Quaternion.Euler(90f, 0f, 0f), room.instance.transform);
            }

            // Remove the three door tiles one at a time.
            for (int i = 1; i <= MazeGenerator.PathSize; i++)
            {
                int along = room.exitPointOffset * MazeGenerator.CellStride + i;
                tilemap.SetTile(BorderCell(direction, along), null);
                yield return new WaitForSeconds(doorOpenStepDelay);
            }
        }

        // Offsets tuned for a particle system rotated 90 degrees on X; adjust to suit your own effect prefab.
        private Vector2 GetDoorParticleLocalPosition(Room room)
        {
            Vector2Int origin = TileOrigin;
            Vector2Int direction = room.exitPoint;
            int offset = room.exitPointOffset * MazeGenerator.CellStride;

            if (direction.y > 0) return new Vector2(offset + MazeGenerator.CellStride + origin.x, RoomHeight + origin.y);
            if (direction.y < 0) return new Vector2(offset + MazeGenerator.CellStride + origin.x, origin.y);
            if (direction.x < 0) return new Vector2(origin.x + 0.775f, offset + MazeGenerator.CellStride + 0.5f + origin.y);
            return new Vector2(RoomWidth - 0.775f + origin.x, offset + MazeGenerator.CellStride + 0.5f + origin.y);
        }
    }
}