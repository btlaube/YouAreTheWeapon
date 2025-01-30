using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerColliderEventTrigger : MonoBehaviour
{
    public UnityEvent swordDamage;
    public UnityEvent wielderDamage;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch(collision.transform.tag)
        {
            case "Enemy":
                Debug.Log("Sword damage");
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
                if (wielderDamage != null) wielderDamage.Invoke();
                break;
            case "Coin":
                Debug.Log("Collect coin.");
                GetComponent<PlayerCoins>().GainCoins(1);
                break;
            default:
                break;
        }
    }
}
