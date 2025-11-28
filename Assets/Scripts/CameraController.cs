using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    public Camera gameCamera;
    public Vector3 offset = new Vector3(0, -20, 0);
    public float duration = 1.5f;

    private void Awake()
    {
        if (gameCamera == null) gameCamera = GetComponent<Camera>();
        if (gameCamera == null) gameCamera = Camera.main;
    }

    public void MoveDown()
    {
        StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        // Wait 1 second before moving (as per your original logic)
        yield return new WaitForSeconds(1f);

        Vector3 startPos = gameCamera.transform.position;
        Vector3 targetPos = startPos + offset;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // Smooth Step

            gameCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        gameCamera.transform.position = targetPos;
    }
}