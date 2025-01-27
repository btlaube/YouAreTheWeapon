using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public int id;                   // Room ID
    public Vector2Int position;      // Grid position of the room
    public Room prevRoom;            // Previous room (entry direction)
    public Room nextRoom;            // Next room (exit direction)

    public Vector2Int entryPoint;    // Entry direction (relative)
    public Vector2Int exitPoint;     // Exit direction (relative)

    public Room(int id, Vector2Int position)
    {
        this.id = id;
        this.position = position;
    }

    public override string ToString()
    {
        return $"Room {id}: Entry {entryPoint}, Exit {exitPoint}";
    }
}
