using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyColliderEventTrigger : MonoBehaviour
{
    public UnityEvent hitByAttack;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch(collision.tag)
        {
            default:
                break;
        }
    }
}
