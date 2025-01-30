using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance;
    public Animator transition;
    // public CanvasGroupScript canvasGroup;

    [SerializeField] private float transitionTime = 1f;

    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // DEBUG: Testing purposes
        if (Input.GetKeyDown(KeyCode.L)) // Press 'L' to trigger 
        {
            ActivateTransition(0);
        }
    }

    public void ResetScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(int sceneToLoad) {
        StartCoroutine(LoadLevel(sceneToLoad));
    }

    IEnumerator LoadLevel(int levelIndex) {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelIndex);

        transition.SetTrigger("End");
    }

    public void StartTransition()
    {
        transition.SetTrigger("Start");
    }

    public void EndTransition()
    {
        transition.SetTrigger("End");
    }

    public void ActivateTransition(int transitionIndex)
    {
        StartCoroutine(Transition(transitionIndex));
    }

    IEnumerator Transition(int transitionIndex) {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        transition.SetTrigger("End");
    }

    public void Quit() {
        Application.Quit();
    }
}
