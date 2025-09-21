using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Text titleText; // optional: "Paused" / "Game Over"

    [Header("Scene Names")]
    public string mainMenuScene = "Menu";

    private bool isPaused = false;
    private bool isGameOver = false;

    void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (resumeButton) resumeButton.interactable = true; // default
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Only toggle with P if not Game Over
        if (GetPauseInput() && !isGameOver)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    private bool GetPauseInput()
    {
        #if ENABLE_INPUT_SYSTEM
            return Keyboard.current?.pKey.wasPressedThisFrame == true;
        #else
            return Input.GetKeyDown(KeyCode.P);
        #endif
    }

    private void Pause()
    {
        isPaused = true;
        isGameOver = false;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        
        // Force cursor to be visible and unlocked
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Force focus on the pause panel
        if (pausePanel != null)
        {
            Canvas canvas = pausePanel.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 100; // Bring to front
            }
        }

        if (titleText) titleText.text = "Paused";
        if (resumeButton) resumeButton.interactable = true;
        
        Debug.Log("Pause activated - Cursor should be visible and clickable");
    }

    // 🔴 Call this when the player dies
    public void GameOver()
    {
        isPaused = true;
        isGameOver = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        
        // Force cursor to be visible and unlocked
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Force focus on the pause panel
        if (pausePanel != null)
        {
            Canvas canvas = pausePanel.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 100; // Bring to front
            }
        }

        if (titleText) titleText.text = "Game Over";
        if (resumeButton) resumeButton.interactable = false; // Disable resume

        Debug.Log("Game Over - Cursor should be visible, Resume disabled");
    }

    // Button methods (connect in Inspector)
    public void Resume()
    {
        if (isGameOver) return; // Can't resume when game over

        Debug.Log("Resume called");
        isPaused = false;
        isGameOver = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        
    }

    public void Restart()
    {
        Debug.Log("Restart button clicked");
        Time.timeScale = 1f;
        isPaused = false;
        isGameOver = false; 
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Debug.Log("Main Menu button clicked");
        Time.timeScale = 1f;
        isPaused = false;
        isGameOver = false;
        
        
        SceneManager.LoadScene(mainMenuScene);
    }

    public void Exit()
    {
        Debug.Log("Exit called - only from button!");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
