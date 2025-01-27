using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerControls", menuName = "Input/PlayerControls")]
public class PlayerControls : ScriptableObject
{
    [System.Serializable]
    public class ControlBinding
    {
        public string actionName;
        public List<KeyCode> keyCodes = new List<KeyCode>();
    }

    public List<ControlBinding> controls = new List<ControlBinding>();
}
