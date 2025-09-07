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

    [Header("Scene Names")]
    public string mainMenuScene = "Menu";

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Toggle pause with P
        if (GetPauseInput())
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
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Button methods (connect in Inspector)
    public void Resume()
    {
        Debug.Log("Resume called");
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

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
        Debug.Log("Exit called - only from button!");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
