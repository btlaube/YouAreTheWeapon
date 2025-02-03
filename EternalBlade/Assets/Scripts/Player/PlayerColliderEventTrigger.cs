using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerColliderEventTrigger : MonoBehaviour
{
    public UnityEvent swordDamage;
    public UnityEvent wielderDamage;

    private AudioHandler audioHandler;

    void Awake()
    {
        audioHandler = GetComponent<AudioHandler>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch(collision.transform.tag)
        {
            case "Enemy":
                Debug.Log("Sword damage");
                audioHandler.Play("Damaged");
                if (swordDamage != null) swordDamage.Invoke();
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch(other.tag)
        {
            case "EnemyAttack":
                Debug.Log("Player damage");
                audioHandler.Play("Damaged");
                if (wielderDamage != null) wielderDamage.Invoke();
                break;
            case "Coin":
                Debug.Log("Collect coin.");
                audioHandler.Play("Collect Coin");
                GetComponent<PlayerCoins>().GainCoins(1);
                break;
            case "Key":
                Debug.Log("Collect key.");
                audioHandler.Play("Collect Key");
                break;
            default:
                break;
        }
    }
}
