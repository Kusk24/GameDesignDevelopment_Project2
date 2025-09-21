using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class VictoryUIManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject victoryPanel;
    public TMP_Text victoryText; // Reference to the UI Text component for victory message
    public ParticleSystem victoryParticles; // Reference to the Particle System for victory effects

    [Header("Scene Names")]
    public string mainMenuScene = "Menu";

    private bool victoryShown = false; // Flag to prevent multiple calls

    void Start()
    {
        if (victoryPanel) victoryPanel.SetActive(false);
    }

    // Call this when player wins
    public void ShowVictory()
    {
        if (victoryShown) return; // Prevent multiple calls
        victoryShown = true;

        Debug.Log("ShowVictory called - displaying victory panel");

        // Show victory panel immediately
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Debug.Log("Victory panel activated");
        }

        // Set victory text
        if (victoryText != null)
        {
            victoryText.text = "Victory!";
            Debug.Log("Victory text set"); // Fixed typo: was Debug.log
        }
        else
        {
            Debug.LogError("VictoryText is null! Make sure to assign it in Inspector.");
        }

        // Play victory particles
        if (victoryParticles != null)
        {
            victoryParticles.gameObject.SetActive(true);
            victoryParticles.Play();
            Debug.Log("Victory particles playing");
        }
        else
        {
            Debug.LogWarning("VictoryParticles is null - no particles will play.");
        }

        // Make cursor visible for UI interaction
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Button methods
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void Exit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
