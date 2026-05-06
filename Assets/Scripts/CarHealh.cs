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
    public bool destroyCarOnZeroHealth = true;

    private bool isDestroyed = false;
    private CarPartFALL[] carPartFALL;

    void Start()
    {
        carPartFALL = GetComponentsInChildren<CarPartFALL>();
        currentHealth = maxHealth;

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

        if (currentHealth <= 0)
        {
            isDestroyed = true;

            foreach (CarPartFALL part in carPartFALL)
            {
                part.Deteach();
            }

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