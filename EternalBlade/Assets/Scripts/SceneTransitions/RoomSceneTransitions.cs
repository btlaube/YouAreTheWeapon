using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomSceneTransitions : MonoBehaviour
{

    private LevelLoader levelLoader;

    void Start()
    {
        levelLoader = LevelLoader.instance;
    }

    public void ResetScene()
    {
        if (levelLoader != null) levelLoader.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(int scene)
    {
        if (levelLoader != null) levelLoader.LoadScene(scene);
    }
}
