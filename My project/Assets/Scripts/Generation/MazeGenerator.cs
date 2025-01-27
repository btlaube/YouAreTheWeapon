using System.Collections;
using System.Collections.Generic;
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

    void Awake()
    {
        // wallTilemap = GetComponent<Tilemap>();
    }

    public void ActivateMaze(Vector2Int mazeEntry, Vector2Int mazeExit)
    {
        DrawExpandedGrid(mazeEntry, mazeExit);
        GenerateExpandedMaze();
        StartCoroutine(PauseForShadows());
        SpawnLights();
    }

    public IEnumerator PauseForShadows()
    {
        yield return new WaitForSeconds(0.1f);

        CreateShadowCasters();
    }

    void DrawExpandedGrid(Vector2Int mazeEntry, Vector2Int mazeExit)
    {
        int expandedRows = rows * 3 + (rows + 1); // 3x3 paths + 1x1 walls
        int expandedCols = cols * 3 + (cols + 1); // 3x3 paths + 1x1 walls
        // expandedGridObjects = new GameObject[expandedRows, expandedCols];

        // Offset to move the tiles to the bottom-left corner
        int offsetX = -roomWidth / 2;
        int offsetY = -roomHeight / 2;

        for (int x = 0; x < expandedCols - 1; x++) // -1 from expandedCols here removes last wall for uneven room size at right side of maze
        {
            for (int y = 0; y < expandedRows; y++)
            {
                // Determine if the current tile is a wall or part of a path
                if (x % 4 == 0 || y % 4 == 0)
                {
                    // Walls are on every 4th row/column
                    // Apply the offset to align the platforms with the walls and floor
                    int tileX = x + offsetX;
                    int tileY = y + offsetY;

                    wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), wallTile); // Place a path tile on the background
                }
                else
                {
                    // Draw path on background
                    int tileX = x + offsetX;
                    int tileY = y + offsetY;

                    pathTilemap.SetTile(new Vector3Int(tileX, tileY, 0), pathTile); // Place a path tile on the background
                }
            }
        }

        CreateEntryAndExit(mazeEntry, mazeExit);
        // CreateShadowCasters();
        // AddExitWallsToRoom(mazeEntry, mazeExit);
    }

    private void SpawnLights()
    {
        int expandedRows = rows * 3 + (rows + 1); // 3x3 paths + 1x1 walls
        int expandedCols = cols * 3 + (cols + 1); // 3x3 paths + 1x1 walls
        for (int x = 0; x < expandedCols - 1; x++) // -1 from expandedCols here removes last wall for uneven room size at right side of maze
        {
            for (int y = 0; y < expandedRows; y++)
            {
                // Check if the current cell is the center of a room
                if (x % 4 == 2 && y % 4 == 2)
                {
                    Debug.Log($"Room Center at: ({x},{y})");
                    // Check if this is a dead end
                    Vector2Int cell = new Vector2Int(x, y);
                    if (IsDeadEnd(cell)) // No lights on maze edge.
                    {
                        SpawnLight(cell); // Spawn light at dead end
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
            new Vector2Int(0, 4),   // Up
            new Vector2Int(0, -4),  // Down
            new Vector2Int(-4, 0),  // Left
            new Vector2Int(4, 0)    // Right
        })
        {
            Vector2Int neighbor = cell + direction;
            if (IsPathBetween(cell, neighbor))
            {
                accessibleNeighbors++;
            }
        }
        return accessibleNeighbors == 1; // Dead end if only one accessible neighbor
    }

    // Check if the wall tile between current and neighbor is removed
    bool IsPathBetween(Vector2Int current, Vector2Int neighbor)
    {
        // Center positions of the current and neighbor cells in the expanded grid
        // Vector2Int currentCenter = new Vector2Int(current.x * 4 + 2, current.y * 4 + 2);
        // Vector2Int neighborCenter = new Vector2Int(neighbor.x * 4 + 2, neighbor.y * 4 + 2);

        // Wall midpoint position
        Vector2Int wallPosition = (current + neighbor) / 2;
        
        return pathTilemap.GetTile(new Vector3Int(wallPosition.x, wallPosition.y, 0)) == pathTile;
    }

    private void CreateShadowCasters()
    {
        ShadowCaster2DCreator shadowCasterCreator = GetComponentInChildren<ShadowCaster2DCreator>();
        shadowCasterCreator.Create();

    }

    private void CreateEntryAndExit(Vector2Int mazeEntry, Vector2Int mazeExit)
    {
        // Debug.Log($"Creating entry: {mazeEntry} and exit: {mazeExit}");
        // Determine which walls, ceiling, and floor should have tiles
        bool hasLeftEntry = mazeEntry == new Vector2Int(-1, 0);
        // bool hasRightEntry = mazeEntry == new Vector2Int(1, 0);  // No right entries
        bool hasBottomEntry = mazeEntry == new Vector2Int(0, -1);
        bool hasTopEntry = mazeEntry == new Vector2Int(0, 1);

        // bool hasLeftExit= mazeExit == new Vector2Int(-1, 0);
        bool hasRightExit = mazeExit == new Vector2Int(1, 0);
        bool hasBottomExit = mazeExit == new Vector2Int(0, -1);
        bool hasTopExit = mazeExit == new Vector2Int(0, 1);

        // Debug.Log($"Has hasLeftEntry: {hasLeftEntry} and hasRightExit: {hasRightExit}");


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
                if (hasLeftEntry && x == 0 && y > 0 && y < 4)
                    wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), null);
                    pathTilemap.SetTile(new Vector3Int(tileX, tileY, 0), pathTile); // Place a path tile on the background

                // Bottom entry
                // if (hasBottomEntry && y == 0 && x > roomWidth - 4 && x < roomWidth)
                //     wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), null);

                // Top entry
                // if (hasTopEntry && y == roomHeight - 2 && x > roomWidth - 3 && x < roomWidth)
                //     wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), null);
                
                // Right exit
                if (hasRightExit && x == roomWidth && y > 0 && y < 4)
                    wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), null);
                    pathTilemap.SetTile(new Vector3Int(tileX, tileY, 0), pathTile); // Place a path tile on the background

                // Bottom exit
                // if (hasBottomExit && y == 0 && x > roomWidth - 3 && x < roomWidth)
                //     wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), null);

                // Top exit
                // if (hasTopExit && y == roomHeight - 2 && x > roomWidth - 3 && x < roomWidth)
                //     wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), null);
            }
        }
    }

    private void AddExitWallsToRoom(Vector2Int mazeEntry, Vector2Int mazeExit)
    {
        // Debug.Log($"Creating Exit Wall: {mazeEntry} and exit: {mazeExit}");
        // Determine which walls, ceiling, and floor should have tiles
        bool hasLeftWall = mazeEntry != new Vector2Int(-1, 0) && mazeExit != new Vector2Int(-1, 0);
        bool hasRightWall = mazeEntry != new Vector2Int(1, 0) && mazeExit != new Vector2Int(1, 0);
        bool hasBottomWall = mazeEntry != new Vector2Int(0, -1) && mazeExit != new Vector2Int(0, -1);
        bool hasTopWall = mazeEntry != new Vector2Int(0, 1) && mazeExit != new Vector2Int(0, 1);

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

                // Right exit
                if (!hasRightWall && x == roomWidth && y > 3)
                    wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), wallTile);

                // Bottom exit
                if (!hasBottomWall && y == 0 && x > roomWidth - 4)
                    wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), wallTile);

                // Top exit
                if (!hasTopWall && y == roomHeight - 1 && (x < roomWidth - 3 || x > roomWidth - 1))
                    wallTilemap.SetTile(new Vector3Int(tileX, tileY, 0), wallTile);
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

    private void SpawnLight(Vector2Int room)
    {
        Debug.Log($"Spawning Light at ({room})");
        // Center position of the room cell in the expanded grid
        Vector2Int roomCenter = new Vector2Int(room.x * 4 + 2, room.y * 4 + 2);
        
        // Offset to move the tiles to the bottom-left corner
        int offsetX = -roomWidth / 2;
        int offsetY = -roomHeight / 2;

        // Apply offset to roomCenter
        roomCenter += new Vector2Int(offsetX, offsetY);

        GameObject lightObject = Instantiate(lightPrefab, new Vector2(roomCenter.x, roomCenter.y), Quaternion.identity, transform);
        lightObject.transform.localPosition = new Vector2(roomCenter.x, roomCenter.y);
    }
   
}
