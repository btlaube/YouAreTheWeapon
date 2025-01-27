using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerColliderEventTrigger : MonoBehaviour
{
    public UnityEvent hitEnemy;
    public UnityEvent myEvent;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch(collision.transform.tag)
        {
            case "Enemy":
                Debug.Log("Outch");
                if (hitEnemy != null) hitEnemy.Invoke();
                break;
            default:
                break;
        }
    }
}
