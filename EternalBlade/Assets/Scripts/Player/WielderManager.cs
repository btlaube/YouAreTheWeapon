using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WielderManager : MonoBehaviour
{
    public GameObject[] wielderCanvasObjects;
    public RuntimeAnimatorController[] wielderAnimators;

    public Transform[] statLists;
    private List<Wielder> wielders = new List<Wielder>();

    private WielderStats playerStats;
    [SerializeField] private int numSacrifices;
    private int[] selectedWielders;
    [SerializeField] private int currentWielder;

    void Awake()
    {
        playerStats = GameObject.Find("Player").GetComponentInChildren<WielderStats>();
    }

    // Start is called before the first frame update
    void Start()
    {
        selectedWielders = new int[statLists.Length];        
        LoadWielders();
        numSacrifices = PlayerPrefs.GetInt("Sacrifices", 0);

        // Add Wielder 1 Tank
        wielders.Add(new Wielder(2, 5, 7, 6)); // Total not including starting health: 14
        // Add Wielder 2 Rogue
        wielders.Add(new Wielder(5, 3, 5, 4)); // Total not including starting health: 13
        // Add Wielder 3 Mage
        wielders.Add(new Wielder(3, 5, 6, 5)); // Total not including starting health: 14

        UpdateStatDisplay();
        DisableWielders();
    }

    public void LoadWielders()
    {
        selectedWielders[0] = PlayerPrefs.GetInt("Wielder1", 0);
        selectedWielders[1] = PlayerPrefs.GetInt("Wielder2", 0);
        selectedWielders[2] = PlayerPrefs.GetInt("Wielder3", 0);
    }

    public void SaveWielders()
    {
        PlayerPrefs.SetInt("Wielder1", selectedWielders[0]);
        PlayerPrefs.SetInt("Wielder2", selectedWielders[1]);
        PlayerPrefs.SetInt("Wielder3", selectedWielders[2]);
        PlayerPrefs.Save();
    }

    public void ClearWielders()
    {
        PlayerPrefs.DeleteKey("Wielder1");
        PlayerPrefs.DeleteKey("Wielder2");
        PlayerPrefs.DeleteKey("Wielder3");
    }

    public void DisableWielders()
    {
        for (int i = 0; i < wielderCanvasObjects.Length; i++)
        {
            if (selectedWielders[i] == 1)
            {
                wielderCanvasObjects[i].SetActive(false);
            }
            else
            {
                wielderCanvasObjects[i].SetActive(true);
            }
        }
    }

    public void SelectWielder(int wielderIndex)
    {
        Debug.Log($"Selected wielder: {wielderIndex}");
        currentWielder = wielderIndex;        
        PlayerPrefs.SetInt("CurrentWielder", currentWielder);
        playerStats.SetStats(wielders[wielderIndex].GetMovement(), wielders[wielderIndex].GetAttackSpeed(), wielders[wielderIndex].GetMaxHealth(), wielders[wielderIndex].GetStartingHealth());
        playerStats.ApplyStats();
        playerStats.UpdateStatDisplay();
        UpdateWielderAnimator();
    }

    public void UpdateWielderAnimator()
    {
        currentWielder = PlayerPrefs.GetInt("CurrentWielder", 0);
        GameObject.Find("Player").GetComponent<Animator>().runtimeAnimatorController = wielderAnimators[currentWielder];
    }

    public void SacrificeCurrentWielder()
    {
        selectedWielders[currentWielder] = 1;
        SaveWielders();
    }

    public void UpdateStatDisplay()
    {
        for (int i = 0; i < statLists.Length; i++)
        {
            TMP_Text movementText = statLists[i].GetChild(0).GetComponent<TMP_Text>();
            TMP_Text attackSpeedText = statLists[i].GetChild(1).GetComponent<TMP_Text>();
            TMP_Text maxHealthText = statLists[i].GetChild(2).GetComponent<TMP_Text>();

            movementText.text = $"{wielders[i].GetMovement()}";
            attackSpeedText.text = $"{wielders[i].GetAttackSpeed()}";
            maxHealthText.text = $"{wielders[i].GetMaxHealth()}";
        }
    }
}
