
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public Slider healthBar;
    public TextMeshProUGUI healthText;

    [Tooltip("After a hit, the player can't be hurt again for this long")]
    public float hitSafeTime = 0.6f;
    public Color hitColour = new Color(1f, 0.25f, 0.25f);

    private float lastHitTime = -999f;
    private Renderer bodyRenderer;

    void Start()
    {
        bodyRenderer = GetComponent<Renderer>();
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        // No damage while dashing
        PlayerDash dash = GetComponent<PlayerDash>();
        if (dash != null && dash.IsInvulnerable) return;

        // Stops two things hitting at once from taking a huge chunk of health
        if (Time.time - lastHitTime < hitSafeTime) return;
        lastHitTime = Time.time;

        if (bodyRenderer != null && currentHealth > 0) StartCoroutine(HitFlash());

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

    IEnumerator HitFlash()
    {
        Color before = bodyRenderer.material.color;
        for (int i = 0; i < 3; i++)
        {
            bodyRenderer.material.color = hitColour;
            yield return new WaitForSeconds(0.08f);
            bodyRenderer.material.color = before;
            yield return new WaitForSeconds(0.08f);
        }
    }

    void UpdateHealthUI()
    {
        healthBar.value = currentHealth;
        healthText.text = currentHealth + " / " + maxHealth + " HP";
    }
}
