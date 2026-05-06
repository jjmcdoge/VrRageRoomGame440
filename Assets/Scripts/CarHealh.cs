using UnityEngine;
using TMPro;

public class CarHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("UI References")]
    public GameObject winScreen;
    public TextMeshProUGUI healthText;

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
            carParts = GetComponentsInChildren<CarPartFALL>();
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
        {
            return;
        }

        currentHealth -= damageAmount;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log("Car took damage. Current health: " + currentHealth);

        UpdateHealthUI();

        DamageCurrentPart();

        if (currentHealth <= 0)
        {
            isDestroyed = true;

            if (detachAllPartsOnZeroHealth)
            {
                DetachAllRemainingParts();
            }

            WinGame();
        }
    }

    void DamageCurrentPart()
    {
        if (carParts == null || carParts.Length == 0)
        {
            return;
        }

        if (currentPartIndex >= carParts.Length)
        {
            return;
        }

        currentPartHitCount++;

        Debug.Log("Current part hit count: " + currentPartHitCount + " / " + hitsNeededPerPart);

        if (currentPartHitCount >= hitsNeededPerPart)
        {
            if (carParts[currentPartIndex] != null)
            {
                carParts[currentPartIndex].Deteach();
            }

            currentPartIndex++;
            currentPartHitCount = 0;
        }
    }

    void DetachAllRemainingParts()
    {
        for (int i = currentPartIndex; i < carParts.Length; i++)
        {
            if (carParts[i] != null)
            {
                carParts[i].Deteach();
            }
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

        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }

        Invoke(nameof(FreezeGame), 3f);
    }

    void FreezeGame()
    {
        Time.timeScale = 0f;
    }
}