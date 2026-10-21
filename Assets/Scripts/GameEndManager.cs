
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameEndManager : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public PlayerHealth playerHealth;
    public TreasureManager treasureManager;
    public SurvivalTimer survivalTimer;

    // Our new Game Over screen
    public GameObject gameOverPanel;

    private bool gameEnded = false;

    void Start()
    {
        Time.timeScale = 1f;
        gameEnded = false;

        resultText.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (gameEnded) return;

        if (playerHealth.currentHealth <= 0)
        {
            EndGame("GAME OVER!");
        }
        else if (treasureManager.AllTreasuresCollected())
        {
            EndGame("YOU WIN!");
        }
        else if (survivalTimer.timeRemaining <= 0)
        {
            EndGame("TIME'S UP!");
        }
    }

    void EndGame(string message)
    {
        gameEnded = true;

        resultText.text = message;
        resultText.gameObject.SetActive(true);

        // Show the red screen and buttons
        gameOverPanel.SetActive(true);

        // Pause gameplay
        Time.timeScale = 0f;

        Debug.Log(message);
    }

    // Restart the level
    public void Respawn()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // Return to title screen
    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
    }
}
