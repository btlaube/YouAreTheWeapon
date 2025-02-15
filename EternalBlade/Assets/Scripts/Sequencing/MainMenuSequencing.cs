using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSequencing : MonoBehaviour
{
    private Transform mainMenuCanvas;

    void Awake()
    {
        mainMenuCanvas = GameObject.Find("Main Menu Canvas").transform;
    }

    public void StartButtonSequence()
    {
        Debug.Log("Start button sequence");
        // Set swpawn wielder to true
        PlayerPrefs.SetInt("SpawnWielder", 0);
        PlayerPrefs.SetInt("ShowWielder", 1);
        PlayerPrefs.Save();
        LevelLoader.instance.LoadScene(1);
    }

}
