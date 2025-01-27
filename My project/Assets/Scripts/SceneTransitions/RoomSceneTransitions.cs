using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomSceneTransitions : MonoBehaviour
{
    public void ResetScene()
    {
        LevelLoader levelLoader = GameObject.Find("LevelLoader").GetComponent<LevelLoader>();
        if (levelLoader != null) levelLoader.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
