using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameSceneSequencing : MonoBehaviour
{
    public GameObject swordDeathCanvas;
    public GameObject dungeonClearedCanvas;
    public TMP_Text dungeonClearsText;

    public int dungeonClears;
    public bool spawnNewWielder;

    void Start()
    {
        dungeonClears = PlayerPrefs.GetInt("DungeonClears", 0);
        swordDeathCanvas.GetComponent<Canvas>().enabled = false;
        dungeonClearedCanvas.GetComponent<Canvas>().enabled = false;
    }

    void Update()
    {
        if (dungeonClearsText != null) dungeonClearsText.text = $"{dungeonClears}";

        // DEBUG: Testing purposes clear rooms
        if (Input.GetKeyDown(KeyCode.O))
        {
            ResetClears();
        }
    }

    public void ClearDungeon()
    {
        dungeonClears++;
        PlayerPrefs.SetInt("DungeonClears", dungeonClears);
        PlayerPrefs.Save();
        StartCoroutine(DungeonClearedSequence());
    }

    public IEnumerator DungeonClearedSequence()
    {
        DisableEnemiesAndPlayer();
        // LevelLoader.instance.StartTransition();
        yield return new WaitForSeconds(1f);
        // Replace with return to previous start room
        dungeonClearedCanvas.GetComponent<Canvas>().enabled = true;
        Debug.Log("Finished Dungeon Cleared Sequence");
    }

    public void Continue()
    {
        LevelLoader.instance.ResetScene();
    }

    public static void ResetClears()
    {
        PlayerPrefs.DeleteKey("DungeonClears");
    }

    public void StartSwordDeathSequence()
    {
        StartCoroutine(SwordDeathSequence());
    }

    public IEnumerator SwordDeathSequence()
    {
        DisableEnemiesAndPlayer();
        // LevelLoader.instance.StartTransition();
        yield return new WaitForSeconds(1f);
        // Replace with return to previous start room
        swordDeathCanvas.GetComponent<Canvas>().enabled = true;
        Debug.Log("Finished sword death sequence");
    }

    public void StartSacrificeWielderSequence()
    {
        StartCoroutine(SacrificeWielderSequence());
    }

    public IEnumerator SacrificeWielderSequence()
    {
        // LevelLoader.instance.EndTransition();
        swordDeathCanvas.GetComponent<Canvas>().enabled = false;
        yield return new WaitForSeconds(1f);
        StartCoroutine(WielderDeathSequence());
    }

    public void StartEndRunSequence()
    {
        StartCoroutine(EndRun());
    }

    public IEnumerator EndRun()
    {
        ResetClears();
        // LevelLoader.instance.EndTransition();
        yield return new WaitForSeconds(1f);
        // swordDeathCanvas.GetComponent<Canvas>().enabled = false;
        LevelLoader.instance.LoadScene(0);
    }

    public void StartWielderDeathSequence()
    {
        StartCoroutine(WielderDeathSequence());
    }

    public IEnumerator WielderDeathSequence()
    {
        DisableEnemiesAndPlayer();
        yield return new WaitForSeconds(1f);
        // Replace with return to previous start room
        GetComponent<RoomSceneTransitions>().ResetScene();
        Debug.Log("Finished wielder death sequence");
    }

    // Disable all enemies and player
    public void DisableEnemiesAndPlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.GetComponent<PlayerController>().enabled = false;
            player.GetComponent<PlayerHealth>().enabled = false;
            player.GetComponent<PlayerAttack>().enabled = false;
        }

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            EnemyBehavior enemyBehavior = enemy.GetComponent<EnemyBehavior>();
            if (enemyBehavior != null) enemyBehavior.enabled = false;
        }
    }

}
