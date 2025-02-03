using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    [SerializeField] private List<Transform> cameraPositions = new List<Transform>(); // List of camera positions
    private Camera cameraComponent;

    // ScreenShake
    private CameraShake screenShakeController;

    void Awake()
    {
        screenShakeController = GetComponent<CameraShake>();
    }

    void Start()
    {
        // Initialize camera component
        cameraComponent = GetComponent<Camera>();
    }


    void Update()
    {
        cameraPositions.RemoveAll(s => s == null);
        if (player == null || cameraPositions.Count == 0) return ;
        // Check if the player has moved beyond the current camera view
        Vector3 playerPosition = player.position;
        Vector3 cameraPosition = cameraComponent.transform.position;

        // Calculate the camera's pixel rect
        Rect pixelRect = cameraComponent.pixelRect;

        // Convert pixelRect to world space coordinates
        Vector3[] corners = new Vector3[4];
        corners[0] = cameraComponent.ScreenToWorldPoint(new Vector3(pixelRect.xMin, pixelRect.yMin, cameraComponent.nearClipPlane)); // Bottom-left
        corners[1] = cameraComponent.ScreenToWorldPoint(new Vector3(pixelRect.xMax, pixelRect.yMin, cameraComponent.nearClipPlane)); // Bottom-right
        corners[2] = cameraComponent.ScreenToWorldPoint(new Vector3(pixelRect.xMax, pixelRect.yMax, cameraComponent.nearClipPlane)); // Top-right
        corners[3] = cameraComponent.ScreenToWorldPoint(new Vector3(pixelRect.xMin, pixelRect.yMax, cameraComponent.nearClipPlane)); // Top-left

        // Define bounds
        float leftBorder = corners[0].x;
        float rightBorder = corners[1].x;
        float topBorder = corners[2].y;
        float bottomBorder = corners[0].y;

        if (playerPosition.x < leftBorder || playerPosition.x > rightBorder ||
            playerPosition.y < bottomBorder || playerPosition.y > topBorder)
        {
            MoveCameraToNearestPosition(playerPosition);
            Debug.Log(playerPosition);
        }
    }

    void MoveCameraToNearestPosition(Vector3 playerPosition)
    {
        // Debug.Log(Camera.current.pixelRect);
        // Find the nearest camera position in the list based on player position
        Transform nearestPosition = cameraPositions[0];
        float minDistance = Mathf.Infinity;

        foreach (Transform pos in cameraPositions)
        {
            float distance = Vector3.Distance(playerPosition, pos.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPosition = pos;
            }
        }

        if (nearestPosition != null)
        {
            Vector3 newPosition = nearestPosition.position;
            newPosition.z = cameraComponent.transform.position.z; // Keep the original z value
            cameraComponent.transform.position = newPosition;
        }
    }

    public void ClearCameraPositions()
    {
        Debug.Log("Clear camera pos");
        cameraPositions.Clear();
    }

    public void UpdateCameraPositions()
    {
        cameraPositions.Clear();
        // Get all objects with the "CameraPosition" tag
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("CameraPosition"))
        {
            // Add the transform of each object to the list
            cameraPositions.Add(obj.transform);
        }
    }

}
