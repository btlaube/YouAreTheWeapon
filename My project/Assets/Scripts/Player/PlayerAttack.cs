using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public List<KeyCode> attackKeys;
    public GameObject attackPrefab;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Get reference to Settings instance
        Settings settings = Settings.Instance;
        attackKeys = settings.LookUpKeyBind("Attack");

    }

    // Update is called once per frame
    void Update()
    {

        // TODO: Implement attack cooldown
        if (IsAnyKeyDown(attackKeys))
        {
            Debug.Log("Attack!");
            StartCoroutine(Attack());
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

        yield return new WaitForSeconds(0.5f);

        Destroy(attackObject);
    }

}
