using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackCooldown;
    private float attackTimer;
    private bool hasAttacked;

    public List<KeyCode> attackKeys;
    public GameObject attackPrefab;
    private SpriteRenderer sr;
    private AudioHandler audioHandler;
    private Animator animator;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        audioHandler = GetComponent<AudioHandler>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // Get reference to Settings instance
        Settings settings = Settings.Instance;
        attackKeys = settings.LookUpKeyBind("Attack");

        hasAttacked = false;
        attackTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsAnyKeyDown(attackKeys) && !hasAttacked)
        {
            Debug.Log("Attack!");
            audioHandler.Play("Attack");
            animator.SetTrigger("Attack");
            StartCoroutine(Attack());
            hasAttacked = true;
        }
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

    // Input Helper methods
    private bool IsAnyKeyHeld(List<KeyCode> attackKeys)
    {
        foreach (var key in attackKeys)
        {
            if (Input.GetKey(key))
                return true;
        }
        return false;
    }

    private bool IsAnyKeyDown(List<KeyCode> attackKeys)
    {
        foreach (var key in attackKeys)
        {
            if (Input.GetKeyDown(key))
                return true;
        }
        return false;
    }

    public IEnumerator Attack()
    {

        // Move attack to left or right of player
        GameObject attackObject = Instantiate(attackPrefab, transform.position, Quaternion.identity, transform);
        attackObject.transform.localPosition += (sr.flipX ? new Vector3(-1, 0.5f, 0) : new Vector3(1, 0.5f, 0));

        yield return new WaitForSeconds(0.125f);

        Destroy(attackObject);
    }

}
