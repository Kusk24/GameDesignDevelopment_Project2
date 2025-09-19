using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("A Canvas child panel with buttons (Restart/Main Menu/Quit). Keep it INACTIVE in the scene.")]
    public GameObject gameOverPanel;
    [Tooltip("Optional: which UI element should be selected first on Game Over (e.g., Restart button).")]
    public Selectable firstSelected;

    [Header("Behavior")]
    [Tooltip("Freeze the world on Game Over via Time.timeScale = 0.")]
    public bool freezeTimeOnGameOver = true;

    public enum State { Playing, Won, Lost }
    public State GameState { get; private set; } = State.Playing;

    bool _handlingGameOver;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Safety: if we reloaded after a paused game, restore time
        Time.timeScale = 1f;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    // --- Public API ---

    public void TriggerGameOver(string reason = "Lives depleted")
    {
        if (_handlingGameOver || GameState != State.Playing) return;
        StartCoroutine(GameOverRoutine(reason));
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        // If you have a dedicated menu scene, change the name here:
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    // --- Internals ---

    IEnumerator GameOverRoutine(string reason)
    {
        _handlingGameOver = true;
        GameState = State.Lost;
        Debug.Log($"GAME OVER: {reason}");

        // 1) Stop enemy spawning / wave progression cleanly
        var spawners = FindObjectsOfType<EnemySpawnManager>(includeInactive: true);
        foreach (var s in spawners)
        {
            s.StopAllCoroutines();
            s.enabled = false;
        }

        // 2) Disable enemy brains so nothing keeps wandering/shooting
        var enemies = FindObjectsOfType<EnemyMovement>(includeInactive: true);
        foreach (var e in enemies) e.enabled = false;

        // 3) Disable power-up spawning (existing pickups become harmless once we freeze)
        var puSpawner = FindFirstObjectByType<PowerUpSpawner>();
        if (puSpawner) puSpawner.enabled = false;

        // 4) Disable player input/movement so we don’t get ghost actions if player object still exists
        var playerMove = FindFirstObjectByType<PlayerMovement>();
        if (playerMove) playerMove.enabled = false;

        // (Optional small delay so death SFX/VFX kick off before freeze)
        yield return null;

        if (freezeTimeOnGameOver)
            Time.timeScale = 0f;

        // 5) Show the Game Over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Focus the first selected control (for keyboard/controller)
            if (firstSelected != null)
                EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
        }
    }
}