using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

    public Vector2 defaultDurationAndMagnitude;

    public void ShakeScreen()
    {
        StartCoroutine(Shake(defaultDurationAndMagnitude.x, defaultDurationAndMagnitude.y));
    }

    public IEnumerator Shake(float duration, float magnitude) {
        Vector3 originalPos = transform.position;

        float elapsedTime = 0f;

        while(elapsedTime < duration) {
            float xOffset = Random.Range(-0.5f, 0.5f) * magnitude;
            float yOffset = Random.Range(-0.5f, 0.5f) * magnitude;

            transform.position = new Vector3(originalPos.x + xOffset, originalPos.y + yOffset, originalPos.z);

            yield return null;
            elapsedTime += Time.deltaTime;
        }
        transform.position = originalPos;
    }
}
