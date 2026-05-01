using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class DungeonManager : MonoBehaviour
{
    public int roomWidth;
    public int roomHeight;

    public TMP_Text progressText;

    private RecursiveGenerator dungeonGenerator;
    private Camera mCamera;

    private Transform player;
    private AudioHandler AudioHandler;

    void Awake()
    {
        player = GameObject.Find("Player").transform;
        dungeonGenerator = GetComponentInChildren<RecursiveGenerator>();
        AudioHandler = GetComponent<AudioHandler>();
        mCamera = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (dungeonGenerator != null) dungeonGenerator.Generate();
    }

    // Update is called once per frame
    void Update()
    {
        // Vector2Int roomVector = GetPlayerCurrentDungeonRoomPosition();
        // Debug.Log(roomVector);
        Room currentRoom = GetPlayerCurrentDungeonRoom();
        // int progress = roomVector.x + roomVector.y;
        int progress = currentRoom.id;
        if (progressText != null) progressText.text = $"{progress} / {dungeonGenerator.numRooms - 1}";
    }

    public void RemoveCurrentRoomExitDoor()
    {
        AudioHandler.Play("Door Open");
        // Get current room via mCamera position
        Vector2Int currentRoomPos = GetPlayerCurrentDungeonRoomPosition();
        dungeonGenerator.ClearRoomExitDoor(currentRoomPos);
    }

    public Room GetPlayerCurrentDungeonRoom()
    {
        Dictionary<Vector2Int, Room> roomMap = dungeonGenerator.GetRoomMap();
        Room currentRoom = roomMap[new Vector2Int(0, 0)];
        float minDist = float.MaxValue;
        foreach (Room room in roomMap.Values)
        {
            Vector2 roomPosition = new Vector2(room.position.x * roomWidth, room.position.y * roomHeight);
            float dist = Vector2.Distance(player.position,  roomPosition);
            if (dist < minDist)
            {
                currentRoom = room;
                minDist = dist;
            }
        }
        return currentRoom;
    }

    public Vector2Int GetPlayerCurrentDungeonRoomPosition()
    {
        Dictionary<Vector2Int, Room> roomMap = dungeonGenerator.GetRoomMap();
        Room currentRoom = roomMap[new Vector2Int(0, 0)];
        float minDist = float.MaxValue;
        foreach (Room room in roomMap.Values)
        {
            Vector2 roomPosition = new Vector2(room.position.x * roomWidth, room.position.y * roomHeight);
            float dist = Vector2.Distance(player.position,  roomPosition);
            if (dist < minDist)
            {
                currentRoom = room;
                minDist = dist;
            }
        }
        return currentRoom.position;
    }

    public Vector2Int GetObjectCurrentMazeRoom(Transform obj)
    {
        Vector2Int currentRoomPos = GetPlayerCurrentDungeonRoomPosition();
        // multiple by room size
        currentRoomPos *= new Vector2Int(roomWidth, roomHeight);
        Vector2 distance = new Vector2(obj.position.x - currentRoomPos.x, obj.position.y - currentRoomPos.y);
        Debug.Log(distance);
        // Use distance from center of dungeon room to figure out which maze room the object is in
        

        return new Vector2Int(0, 0);
    }

}
