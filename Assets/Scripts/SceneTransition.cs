using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public Image loadingImage;
    public float fadeDuration = 1f;
    public float holdDuration = 2f; // how long the screen stays fully visible before switching

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); 
        SetAlpha(0); // Hide at start
    }

    public void LoadSceneWithTransition(string sceneName)
    {
        StartCoroutine(DoTransition(sceneName));
    }

    private IEnumerator DoTransition(string sceneName)
    {
        // Fade in
        yield return StartCoroutine(Fade(1));

        // Hold the loading image (and TMP text stays on screen too)
        yield return new WaitForSeconds(holdDuration);

        // Load scene
        SceneManager.LoadScene(sceneName);

        // Fade out after load
        yield return StartCoroutine(Fade(0));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = loadingImage.color.a;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            SetAlpha(newAlpha);
            yield return null;
        }
    }

    private void SetAlpha(float a)
    {
        Color c = loadingImage.color;
        loadingImage.color = new Color(c.r, c.g, c.b, a);
    }
}