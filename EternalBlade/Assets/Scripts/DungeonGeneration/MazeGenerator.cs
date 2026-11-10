using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace MazeDungeon
{
    /// <summary>Where a spawn rule is allowed to place its prefab.</summary>
    public enum MazeSpawnLocation
    {
        DeadEnd,
        AnyCell
    }

    /// <summary>Which tiles get painted on the optional background (path) tilemap.</summary>
    public enum MazePathPaintMode
    {
        None,
        /// <summary>Only the tiles of carved passages between cells.</summary>
        PassagesOnly,
        /// <summary>Every non-wall tile in the maze.</summary>
        AllWalkableTiles
    }

    /// <summary>
    /// Describes one kind of object (enemy, pickup, turret, ...) to scatter through a maze.
    /// Chance = baseChance + chanceIncreasePerRoom * (room.id - 1), clamped to 0..1.
    /// </summary>
    [System.Serializable]
    public class MazeSpawnRule
    {
        [Tooltip("Label for your own reference. Not used at runtime.")]
        public string label = "New Rule";
        public GameObject prefab;
        public MazeSpawnLocation location = MazeSpawnLocation.DeadEnd;
        [Range(0f, 1f)] public float baseChance = 1f;
        [Range(0f, 1f)] public float chanceIncreasePerRoom = 0f;
        [Tooltip("Maximum spawns per maze. 0 = unlimited.")]
        [Min(0)] public int maxPerRoom = 0;
        [Tooltip("Only spawn on cells that have a solid wall directly below them.")]
        public bool requireFloorBelow = false;
        [Tooltip("Never spawn on the cell next to the room's entry or exit opening.")]
        public bool avoidEntryAndExit = true;
        [Tooltip("If true, later rules cannot spawn on a cell this rule used.")]
        public bool claimsCell = true;
    }

    internal static class MazeListUtil
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }

    /// <summary>
    /// Generates a randomized (Prim's algorithm) tile-based maze inside a room.
    /// Each maze cell is 3x3 tiles with 1-tile walls between cells, so the room is
    /// (cols * 4 + 1) x (rows * 4 + 1) tiles.
    /// </summary>
    [DisallowMultipleComponent]
    public class MazeGenerator : MonoBehaviour
    {
        /// <summary>Tiles per cell including one wall.</summary>
        public const int CellStride = 4;
        /// <summary>Walkable tiles across one cell.</summary>
        public const int PathSize = 3;

        [Header("Maze Size (in cells)")]
        [Min(1)] public int cols = 9;
        [Min(1)] public int rows = 5;

        [Header("Tiles")]
        public TileBase wallTile;
        public TileBase pathTile;
        public Tilemap wallTilemap;
        [Tooltip("Optional background tilemap.")]
        public Tilemap pathTilemap;
        public MazePathPaintMode pathPaintMode = MazePathPaintMode.PassagesOnly;

        [Header("Population")]
        [Tooltip("Optional. Spawned at every dead end.")]
        public GameObject lightPrefab;
        public List<MazeSpawnRule> spawnRules = new List<MazeSpawnRule>();
        [Tooltip("Added to every spawned object's local position (cell centre by default).")]
        public Vector2 spawnOffset;
        [Tooltip("Seconds to wait after drawing the maze before firing 'On Maze Built' and spawning objects.")]
        [Min(0f)] public float postBuildDelay = 0.1f;

        [Header("Events")]
        [Tooltip("Fires once the walls are drawn, before objects are spawned. Hook shadow caster generation, composite colliders, etc. here.")]
        public UnityEvent onMazeBuilt;
        [Tooltip("Fires after all objects have been spawned.")]
        public UnityEvent onMazePopulated;

        /// <summary>True once the maze has been built and populated.</summary>
        public bool IsPopulated { get; private set; }

        public int TileWidth => cols * CellStride + 1;
        public int TileHeight => rows * CellStride + 1;

        /// <summary>Tile coordinate of the bottom-left tile; the maze is centred on the tilemap origin.</summary>
        public Vector2Int TileOrigin => new Vector2Int(-TileWidth / 2, -TileHeight / 2);

        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        private struct Edge
        {
            public Vector2Int from;
            public Vector2Int to;

            public Edge(Vector2Int from, Vector2Int to)
            {
                this.from = from;
                this.to = to;
            }
        }

        // Passage data: eastOpen[x,y] = open between (x,y) and (x+1,y); northOpen[x,y] = open between (x,y) and (x,y+1).
        private bool[,] eastOpen;
        private bool[,] northOpen;

        // ---------------------------------------------------------------- Public API

        /// <summary>Builds the maze for the given room, using its entry/exit data.</summary>
        public void ActivateMaze(Room room)
        {
            if (room == null)
            {
                Debug.LogError("MazeGenerator.ActivateMaze: room is null.", this);
                return;
            }
            if (wallTilemap == null || wallTile == null)
            {
                Debug.LogError("MazeGenerator: 'wallTilemap' and 'wallTile' must be assigned.", this);
                return;
            }

            StopAllCoroutines();
            IsPopulated = false;

            DrawGrid();
            GenerateMaze();
            OpenBorder(room.entryPoint, room.entryPointOffset);
            OpenBorder(room.exitPoint, room.exitPointOffset);

            StartCoroutine(BuildRoutine(room));
        }

        /// <summary>True if there is an open passage between two adjacent cells (cell coordinates, not tiles).</summary>
        public bool HasPassage(Vector2Int a, Vector2Int b)
        {
            if (eastOpen == null || !IsValidCell(a) || !IsValidCell(b)) return false;

            if (a.y == b.y && Mathf.Abs(a.x - b.x) == 1)
                return eastOpen[Mathf.Min(a.x, b.x), a.y];
            if (a.x == b.x && Mathf.Abs(a.y - b.y) == 1)
                return northOpen[a.x, Mathf.Min(a.y, b.y)];
            return false;
        }

        /// <summary>Cells (cell coordinates) with exactly one open passage.</summary>
        public List<Vector2Int> GetDeadEnds()
        {
            var result = new List<Vector2Int>();
            if (eastOpen == null) return result;

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    var cell = new Vector2Int(x, y);
                    if (CountPassages(cell) == 1) result.Add(cell);
                }
            }
            return result;
        }

        /// <summary>Local position of a cell's centre, relative to this object.</summary>
        public Vector2 CellToLocal(Vector2Int cell)
        {
            Vector2Int origin = TileOrigin;
            return new Vector2(origin.x + cell.x * CellStride + CellStride / 2,
                               origin.y + cell.y * CellStride + CellStride / 2);
        }

        /// <summary>Shortest path between two cells (cell coordinates, inclusive). Empty if none exists.</summary>
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
        {
            var path = new List<Vector2Int>();
            if (eastOpen == null || !IsValidCell(start) || !IsValidCell(target)) return path;

            var cameFrom = new Dictionary<Vector2Int, Vector2Int> { [start] = start };
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                if (current == target) break;

                foreach (Vector2Int dir in Directions)
                {
                    Vector2Int next = current + dir;
                    if (cameFrom.ContainsKey(next) || !HasPassage(current, next)) continue;
                    cameFrom[next] = current;
                    queue.Enqueue(next);
                }
            }

            if (!cameFrom.ContainsKey(target)) return path;

            Vector2Int step = target;
            while (step != start)
            {
                path.Add(step);
                step = cameFrom[step];
            }
            path.Add(start);
            path.Reverse();
            return path;
        }

        // ---------------------------------------------------------------- Building

        private IEnumerator BuildRoutine(Room room)
        {
            yield return null;
            if (postBuildDelay > 0f) yield return new WaitForSeconds(postBuildDelay);

            onMazeBuilt?.Invoke();
            Populate(room);

            IsPopulated = true;
            onMazePopulated?.Invoke();
        }

        private void DrawGrid()
        {
            Vector2Int origin = TileOrigin;
            int width = TileWidth;
            int height = TileHeight;
            bool paintAll = pathPaintMode == MazePathPaintMode.AllWalkableTiles && pathTilemap != null;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pos = new Vector3Int(x + origin.x, y + origin.y, 0);

                    if (x % CellStride == 0 || y % CellStride == 0)
                        wallTilemap.SetTile(pos, wallTile);
                    else if (paintAll)
                        pathTilemap.SetTile(pos, pathTile);
                }
            }
        }

        /// <summary>Clears the 3-tile opening on the given border (direction relative to the room).</summary>
        private void OpenBorder(Vector2Int direction, int offsetCells)
        {
            if (direction == Vector2Int.zero) return;

            Vector2Int origin = TileOrigin;
            int start = offsetCells * CellStride;

            for (int i = 1; i <= PathSize; i++)
            {
                int along = start + i;
                int x = direction.x > 0 ? TileWidth - 1 : direction.x < 0 ? 0 : along;
                int y = direction.y > 0 ? TileHeight - 1 : direction.y < 0 ? 0 : along;
                wallTilemap.SetTile(new Vector3Int(x + origin.x, y + origin.y, 0), null);
            }
        }

        // Randomized Prim's algorithm over the cell grid.
        private void GenerateMaze()
        {
            eastOpen = new bool[cols, rows];
            northOpen = new bool[cols, rows];

            var visited = new bool[cols, rows];
            var frontier = new List<Edge>();

            var startCell = new Vector2Int(Random.Range(0, cols), Random.Range(0, rows));
            visited[startCell.x, startCell.y] = true;
            AddFrontier(startCell, visited, frontier);

            while (frontier.Count > 0)
            {
                int index = Random.Range(0, frontier.Count);
                Edge edge = frontier[index];
                int last = frontier.Count - 1;
                frontier[index] = frontier[last];
                frontier.RemoveAt(last);

                if (visited[edge.to.x, edge.to.y]) continue;

                visited[edge.to.x, edge.to.y] = true;
                CarvePassage(edge.from, edge.to);
                AddFrontier(edge.to, visited, frontier);
            }
        }

        private void AddFrontier(Vector2Int cell, bool[,] visited, List<Edge> frontier)
        {
            foreach (Vector2Int dir in Directions)
            {
                Vector2Int neighbor = cell + dir;
                if (IsValidCell(neighbor) && !visited[neighbor.x, neighbor.y])
                    frontier.Add(new Edge(cell, neighbor));
            }
        }

        private void CarvePassage(Vector2Int a, Vector2Int b)
        {
            if (a.x != b.x) eastOpen[Mathf.Min(a.x, b.x), a.y] = true;
            else northOpen[a.x, Mathf.Min(a.y, b.y)] = true;

            // Tile at the centre of the wall between the two cells.
            int midX = (a.x + b.x) * CellStride / 2 + CellStride / 2;
            int midY = (a.y + b.y) * CellStride / 2 + CellStride / 2;
            int half = PathSize / 2;

            if (a.x == b.x) // neighbours stacked vertically: the wall is a horizontal strip
            {
                for (int x = midX - half; x <= midX + half; x++) OpenWallTile(x, midY);
            }
            else // neighbours side by side: the wall is a vertical strip
            {
                for (int y = midY - half; y <= midY + half; y++) OpenWallTile(midX, y);
            }
        }

        private void OpenWallTile(int x, int y)
        {
            Vector2Int origin = TileOrigin;
            var pos = new Vector3Int(x + origin.x, y + origin.y, 0);

            wallTilemap.SetTile(pos, null);
            if (pathPaintMode != MazePathPaintMode.None && pathTilemap != null)
                pathTilemap.SetTile(pos, pathTile);
        }

        // ---------------------------------------------------------------- Population

        private void Populate(Room room)
        {
            List<Vector2Int> deadEnds = GetDeadEnds();

            if (lightPrefab != null)
            {
                foreach (Vector2Int cell in deadEnds) Spawn(lightPrefab, cell);
            }

            if (spawnRules == null) return;

            var claimed = new HashSet<Vector2Int>();
            var candidates = new List<Vector2Int>();

            foreach (MazeSpawnRule rule in spawnRules)
            {
                if (rule == null || rule.prefab == null) continue;

                candidates.Clear();
                if (rule.location == MazeSpawnLocation.DeadEnd)
                {
                    candidates.AddRange(deadEnds);
                }
                else
                {
                    for (int x = 0; x < cols; x++)
                        for (int y = 0; y < rows; y++)
                            candidates.Add(new Vector2Int(x, y));
                }
                candidates.Shuffle();

                float chance = Mathf.Clamp01(rule.baseChance + rule.chanceIncreasePerRoom * Mathf.Max(0, room.id - 1));
                int spawnedCount = 0;

                foreach (Vector2Int cell in candidates)
                {
                    if (rule.maxPerRoom > 0 && spawnedCount >= rule.maxPerRoom) break;
                    if (claimed.Contains(cell)) continue;
                    if (rule.avoidEntryAndExit && IsPortalCell(room, cell)) continue;
                    if (rule.requireFloorBelow && !HasFloorBelow(room, cell)) continue;
                    if (chance <= 0f || Random.value > chance) continue;

                    Spawn(rule.prefab, cell);
                    spawnedCount++;
                    if (rule.claimsCell) claimed.Add(cell);
                }
            }
        }

        private GameObject Spawn(GameObject prefab, Vector2Int cell)
        {
            GameObject spawned = Instantiate(prefab, transform);
            Vector2 local = CellToLocal(cell) + spawnOffset;
            spawned.transform.localPosition = new Vector3(local.x, local.y, spawned.transform.localPosition.z);
            return spawned;
        }

        private int CountPassages(Vector2Int cell)
        {
            int count = 0;
            foreach (Vector2Int dir in Directions)
            {
                if (HasPassage(cell, cell + dir)) count++;
            }
            return count;
        }

        private bool HasFloorBelow(Room room, Vector2Int cell)
        {
            if (cell.y > 0)
                return !HasPassage(cell, cell + Vector2Int.down);

            // Bottom row: solid unless an entry/exit opening is directly below this cell.
            bool entryBelow = room.entryPoint == Vector2Int.down && room.entryPointOffset == cell.x;
            bool exitBelow = room.exitPoint == Vector2Int.down && room.exitPointOffset == cell.x;
            return !entryBelow && !exitBelow;
        }

        private bool IsPortalCell(Room room, Vector2Int cell)
        {
            return MatchesPortal(room.entryPoint, room.entryPointOffset, cell)
                || MatchesPortal(room.exitPoint, room.exitPointOffset, cell);
        }

        private bool MatchesPortal(Vector2Int direction, int offset, Vector2Int cell)
        {
            if (direction == Vector2Int.zero) return false;
            if (direction.x < 0) return cell.x == 0 && cell.y == offset;
            if (direction.x > 0) return cell.x == cols - 1 && cell.y == offset;
            if (direction.y < 0) return cell.y == 0 && cell.x == offset;
            return cell.y == rows - 1 && cell.x == offset;
        }

        private bool IsValidCell(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < cols && cell.y >= 0 && cell.y < rows;
        }
    }
}