using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour {
    
    [SerializeField] private float trackingSpeed;
    public float minX;
    public float minY;
    public float maxX;
    public float maxY;
    // public static CameraFollow instance;

    [SerializeField] private Transform camTarget;

    // void Awake() {
    //     if (instance == null) {
    //         instance = this;
    //     }
    //     else {
    //         Destroy(gameObject);
    //         return;
    //     }
        
    //     DontDestroyOnLoad(gameObject);
    // }

    void FixedUpdate() {
        if (camTarget != null) {
            var newPos = Vector2.Lerp(transform.position, new Vector2(camTarget.position.x, camTarget.position.y + 2f), Time.deltaTime * trackingSpeed);
            var camPosition = new Vector3(newPos.x, newPos.y, -10f);
            var v3 = camPosition;
            var clampX = Mathf.Clamp(v3.x, minX, maxX);
            var clampY = Mathf.Clamp(v3.y, minY, maxY);
            transform.position = new Vector3(clampX, clampY, -10f);
        }
    }

    public void UpdateCameraBounds(float newMinX, float newMinY, float newMaxX, float newMaxY)
    {
        minX = newMinX;
        minY = newMinY;
        maxX = newMaxX;
        maxY = newMaxY;
    }

}
