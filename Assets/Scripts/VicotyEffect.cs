using UnityEngine;
using TMPro;

public class VictoryEffect : MonoBehaviour
{
    public TextMeshProUGUI victoryText;
    public float fadeDuration = 1f;
    public float scaleBounce = 1.2f;

    private void OnEnable()
    {
        StartCoroutine(PlayVictoryAnimation());
    }

    private System.Collections.IEnumerator PlayVictoryAnimation()
    {
        // Reset
        victoryText.alpha = 0f;
        victoryText.transform.localScale = Vector3.zero;

        float time = 0;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            // Fade in
            victoryText.alpha = Mathf.Lerp(0, 1, t);

            // Bounce scale
            float scale = Mathf.Lerp(0, scaleBounce, t);
            victoryText.transform.localScale = new Vector3(scale, scale, 1);

            yield return null;
        }

        // Snap to normal scale after bounce
        victoryText.transform.localScale = Vector3.one;
    }
}
