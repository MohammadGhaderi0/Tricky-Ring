using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SplashController : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Main Menu";
    [SerializeField] private float minimumWaitTime = 3.0f;

    private VisualElement _loadingFill;

    private void Start()
    {
        var uiDoc = GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        _loadingFill = root.Q<VisualElement>("LoadingFill");
        if (_loadingFill != null) _loadingFill.style.width = Length.Percent(0);
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        op.allowSceneActivation = false;

        // Use absolute time to avoid frame-rate lag issues
        float startTime = Time.time;
        float elapsed = 0f;

        while (!op.isDone)
        {
            elapsed = Time.time - startTime;

            // 1. Calculate Progress
            float actualProgress = Mathf.Clamp01(op.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(elapsed / minimumWaitTime);

            // 2. Visuals (Use the slower one)
            float visualProgress = Mathf.Min(actualProgress, timeProgress);

            if (_loadingFill != null)
            {
                _loadingFill.style.width = Length.Percent(visualProgress * 100);
            }

            // 3. COMPLETE?
            if (actualProgress >= 1f && timeProgress >= 1f)
            {
                Debug.Log($"SplashController [{gameObject.GetInstanceID()}] FINISHED waiting. Switching scene...");
                if (_loadingFill != null) _loadingFill.style.width = Length.Percent(100);
                yield return new WaitForSeconds(0.2f);
                op.allowSceneActivation = true;
                yield break;
            }
            yield return null;
        }
    }
}