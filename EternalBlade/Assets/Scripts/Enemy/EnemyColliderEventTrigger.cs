using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyColliderEventTrigger : MonoBehaviour
{
    public UnityEvent hitByAttack;

    private void OnTriggerStay2D(Collider2D collision)
    {
        switch(collision.tag)
        {
            case "Player":
                transform.GetComponent<EnemyBehavior>().PlayerEnteredRange();
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch(collision.tag)
        {
            case "Player":
                transform.GetComponent<EnemyBehavior>().PlayerEnteredRange();
                break;
            default:
                break;
        }
    }

    public void PlayerEnterAttackCollider()
    {
        Debug.Log("ahsdkfhsd");
    }

}
