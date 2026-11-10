using UnityEngine;

namespace MazeDungeon
{
    /// <summary>
    /// Runtime data for a single room in the generated path.
    /// Directions are relative grid directions: (1,0) = right, (0,1) = up, (0,-1) = down, (-1,0) = left.
    /// A direction of (0,0) means "no entry" / "no exit".
    /// </summary>
    [System.Serializable]
    public class Room
    {
        /// <summary>Index of the room along the path (0 = start room).</summary>
        public int id;

        /// <summary>Grid position of the room in room units.</summary>
        public Vector2Int position;

        /// <summary>Direction the player enters from (relative to this room).</summary>
        public Vector2Int entryPoint;

        /// <summary>Direction the player exits through (relative to this room).</summary>
        public Vector2Int exitPoint;

        /// <summary>Which maze cell along the entry wall the opening sits at.</summary>
        public int entryPointOffset;

        /// <summary>Which maze cell along the exit wall the opening sits at.</summary>
        public int exitPointOffset;

        // Links are not serialized: Room references itself, which Unity's serializer can't handle.
        [System.NonSerialized] public Room prevRoom;
        [System.NonSerialized] public Room nextRoom;

        /// <summary>The spawned GameObject for this room.</summary>
        [System.NonSerialized] public GameObject instance;

        public bool IsStartRoom => prevRoom == null;
        public bool IsFinalRoom => nextRoom == null;
        public bool HasEntry => entryPoint != Vector2Int.zero;
        public bool HasExit => exitPoint != Vector2Int.zero;

        public Room(int id, Vector2Int position)
        {
            this.id = id;
            this.position = position;
        }

        public void SetInstance(GameObject roomObject)
        {
            instance = roomObject;
        }

        public override string ToString()
        {
            return $"Room {id}: Entry {entryPoint}, Exit {exitPoint}";
        }
    }
}