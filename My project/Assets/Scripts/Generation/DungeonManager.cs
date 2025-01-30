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

    void Awake()
    {
        player = GameObject.Find("Player").transform;
        dungeonGenerator = GetComponentInChildren<RecursiveGenerator>();
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
        Vector2Int roomVector = GetPlayerCurrentDungeonRoom();
        int progress = roomVector.x + roomVector.y;
        if (progressText != null) progressText.text = $"{progress} / {dungeonGenerator.numRooms - 1}";
        
        // DEBUG: Testing purposes clear rooms
        if (Input.GetKeyDown(KeyCode.R)) // Press 'R' to trigger regeneration
        {
            dungeonGenerator.Regenerate();
        }
        
        if (Input.GetKeyDown(KeyCode.T)) // Press 'T' to trigger delete room door
        {
            Debug.Log(GetPlayerCurrentDungeonRoom());

        }

        if (Input.GetKeyDown(KeyCode.D)) // Press 'D' to trigger debug roomMap
        {
            dungeonGenerator.DebugRoomMap();
        }
    }

    public void RemoveCurrentRoomExitDoor()
    {
        // Get current room via mCamera position
        Vector2Int currentRoomPos = GetPlayerCurrentDungeonRoom();
        Debug.Log($"cam pos: {mCamera.transform.position}, room position: {currentRoomPos}");
        dungeonGenerator.ClearRoomExitDoor(currentRoomPos);
    }

    public Vector2Int GetPlayerCurrentDungeonRoom()
    {
        return new Vector2Int((int)(mCamera.transform.position.x / roomWidth), (int)(mCamera.transform.position.y / roomHeight));
    }

    public Vector2Int GetObjectCurrentMazeRoom(Transform obj)
    {
        Vector2Int currentRoomPos = GetPlayerCurrentDungeonRoom();
        // multiple by room size
        currentRoomPos *= new Vector2Int(roomWidth, roomHeight);
        Vector2 distance = new Vector2(obj.position.x - currentRoomPos.x, obj.position.y - currentRoomPos.y);
        Debug.Log(distance);
        // Use distance from center of dungeon room to figure out which maze room the object is in
        

        return new Vector2Int(0, 0);
    }

}
