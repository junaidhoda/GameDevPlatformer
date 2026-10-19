
using UnityEngine;
using TMPro;

public class GameEndManager : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public PlayerHealth playerHealth;
    public TreasureManager treasureManager;
    public SurvivalTimer survivalTimer;

    private bool gameEnded = false;

    void Start()
    {
        Time.timeScale = 1f;
        resultText.gameObject.SetActive(false);
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

        resultText.gameObject.SetActive(true);
        resultText.text = message;

        Time.timeScale = 0f;

        Debug.Log(message);
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
    }
}
