
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public Slider healthBar;
    public TextMeshProUGUI healthText;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);

        UpdateHealthUI();

        Debug.Log("Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Game Over!");
        }
    }

    void UpdateHealthUI()
    {
        healthBar.value = currentHealth;
        healthText.text = currentHealth + " / " + maxHealth + " HP";
    }
}
