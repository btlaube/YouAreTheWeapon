using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Shop : MonoBehaviour
{

    private PlayerCoins playerCoins;
    private PlayerHealth playerHealth;

    public TMP_Text coinsText;
    public Button healSwordButton;
    public Button healWielderButton;

    void Awake()
    {
        playerCoins = GameObject.Find("Player").GetComponent<PlayerCoins>();
        playerHealth = GameObject.Find("Player").GetComponent<PlayerHealth>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        coinsText.text = $"Coin: {playerCoins.coins}";
        healSwordButton.interactable = false;
        healWielderButton.interactable = false;
        if (playerCoins.coins > 0)
        {
            if (!playerHealth.IsSwordMaxHealth()) healSwordButton.interactable = true;
            if (!playerHealth.IsWielderMaxHealth()) healWielderButton.interactable = true;
        }
    }
}
