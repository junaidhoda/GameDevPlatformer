
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game clicked!");
        Application.Quit();
    }
}
