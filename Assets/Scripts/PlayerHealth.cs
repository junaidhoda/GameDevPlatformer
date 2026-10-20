
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
        // No damage while dashing
        PlayerDash dash = GetComponent<PlayerDash>();
        if (dash != null && dash.IsInvulnerable) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);

        UpdateHealthUI();

        Debug.Log("Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Game Over!");
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        healthBar.value = currentHealth;
        healthText.text = currentHealth + " / " + maxHealth + " HP";
    }
}
