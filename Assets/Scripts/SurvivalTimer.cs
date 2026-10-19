using System.Collections;
using UnityEngine;
using TMPro;

public class SurvivalTimer : MonoBehaviour
{
    public float timeRemaining = 60f;
    public TextMeshProUGUI timerText;

    private bool timerRunning = true;

    void Update()
    {
        if (!timerRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            timerRunning = false;

            Debug.Log("Time's up!");
        }

        UpdateTimerDisplay();
    }

    void UpdateTimerDisplay()
    {
        int seconds = Mathf.CeilToInt(timeRemaining);
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        timerText.text = string.Format(
            "{0:00}:{1:00}",
            minutes,
            remainingSeconds
        );
    }
}
