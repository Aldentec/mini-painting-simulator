using UnityEngine;
using System;

// Currency Manager - Handles all money transactions
public class CurrencyManager : MonoBehaviour {
    [Header("Currency Settings")]
    public int startingCurrency = 1000;
    public int maxCurrency = 999999;

    private int currentCurrency;

    // Events
    public static event Action<int> OnCurrencyChanged;

    void Start() {
        // Load saved currency or use starting amount
        currentCurrency = PlayerPrefs.GetInt("PlayerCurrency", startingCurrency);
        OnCurrencyChanged?.Invoke(currentCurrency);
    }

    public bool CanAfford(int amount) {
        return currentCurrency >= amount;
    }

    public bool SpendCurrency(int amount) {
        if (!CanAfford(amount)) return false;

        currentCurrency = Mathf.Max(0, currentCurrency - amount);
        SaveCurrency();
        OnCurrencyChanged?.Invoke(currentCurrency);
        return true;
    }

    public void AddCurrency(int amount) {
        currentCurrency = Mathf.Min(maxCurrency, currentCurrency + amount);
        SaveCurrency();
        OnCurrencyChanged?.Invoke(currentCurrency);
    }

    public int GetCurrency() {
        return currentCurrency;
    }

    public void SetCurrency(int amount) {
        currentCurrency = Mathf.Clamp(amount, 0, maxCurrency);
        SaveCurrency();
        OnCurrencyChanged?.Invoke(currentCurrency);
    }

    void SaveCurrency() {
        PlayerPrefs.SetInt("PlayerCurrency", currentCurrency);
        PlayerPrefs.Save();
    }
}