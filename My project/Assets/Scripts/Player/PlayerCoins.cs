using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCoins : MonoBehaviour
{

    public int coins;

    void Awake()
    {
        LoadCoins();
    }

    public void SaveCoins()
    {
        // Save coins to player prefs
        PlayerPrefs.SetInt("PlayerCoins", coins);
        PlayerPrefs.Save();
    }

    public void LoadCoins()
    {
        coins = PlayerPrefs.GetInt("PlayerCoins", 0);
    }

    public void GainCoins(int gainedCoins)
    {
        coins += gainedCoins;
        SaveCoins();
    }

    public void SpendCoins(int spentCoins)
    {
        coins -= spentCoins;
        SaveCoins();
    }
    
}
