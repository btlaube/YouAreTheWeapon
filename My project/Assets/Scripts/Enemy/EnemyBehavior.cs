using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    private Transform player;

    [Header("Enemy Attack Settings")]
    public float attackCooldown;
    private float attackTimer;
    private bool hasAttacked;

    public List<KeyCode> attackKeys;
    public GameObject attackPrefab;
    private SpriteRenderer sr;
    private Animator animator;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        player = GameObject.Find("Player").transform;
    }

    void Start()
    {
        hasAttacked = false;
        attackTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            // Face player
            float lookDirection = (player.position.x - transform.position.x);
            float multiplier = lookDirection < 0.0f ? -1.0f : 1.0f;
            transform.localScale = new Vector3(multiplier, 1.0f, 1.0f);

            // Attack timer
            if (hasAttacked)
            {
                attackTimer += Time.deltaTime;
            }
            if (attackTimer >= attackCooldown)
            {
                hasAttacked = false;
                attackTimer = 0f;
            }
        }
    }

    public void PlayerEnteredRange()
    {
        // Get distance from player to center of room (parent transform)
        Vector2 playerDistance = (Vector2)transform.parent.position - (Vector2)player.position;
        if (Mathf.Abs(playerDistance.x) < 20f && Mathf.Abs(playerDistance.y) < 10f)
        {
            if(!hasAttacked)
            {
                Debug.Log("Enemy Attack!");
                StartCoroutine(Attack());
                hasAttacked = true;
            }
        }

    }

    public IEnumerator Attack()
    {
        animator.SetTrigger("Attack");
        // Move attack to left or right of player
        GameObject attackObject = Instantiate(attackPrefab, transform.position, Quaternion.identity);
        // attackObject.transform.localPosition += (sr.flipX ? new Vector3(-1, 0.5f, 0) : new Vector3(1, 0.5f, 0));
        // Attack toward player
        float lookDirection = transform.localScale.x < 0.0f ? -1.0f : 1.0f;
        attackObject.GetComponent<Rigidbody2D>().velocity = new Vector2(10f * lookDirection, 0f);

        yield return new WaitForSeconds(5f);

        Destroy(attackObject);
    }
}
