using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Press Esc (or P) during a level to pause. Added automatically to any scene
// that has a player, so nothing needs to be placed in the scene.
public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    private GameObject menu;
    private CursorLockMode previousLock;
    private bool previousCursorVisible;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            IsPaused = false;
            if (FindFirstObjectByType<PlayerMovement>() != null && FindFirstObjectByType<PauseMenu>() == null)
                new GameObject("PauseMenu").AddComponent<PauseMenu>();
        };
    }

    void Update()
    {
        Keyboard k = Keyboard.current;
        if (k == null) return;
        if (!k.escapeKey.wasPressedThisFrame && !k.pKey.wasPressedThisFrame) return;

        if (IsPaused) Resume();
        else if (Time.timeScale > 0f) Pause(); // don't open over Game Over / Level Complete
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        previousLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        menu = GameMenuUI.Build("PauseMenuUI", "GAME", "PAUSED", null,
            new GameMenuUI.ButtonSpec("RESUME", Resume),
            new GameMenuUI.ButtonSpec("RESTART LEVEL", RestartLevel),
            new GameMenuUI.ButtonSpec("MAIN MENU", GoToMainMenu),
            new GameMenuUI.ButtonSpec("QUIT", Quit, GameMenuUI.ButtonStyle.Danger));
    }

    public void Resume()
    {
        if (menu != null) Destroy(menu);
        IsPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = previousLock;
        Cursor.visible = previousCursorVisible;
    }

    void RestartLevel()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void GoToMainMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void Quit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
