using UnityEngine;
using TMPro;

public class CarHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("UI References")]
    public GameObject winScreen;
    public TextMeshProUGUI[] healthTexts;

    [Header("Car Destruction")]
    public bool detachAllPartsOnZeroHealth = true;

    [Header("Part Breaking Settings")]
    public CarPartFALL[] carParts;
    public int hitsNeededPerPart = 4;

    private int currentPartIndex = 0;
    private int currentPartHitCount = 0;
    private bool isDestroyed = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (carParts == null || carParts.Length == 0)
        {
            carParts = GetComponentsInChildren<CarPartFALL>(); // Automatically find all CarPartFALL components in child objects if not assigned in the Inspector.
        }

        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }

        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDestroyed)
            return;

        currentHealth -= damageAmount;

        if (currentHealth < 0)
            currentHealth = 0;

        Debug.Log("Car took damage. Current health: " + currentHealth);

        UpdateHealthUI();
        DamageCurrentPart();
         
        if (currentHealth <= 0) 
        {
            isDestroyed = true;

            if (detachAllPartsOnZeroHealth) // If this option is enabled, we immediately detach all remaining parts when the car is destroyed, instead of waiting for the player to hit them.
            {
                DetachAllRemainingParts();
            }

            WinGame();
        }
    }

    void DamageCurrentPart() // This is called every time the car takes damage, and it handles the logic for breaking parts based on the hit count.
    {
        if (carParts == null || carParts.Length == 0)
            return;

        if (currentPartIndex >= carParts.Length)
            return;

        currentPartHitCount++;

        Debug.Log("Current part hit count: " + currentPartHitCount + " / " + hitsNeededPerPart);

        if (currentPartHitCount >= hitsNeededPerPart) // If we've hit the current part enough times, detach it and move on to the next one.
        {
            if (carParts[currentPartIndex] != null)
            {
                carParts[currentPartIndex].Deteach();
            }

            currentPartIndex++;
            currentPartHitCount = 0;
        }
    }

    void DetachAllRemainingParts() // This is called when the car's health reaches zero, and it ensures that all remaining parts are detached immediately.
    {
        for (int i = currentPartIndex; i < carParts.Length; i++)
        {
            if (carParts[i] != null)
            {
                carParts[i].Deteach();
            }
        }
    }

    void UpdateHealthUI() // This updates the health text on the UI, and changes the color to red if health is 3 or below.
    {
        foreach (TextMeshProUGUI text in healthTexts)
        {
            if (text != null)
            {
                text.text = "Car Health: " + currentHealth + " / " + maxHealth;
                text.color = currentHealth <= 3 ? Color.red : Color.white;
            }
        }
    }

    void WinGame()
    {
        Debug.Log("You Win! The car has been destroyed.");

        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }

        // No Time.timeScale freeze here, so the player can still move.
    }
}