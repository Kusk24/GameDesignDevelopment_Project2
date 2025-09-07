using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;   // for Button

[DefaultExecutionOrder(1000)]
public class MenuUIHandler : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public Button level1Button;
    public Button level2Button;
    public Button exitButton;

    private void OnEnable()
    {
        // Give keyboard/controller focus to Start by default
        if (startButton != null)
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
    }

    private void Update()
    {
        // Quick shortcuts
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            StartGame();

        if (Input.GetKeyDown(KeyCode.Escape))
            ExitGame();
    }

    public void StartGame()   { LoadSceneByName("Level1"); }
    public void LoadLevel1()  { LoadSceneByName("Level1"); }
    public void LoadLevel2()  { LoadSceneByName("Level2"); }

    public void ExitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    private void LoadSceneByName(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogError($"Scene '{sceneName}' isn't in Build Settings.");
    }
}
