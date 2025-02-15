using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MazeGenerator : MonoBehaviour
{
    public int rows = 10; // Number of rows in the grid
    public int cols = 10; // Number of columns in the grid
    public float cellSize = 1.0f; // Size of each cell
    public float roomSize = 3.0f; // Size of each room
    public int roomHeight;
    public int roomWidth;

    public Color wallColor = Color.black; // Color for walls
    public Color pathColor = Color.white; // Color for paths

    private GameObject[,] gridObjects; // Store grid elements for modification
    private GameObject[,] expandedGridObjects; // Store expanded grid elements for modification
    private bool[,] visited; // Track visited cells
    private List<(Vector2Int, Vector2Int)> edgeList = new List<(Vector2Int, Vector2Int)>(); // Edges list

    public Vector2Int enterance;
    public Vector2Int exit;

    [Header("Tiles")]
    public TileBase wallTile;
    public TileBase floorTile;
    public TileBase platformTile;
    public TileBase pathTile;
    public Tilemap wallTilemap;
    public Tilemap pathTilemap;

    [Header("Room Stuff")]
    public GameObject lightPrefab;
    public GameObject enemyPrefab;
    public GameObject keyPrefab;

    public void DebugPath(Vector2Int start, Vector2Int target)
    {
        List<Vector2Int> path = FindPath(start, target);

        if (path.Count > 0)
        {
            Debug.Log("Path found:");
            foreach (Vector2Int step in path)
            {
                Debug.Log($"Step: {step}");
            }
        }
        else
        {
            Debug.Log("No path found.");
        }
    }


    public void ActivateMaze(Room room, Vector2Int mazeEntry, Vector2Int mazeExit)
    {
        DrawExpandedGrid(room, mazeEntry, mazeExit);
        GenerateExpandedMaze();
        StartCoroutine(PauseForShadows());
        // SpawnLights();
    }

    public IEnumerator PauseForShadows()
    {
        yield return new WaitForSeconds(0.1f);

        CreateShadowCasters();
        PopulateMaze();
    }

    void DrawExpandedGrid(Room room, Vector2Int mazeEntry, Vector2Int mazeExit)
    {
        int expandedRows = rows * 3 + (rows + 1); // 3x3 paths + 1x1 walls
        int expandedCols = cols * 3 + (cols + 1); // 3x3 paths + 1x1 walls
        // expandedGridObjects = new GameObject[expandedRows, expandedCols];

        // Offset to move the tiles to the bottom-left corner
        int offsetX = -roomWidth / 2;
        int offsetY = -roomHeight / 2;

        for (int x = 0; x < expandedCols + 3; x++) // -1 from expandedCols here removes last wall for uneven room size at right side of maze
        {
            for (int y = 0; y < expandedRows; y++)
            {
                // Determine if the current tile is a wall or part of a path
                if (x % 4 == 0 || y % 4 == 0)
                {
                    if (x < expandedCols - 1 || (y == expandedRows-1 || y == 0))
                    {
                        // Walls are on every 4th row/column
                        // Apply the offset to align the platforms with the walls and floor
                        int tileX = x + offsetX;
                        int tileY = y + offsetY;

                        wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), wallTile); // Place a path tile on the backgroun
                    }
                }
                else
                {
                    // Draw path on background
                    // int tileX = x + offsetX;
                    // int tileY = y + offsetY;

                    // pathTilemap.SetTile(new Vector3Int(tileX, tileY, 0), pathTile); // Place a path tile on the background
                }
            }
        }

        CreateEntryAndExit(room, mazeEntry, mazeExit);
        // CreateShadowCasters();
        // AddExitWallsToRoom(mazeEntry, mazeExit);
    }

    private void PopulateMaze()
    {
        int expandedRows = rows * 3 + (rows + 1); // 3x3 paths + 1x1 walls
        int expandedCols = cols * 3 + (cols + 1); // 3x3 paths + 1x1 walls

        bool spawnedKey = false;
        float enemyChance = 0.3f;
        for (int x = 0; x < expandedCols - 1; x++) // -1 from expandedCols here removes last wall for uneven room size at right side of maze
        {
            for (int y = 0; y < expandedRows; y++)
            {
                // Check if the current cell is the center of a room
                if (x % 4 == 2 && y % 4 == 2)
                {
                    // Check if this is a dead end
                    Vector2Int cell = new Vector2Int(x, y);
                    if (IsDeadEnd(cell)) // No lights on maze edge.
                    {
                        if (!spawnedKey)
                        {
                            SpawnObjectInRoom(keyPrefab, cell);
                            spawnedKey = true;
                        }
                        else if (Random.Range(0f, 1f) <= enemyChance && !IsPathBetween(cell, new Vector2Int(cell.x, cell.y - 4)))
                        {
                            SpawnEnemy(cell);
                        }
                        else
                        {
                            SpawnLight(cell); // Spawn light at dead end
                        }
                    }
                }
            }
        }
    }

    // Spawn lights helper functions
    bool IsDeadEnd(Vector2Int cell)
    {
        int accessibleNeighbors = 0;
        foreach (Vector2Int direction in new Vector2Int[] {
            new Vector2Int(0, 1),   // Up
            new Vector2Int(0, -1),  // Down
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 0)    // Right
        })
        {
            Vector2Int neighbor = cell + (direction * 4);
            if (IsValidRoom(neighbor) && IsPathBetween(cell, neighbor))
            {
                accessibleNeighbors++;
            }
        }
        return accessibleNeighbors == 1; // Dead end if only one accessible neighbor
    }

    // Check if the wall tile between current and neighbor is removed
    bool IsPathBetween(Vector2Int current, Vector2Int neighbor)
    {
        // Wall midpoint position
        Vector2Int wallPosition = (current + neighbor) / 2;

        // Offset to check correct tile position on tilemap
        wallPosition += new Vector2Int(-roomWidth / 2, -roomHeight / 2);

        bool isPath = wallTilemap.GetTile(new Vector3Int(wallPosition.x, wallPosition.y, 0)) != wallTile;

        return isPath;
    }

    private void CreateShadowCasters()
    {
        ShadowCaster2DCreator shadowCasterCreator = GetComponentInChildren<ShadowCaster2DCreator>();
        shadowCasterCreator.Create();

    }

    private void CreateEntryAndExit(Room room, Vector2Int mazeEntry, Vector2Int mazeExit)
    {
        // Determine which walls, ceiling, and floor should have tiles
        bool hasLeftEntry = mazeEntry == new Vector2Int(-1, 0);
        // bool hasRightEntry = mazeEntry == new Vector2Int(1, 0);  // No right entries
        bool hasBottomEntry = mazeEntry == new Vector2Int(0, -1);
        bool hasTopEntry = mazeEntry == new Vector2Int(0, 1);

        // bool hasLeftExit= mazeExit == new Vector2Int(-1, 0);
        bool hasRightExit = mazeExit == new Vector2Int(1, 0);
        bool hasBottomExit = mazeExit == new Vector2Int(0, -1);
        bool hasTopExit = mazeExit == new Vector2Int(0, 1);
        // Offset to move the tiles to the bottom-left corner
        int offsetX = -roomWidth / 2;
        int offsetY = -roomHeight / 2;

        // Add tiles to the tilemap based on the presence of walls/floors/ceilings
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                // Adjust the position with the offset
                int tileX = x + offsetX;
                int tileY = y + offsetY;

                // Left entry
                if (hasLeftEntry && x == 0)
                {
                    if (y > (room.entryPointOffset * 4) && y < ((room.entryPointOffset * 4) + 4))
                    {
                        wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), null);
                    }
                }
                // Bottom entry
                if (hasBottomEntry && y == 0)
                {
                    if (x > (room.entryPointOffset * 4) && x < ((room.entryPointOffset * 4) + 4))
                    {
                        wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), null);
                    }
                }

                // Top entry
                if (hasTopEntry && y == roomHeight - 2)
                {
                    if (x > (room.entryPointOffset * 4) && x < ((room.entryPointOffset * 4) + 4))
                    {
                        wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), null);
                    }
                }
                
                // Right exit
                if (hasRightExit && x == roomWidth - 1)
                {
                    if (y > (room.exitPointOffset * 4) && y < ((room.exitPointOffset * 4) + 4))
                    {
                        wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), null);
                    }
                }

                // Bottom exit
                if (hasBottomExit && y == 0)
                {
                    if (x > (room.exitPointOffset * 4) && x < ((room.exitPointOffset * 4) + 4))
                    {
                        wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), null);
                    }
                }

                // Top exit
                if (hasTopExit && y == roomHeight - 2)
                {
                    if (x > (room.exitPointOffset * 4) && x < ((room.exitPointOffset * 4) + 4))
                    {
                        wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), null);
                    }
                }
            }
        }
    }

    void GenerateExpandedMaze()
    {
        visited = new bool[cols, rows];

        // Start from a random cell
        Vector2Int startCell = new Vector2Int(Random.Range(0, cols), Random.Range(0, rows));
        // Vector2Int startCell = new Vector2Int(0, 0);
        visited[startCell.x, startCell.y] = true;

        // Add all edges (walls) of the start cell to the edge list
        AddEdgesExpanded(startCell);

        while (edgeList.Count > 0)
        {
            // Pick a random edge
            int randomIndex = Random.Range(0, edgeList.Count);
            var edge = edgeList[randomIndex];
            edgeList.RemoveAt(randomIndex);

            Vector2Int current = edge.Item1;
            Vector2Int neighbor = edge.Item2;

            // If the neighbor cell is not visited, add it to the maze
            if (!visited[neighbor.x, neighbor.y])
            {
                visited[neighbor.x, neighbor.y] = true;

                // Remove the wall between current and neighbor
                RemoveWallExpanded(current, neighbor);

                // Add the neighbor's edges to the edge list
                AddEdgesExpanded(neighbor);
            }
        }        
    }

    void AddEdgesExpanded(Vector2Int cell)
    {
        // Directions for neighbors
        foreach (Vector2Int direction in new Vector2Int[] {
            new Vector2Int(0, 1),   // Up
            new Vector2Int(0, -1),  // Down
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 0)    // Right
        })
        {
            Vector2Int neighbor = cell + direction;

            if (IsValidCell(neighbor) && !visited[neighbor.x, neighbor.y])
            {
                edgeList.Add((cell, neighbor));
            }
        }
    }

    bool IsValidCell(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < cols && cell.y >= 0 && cell.y < rows;
    }

    bool IsValidRoom(Vector2Int room)
    {
        int expandedRows = rows * 3 + (rows + 1); // 3x3 paths + 1x1 walls
        int expandedCols = cols * 3 + (cols + 1); // 3x3 paths + 1x1 walls
        return room.x >= 0 && room.x < expandedCols && room.y >= 0 && room.y < expandedRows;
    }

    void RemoveWallExpanded(Vector2Int current, Vector2Int neighbor)
    {

        // Center positions of the current and neighbor cells in the expanded grid
        Vector2Int currentCenter = new Vector2Int(current.x * 4 + 2, current.y * 4 + 2);
        Vector2Int neighborCenter = new Vector2Int(neighbor.x * 4 + 2, neighbor.y * 4 + 2);
        // Convert grid coordinates to Unity grid indices
        // int wallX = current.x * 2 + 1 + (neighbor.x - current.x);
        // int wallY = current.y * 2 + 1 + (neighbor.y - current.y);

        // Wall midpoint position
        Vector2Int wallPosition = (currentCenter + neighborCenter) / 2;
        int wallX = wallPosition.x;
        int wallY = wallPosition.y;

        // Offset to move the tiles to the bottom-left corner
        int offsetX = -roomWidth / 2;
        int offsetY = -roomHeight / 2;

        // Determine wall orientation
        if (current.x == neighbor.x) // Vertical wall
        {

            for (int x = wallX - 1; x <= wallX + 1; x++)
            {
                // Delete tile
                wallTilemap.SetTile(new Vector3Int(x + offsetX, wallY + offsetY, 0), null);
                pathTilemap.SetTile(new Vector3Int(x + offsetX, wallY + offsetY, 0), pathTile); // Place a path tile on the background
            }
        }
        else if (current.y == neighbor.y) // Horizontal wall
        {
            for (int y = wallY - 1; y <= wallY + 1; y++)
            {
                // Delete tile
                wallTilemap.SetTile(new Vector3Int(wallX + offsetX, y + offsetY, 0), null);
                pathTilemap.SetTile(new Vector3Int(wallX + offsetX, y + offsetY, 0), pathTile); // Place a path tile on the background
            }
        }
    }

    public void SpawnLight(Vector2Int room)
    {
        Vector2Int roomCenter = new Vector2Int(room.x, room.y);
        
        // Offset to move the tiles to the bottom-left corner
        int offsetX = -roomWidth / 2;
        int offsetY = -roomHeight / 2;

        // Apply offset to roomCenter
        roomCenter += new Vector2Int(offsetX, offsetY);

        GameObject lightObject = Instantiate(lightPrefab, new Vector2(roomCenter.x, roomCenter.y), Quaternion.identity, transform);
        lightObject.transform.localPosition = new Vector2(roomCenter.x, roomCenter.y);
    }

    private void SpawnEnemy(Vector2Int room)
    {
        // Apply offset to roomCenter
        room += new Vector2Int(-roomWidth / 2, -roomHeight / 2);

        GameObject enemyObject = Instantiate(enemyPrefab, new Vector2(room.x, room.y), Quaternion.identity, transform);
        enemyObject.transform.localPosition = new Vector2(room.x, room.y);
    }

    private void SpawnObjectInRoom(GameObject obj, Vector2Int room)
    {
        // Apply offset to roomCenter
        room += new Vector2Int(-roomWidth / 2, -roomHeight / 2);
        GameObject spawnObject = Instantiate(obj, new Vector2(room.x, room.y), Quaternion.identity, transform);
        spawnObject.transform.localPosition = new Vector2(room.x, room.y);
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        List<Node> openList = new List<Node>();
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        // Add the start node to the open list
        Node startNode = new Node(start, null, 0, GetHeuristic(start, target));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // Get the node with the lowest fCost
            Node currentNode = openList.OrderBy(node => node.fCost).First();

            // If the target is reached, reconstruct the path
            if (currentNode.Position == target)
            {
                return ReconstructPath(currentNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode.Position);

            // Check neighbors
            foreach (Vector2Int direction in new Vector2Int[] {
                new Vector2Int(0, 1),  // Up
                new Vector2Int(0, -1), // Down
                new Vector2Int(-1, 0), // Left
                new Vector2Int(1, 0)   // Right
            })
            {
                Vector2Int neighborPosition = currentNode.Position + (direction * 4);

                // Skip if already visited or not traversable
                if (closedList.Contains(neighborPosition) || !IsTraversable(currentNode.Position, neighborPosition))
                    continue;

                int newGCost = currentNode.gCost + 1;

                // Check if this neighbor is in the open list
                Node neighborNode = openList.FirstOrDefault(node => node.Position == neighborPosition);

                if (neighborNode == null)
                {
                    // Add the neighbor to the open list
                    neighborNode = new Node(neighborPosition, currentNode, newGCost, GetHeuristic(neighborPosition, target));
                    openList.Add(neighborNode);
                }
                else if (newGCost < neighborNode.gCost)
                {
                    // Update costs if a better path is found
                    neighborNode.gCost = newGCost;
                    neighborNode.Parent = currentNode;
                }
            }
        }

        // Return an empty path if no path is found
        return new List<Vector2Int>();
    }

    private int GetHeuristic(Vector2Int a, Vector2Int b)
    {
        // Manhattan distance
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private bool IsTraversable(Vector2Int current, Vector2Int neighbor)
    {
        // Check if the position is valid and not blocked
        return IsValidRoom(neighbor) && IsPathBetween(current, neighbor);
    }

    private bool IsWallTile(Vector2Int position)
    {
        // Check if a tile at the position is a wall
        TileBase tile = wallTilemap.GetTile(new Vector3Int(position.x, position.y, 0));
        return tile == wallTile;
    }

    private List<Vector2Int> ReconstructPath(Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node current = endNode;

        while (current != null)
        {
            path.Add(current.Position);
            current = current.Parent;
        }

        path.Reverse(); // Reverse the path to start from the beginning
        return path;
    }

   
}

// Node class for A* search
public class Node
{
    public Vector2Int Position;
    public Node Parent;
    public int gCost, hCost;
    public int fCost => gCost + hCost;

    public Node(Vector2Int position, Node parent, int gCost, int hCost)
    {
        Position = position;
        Parent = parent;
        this.gCost = gCost;
        this.hCost = hCost;
    }
}