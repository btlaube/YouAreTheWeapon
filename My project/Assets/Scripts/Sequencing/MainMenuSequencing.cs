using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSequencing : MonoBehaviour
{
    public Transform sword;

    private Transform player;
    private Transform mainMenuCanvas;

    void Awake()
    {
        player = GameObject.Find("Player").transform;
        mainMenuCanvas = GameObject.Find("Main Menu Canvas").transform;
        player.GetComponent<SpriteRenderer>().enabled = false;
    }

    void Start ()
    {
        if (player != null) player.position = new Vector2(-100, 0);
    }

    public void StartButtonSequence()
    {
        Debug.Log("Start button sequence");

        // TODO: Add option check for skip intro scene
        // if (!skip intro scene)
        StartCoroutine(StartButtonCoroutine());
    }

    public IEnumerator StartButtonCoroutine()
    {
        
        // Fade out
        LevelLoader.instance.StartTransition();
        // wait
        yield return new WaitForSeconds(1f);
        // Hide canvas (if this script is on Canvas object)
        mainMenuCanvas.GetComponent<Canvas>().enabled = false;
        // Show Player (no sword)
        if (player != null) player.position = new Vector2(-10, -9);
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
        if (player != null) player.position = new Vector2(0, -7.25f);
        player.GetComponent<SpriteRenderer>().enabled = true;
        // Hide sword
        Destroy(sword.gameObject);
        // wait
        yield return new WaitForSeconds(1f);
        // Fade in
        LevelLoader.instance.EndTransition();
        yield return new WaitForSeconds(2f);
        LevelLoader.instance.LoadScene(1);

        // Fade out
            // Hide canvas
            // Show Player (no sword)
        // Fade in
        // Fade out
            // Player equips sword
        // Load game scene


    }

}
