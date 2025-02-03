using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorColliderEventTrigger : MonoBehaviour
{
    private RecursiveGenerator generator;
    private GameSceneSequencing gameSceneSequencer;
    private Transform player;

    void Awake()
    {
        generator = GameObject.Find("Recursive Room Generator").GetComponent<RecursiveGenerator>();
        gameSceneSequencer = GameObject.Find("Game Scene Manager").GetComponent<GameSceneSequencing>();
        player = GameObject.Find("Player").transform;
    }

    public void ExtendDungeon()
    {
        gameSceneSequencer.ClearDungeon();
        // LevelLoader.instance.ResetScene();
    }

}
