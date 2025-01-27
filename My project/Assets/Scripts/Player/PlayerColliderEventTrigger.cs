using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerColliderEventTrigger : MonoBehaviour
{
    public UnityEvent hitEnemy;
    public UnityEvent myEvent;

    public void OnTriggerEnter2D(Collider2D other)
    {
        switch(other.tag)
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
