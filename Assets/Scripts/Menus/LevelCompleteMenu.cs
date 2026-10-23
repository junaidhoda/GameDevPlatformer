using UnityEngine;
using UnityEngine.SceneManagement;

// Shown when the player lands on the level goal (see LevelGoal).
public class LevelCompleteMenu : MonoBehaviour
{
    private string nextSceneName;

    public static void Show(string titleTop, string titleBottom, string description,
        string continueLabel, string nextScene)
    {
        bool hasNextLevel = !string.IsNullOrEmpty(nextScene)
            && Application.CanStreamedLevelBeLoaded(nextScene);

        LevelCompleteMenu menu = null;
        GameObject root = GameMenuUI.Build("LevelCompleteMenu", titleTop, titleBottom, description,
            new GameMenuUI.ButtonSpec(hasNextLevel ? continueLabel : "COMING SOON",
                () => menu.ContinueToNextLevel(), GameMenuUI.ButtonStyle.Primary, hasNextLevel),
            new GameMenuUI.ButtonSpec("RESTART LEVEL", () => menu.RestartLevel()),
            new GameMenuUI.ButtonSpec("QUIT", () => menu.Quit(), GameMenuUI.ButtonStyle.Danger));

        menu = root.AddComponent<LevelCompleteMenu>();
        menu.nextSceneName = nextScene;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ContinueToNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
