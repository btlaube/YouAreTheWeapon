using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AttackCollider : MonoBehaviour
{
    // public UnityEvent hitEnemy;
    public int attackDamage;

    public void OnTriggerEnter2D(Collider2D other)
    {
        switch(other.tag)
        {
            case "Enemy":
                Debug.Log("Attacked enemy");
                EnemyHealth enemyHealth = other.transform.gameObject.GetComponent<EnemyHealth>();
                if (enemyHealth != null) enemyHealth.TakeDamage(attackDamage); // Relies on Enemy having TriggerCollider
                break;
            case "EnemyAttack":
                Debug.Log("Attacked enemy attack");
                Destroy(other.gameObject);
                break;
            default:
                break;
        }
    }
}
