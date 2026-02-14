using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject attackPrefab;
    public Transform attackPoint;
    private Transform player;
    void Awake()
    {
        player = GameObject.Find("Player").transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Face away from player
        float lookDirection = player.position.x - transform.position.x;
        float multiplier = lookDirection < 0.0f ? 1.0f : -1.0f;
        transform.localScale = new Vector3(multiplier, 1.0f, 1.0f);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        switch(collision.tag)
        {
            case "Attack":
                Debug.Log("Player activated turret");
                StartCoroutine(Attack());
                break;
            default:
                break;
        }
    }

    public IEnumerator Attack()
    {
        // animator.SetTrigger("Attack");
        // Move attack to left or right of player
        GameObject attackObject = Instantiate(attackPrefab, attackPoint.position, Quaternion.identity);
        // attackObject.transform.localPosition += (sr.flipX ? new Vector3(-1, 0.5f, 0) : new Vector3(1, 0.5f, 0));
        // Attack toward player
        float lookDirection = transform.localScale.x < 0.0f ? -1.0f : 1.0f;
        attackObject.GetComponent<Rigidbody2D>().velocity = new Vector2(10f * lookDirection, 0f);
        attackObject.transform.localScale = new Vector3(lookDirection, 1.0f, 1.0f);

        yield return new WaitForSeconds(5f);

        Destroy(attackObject);
    }

}
