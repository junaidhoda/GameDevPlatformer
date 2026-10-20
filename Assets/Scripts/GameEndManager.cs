
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameEndManager : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public PlayerHealth playerHealth;
    public TreasureManager treasureManager;
    public SurvivalTimer survivalTimer;

    // NEW: Our Game Over screen
    public GameObject gameOverPanel;

    private bool gameEnded = false;

    void Start()
    {
        Time.timeScale = 1f;
        gameEnded = false;

        resultText.gameObject.SetActive(false);

        // Hide the menu when the game starts
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

        // Show our red screen
        gameOverPanel.SetActive(true);

        // Display the result
        resultText.gameObject.SetActive(true);
        resultText.text = message;

        // Pause the game
        Time.timeScale = 0f;

        Debug.Log(message);
    }

    // NEW: Respawn button
    public void Respawn()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    // NEW: Main Menu button
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
