using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{

    public static Settings Instance { get; private set; }
    public List<Control> controls = new List<Control>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<KeyCode> LookUpKeyBind(string action)
    {
        foreach (Control control in controls)
        {
            if (control.name == action)
            {
                return control.keyBinds;
            }
        }
        return null;
    }
}
