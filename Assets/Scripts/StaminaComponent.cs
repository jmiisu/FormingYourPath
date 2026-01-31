using System;
using UnityEngine;

public class StaminaComponent : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField] private int maxStamina = 100;
    [SerializeField] private int currentStamina = 100;

    public int Max => maxStamina;
    public int Current => currentStamina;

    public event Action<int, int> OnStaminaChanged;
    public event Action OnGameOver;

    private bool _gameOverFired = false;

    void Start()
    {
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public bool CanSpend(int cost) => currentStamina >= cost;

    public bool TrySpend(int cost)
    {
        if (cost <= 0) return true;
        if (currentStamina <= 0) return false;
        if (currentStamina < cost) return false;

        currentStamina -= cost;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);

        if (currentStamina <= 0) TriggerGameOverOnce();
        return true;
    }
    
    public void Restore(int amount)
    {
        if (amount <= 0) return;
        if (currentStamina <= 0) return;

        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void TriggerGameOverOnce()
    {
        if (_gameOverFired) return;
        _gameOverFired = true;
        OnGameOver?.Invoke();
    }
}
