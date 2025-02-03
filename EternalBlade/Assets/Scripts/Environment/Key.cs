using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    private DungeonManager dungeonManager;

    void Awake()
    {
        dungeonManager = GameObject.Find("Dungeon Manager").GetComponent<DungeonManager>();
    }

    public void CollectKey()
    {
        if (dungeonManager != null) dungeonManager.RemoveCurrentRoomExitDoor();
    }

}
