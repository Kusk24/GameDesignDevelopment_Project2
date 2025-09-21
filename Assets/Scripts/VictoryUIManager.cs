using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryUIManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject victoryPanel;

    [Header("Scene Names")]
    public string mainMenuScene = "Menu";

    void Start()
    {
        if (victoryPanel) victoryPanel.SetActive(false);
    }

    // Call this when player wins
    public void ShowVictory()
    {
        Time.timeScale = 0f; // stop gameplay
        victoryPanel.SetActive(true);

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
