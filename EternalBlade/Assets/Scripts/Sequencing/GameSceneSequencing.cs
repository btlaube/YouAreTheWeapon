using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameSceneSequencing : MonoBehaviour
{
    public GameObject swordDeathCanvas;
    public GameObject dungeonClearedCanvas;
    public TMP_Text dungeonClearsText;

    public Transform gameCanvas;

    public Transform sword;
    private Transform player;

    public int dungeonClears;   // Used for PlayerPrefs
    public int spawnNewWielder; // Used for PlayerPrefs (0: false, 1: true)
    public int newWielderVisual; // Used for PlayerPrefs (0: false, 1: true)

    void Awake()
    {
        player = GameObject.Find("Player").transform;
    }

    void Start()
    {
        dungeonClears = PlayerPrefs.GetInt("DungeonClears", 0);
        swordDeathCanvas.GetComponent<Canvas>().enabled = false;
        dungeonClearedCanvas.GetComponent<Canvas>().enabled = false;

        spawnNewWielder = PlayerPrefs.GetInt("SpawnWielder", 0);
        newWielderVisual = PlayerPrefs.GetInt("ShowWielder", 0);
        StartWakeSequence();
    }

    public void StartWakeSequence()
    {
        sword.GetComponent<SpriteRenderer>().enabled = false;
        player.GetComponent<Animator>().SetBool("HasSword", false);
        if (spawnNewWielder == 1)
        {
            if (newWielderVisual == 1) 
            {
                // Set spawn wielder to false
                PlayerPrefs.SetInt("SpawnWielder", 0);
                PlayerPrefs.Save();
                StartCoroutine(WakeSequence());
            }
            // New player stats
            player.GetComponentInChildren<WielderStats>().InitializeNewWielder();
        }
        else
        {
            player.GetComponent<Animator>().SetBool("HasSword", true);
            // Previous player stats
            player.GetComponentInChildren<WielderStats>().InitializePreviousWielder();
        }
    }

    public IEnumerator WakeSequence()
    {
        // Hide canvas
        gameCanvas.GetComponent<Canvas>().enabled = false;
        // "Hide" player
        if (player != null) player.position = new Vector2(-100, 0);
        // player.GetComponent<Animator>().enabled = false;
        // Show sword
        sword.GetComponent<SpriteRenderer>().enabled = true;
        player.GetComponent<Animator>().SetBool("HasSword", false);
        // wait
        yield return new WaitForSeconds(1f);
        // Fade out
        LevelLoader.instance.StartTransition();
        // wait
        yield return new WaitForSeconds(1f);
        // Show Player (no sword)
        if (player != null) player.position = new Vector2(-10, -9);
        // player.GetComponent<SpriteRenderer>().enabled = false;
        player.GetComponent<Animator>().SetBool("HasSword", false);
        // wait
        yield return new WaitForSeconds(1f);
        // Fade in
        LevelLoader.instance.EndTransition();
        // wait
        yield return new WaitForSeconds(2f);
        // Fade out
        LevelLoader.instance.StartTransition();
        // wait
        yield return new WaitForSeconds(1f);
        // Move player to sword and show sword
        if (player != null) player.position = new Vector2(0.75f, -7.25f);
        // player.GetComponent<SpriteRenderer>().enabled = true;
        player.GetComponent<Animator>().SetBool("HasSword", true);
        // Hide sword
        Destroy(sword.gameObject);
        // show canvas
        gameCanvas.GetComponent<Canvas>().enabled = true;
        // wait
        yield return new WaitForSeconds(1f);
        // Fade in
        LevelLoader.instance.EndTransition();
        // yield return new WaitForSeconds(2f);
        // LevelLoader.instance.LoadScene(1);
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
        // Set spawn wielder to false
        PlayerPrefs.SetInt("SpawnWielder", 0);
        PlayerPrefs.SetInt("ShowWielder", 0);
        PlayerPrefs.Save();
        player.GetComponentInChildren<WielderStats>().SaveStats();
        player.GetComponent<PlayerHealth>().SaveSwordHealth();
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
        // Sacrificing wielder fully heals sword
        player.GetComponent<PlayerHealth>().FullHealSword();
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
        // Set swpawn wielder to true
        PlayerPrefs.SetInt("SpawnWielder", 1);
        PlayerPrefs.SetInt("ShowWielder", 1);
        PlayerPrefs.Save();
        player.GetComponent<PlayerHealth>().SaveSwordHealth();
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
            // player.GetComponent<PlayerHealth>().enabled = false;
            player.GetComponent<PlayerAttack>().enabled = false;
        }

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            EnemyBehavior enemyBehavior = enemy.GetComponent<EnemyBehavior>();
            if (enemyBehavior != null) enemyBehavior.enabled = false;
        }
    }

}
