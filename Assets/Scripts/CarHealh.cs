using UnityEngine;
using TMPro;

public class CarHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("How much health the car starts with.")]
    public int maxHealth = 10;

    // Keeps track of the car's current health during gameplay.
    private int currentHealth;

    [Header("UI References")]
    [Tooltip("Drag your YOU WIN panel here.")]
    public GameObject winScreen;

    [Tooltip("Drag your TextMeshPro health text here.")]
    public TextMeshProUGUI healthText;

    [Header("Car Destruction")]
    [Tooltip("If checked, the car disappears when health reaches 0.")]
    public bool destroyCarOnZeroHealth = true;

    // Stops the win logic from running more than once.
    private bool isDestroyed = false;

    void Start()
    {
        // Start the car at full health.
        currentHealth = maxHealth;

        // Hide the win screen when the level begins.
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }

        // Update the health text right away so it shows full health at the start.
        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount)
    {
        // If the car has already been destroyed, ignore extra damage.
        if (isDestroyed)
        {
            return;
        }

        // Lower the car's health.
        currentHealth -= damageAmount;

        // Prevent health from showing negative numbers.
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log("Car took damage. Current health: " + currentHealth);

        // Refresh the UI every time damage is taken.
        UpdateHealthUI();

        // If the car has no health left, trigger the win condition.
        if (currentHealth <= 0)
        {
            isDestroyed = true;
            WinGame();
        }
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
{
    healthText.text = "Car Health: " + currentHealth + " / " + maxHealth;

    if (currentHealth <= 3)
    {
        healthText.color = Color.red;
    }
    else
    {
        healthText.color = Color.white;
    }
}

    }

    void WinGame()
    {
        Debug.Log("You Win! The car has been destroyed.");

        // Show the win screen if assigned.
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }

        // Turn off the car if that option is enabled.
        if (destroyCarOnZeroHealth)
        {
            gameObject.SetActive(false);
        }

        // Pause the game.
        Time.timeScale = 0f;
    }
}