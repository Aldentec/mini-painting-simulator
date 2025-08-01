using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

// Inventory Item - Data structure for inventory items
[System.Serializable]
public class InventoryItem {
    public string itemName;
    public string itemId;
    public GameObject prefab;
    public Sprite icon;
    public int quantity = 1;
    public DateTime acquiredDate;
    public Dictionary<string, object> customData = new Dictionary<string, object>();

    public InventoryItem(string name, GameObject prefab) {
        this.itemName = name;
        this.itemId = Guid.NewGuid().ToString();
        this.prefab = prefab;
        this.acquiredDate = DateTime.Now;
    }
}

// Helper class for JSON serialization of lists
[System.Serializable]
public class SerializableList<T> {
    public List<T> items;

    public SerializableList(List<T> items) {
        this.items = items;
    }
}

// Player Inventory - Manages player's collection of miniatures and items
public class PlayerInventory : MonoBehaviour {
    [Header("Inventory Settings")]
    public int maxInventorySlots = 100;

    private List<InventoryItem> inventoryItems = new List<InventoryItem>();

    // Events
    public static event Action<InventoryItem> OnItemAdded;
    public static event Action<InventoryItem> OnItemRemoved;
    public static event Action OnInventoryChanged;

    void Start() {
        LoadInventory();
    }

    public bool AddItem(string itemName, GameObject prefab, int quantity = 1) {
        if (inventoryItems.Count >= maxInventorySlots) {
            Debug.LogWarning("Inventory is full!");
            return false;
        }

        // Check if item already exists (for stackable items)
        InventoryItem existingItem = inventoryItems.Find(item => item.itemName == itemName);
        if (existingItem != null) {
            existingItem.quantity += quantity;
        }
        else {
            InventoryItem newItem = new InventoryItem(itemName, prefab);
            newItem.quantity = quantity;
            inventoryItems.Add(newItem);
            OnItemAdded?.Invoke(newItem);
        }

        SaveInventory();
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(string itemName, int quantity = 1) {
        InventoryItem item = inventoryItems.Find(i => i.itemName == itemName);
        if (item == null) return false;

        item.quantity -= quantity;

        if (item.quantity <= 0) {
            inventoryItems.Remove(item);
            OnItemRemoved?.Invoke(item);
        }

        SaveInventory();
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool HasItem(string itemName, int quantity = 1) {
        InventoryItem item = inventoryItems.Find(i => i.itemName == itemName);
        return item != null && item.quantity >= quantity;
    }

    public InventoryItem GetItem(string itemName) {
        return inventoryItems.Find(i => i.itemName == itemName);
    }

    public List<InventoryItem> GetInventoryItems() {
        return new List<InventoryItem>(inventoryItems);
    }

    public List<InventoryItem> GetItemsByCategory(string category) {
        return inventoryItems.Where(item =>
            item.customData.ContainsKey("category") &&
            item.customData["category"].ToString() == category).ToList();
    }

    public int GetTotalItems() {
        return inventoryItems.Sum(item => item.quantity);
    }

    public int GetUniqueItemCount() {
        return inventoryItems.Count;
    }

    void SaveInventory() {
        // Simple save system - you might want to use a more robust solution
        List<string> itemNames = inventoryItems.Select(item => item.itemName).ToList();
        List<int> quantities = inventoryItems.Select(item => item.quantity).ToList();

        string itemNamesJson = JsonUtility.ToJson(new SerializableList<string>(itemNames));
        string quantitiesJson = JsonUtility.ToJson(new SerializableList<int>(quantities));

        PlayerPrefs.SetString("InventoryItems", itemNamesJson);
        PlayerPrefs.SetString("InventoryQuantities", quantitiesJson);
        PlayerPrefs.Save();
    }

    void LoadInventory() {
        string itemNamesJson = PlayerPrefs.GetString("InventoryItems", "");
        string quantitiesJson = PlayerPrefs.GetString("InventoryQuantities", "");

        if (string.IsNullOrEmpty(itemNamesJson) || string.IsNullOrEmpty(quantitiesJson)) {
            Debug.Log("No saved inventory found, starting with empty inventory.");
            return;
        }

        try {
            SerializableList<string> loadedNames = JsonUtility.FromJson<SerializableList<string>>(itemNamesJson);
            SerializableList<int> loadedQuantities = JsonUtility.FromJson<SerializableList<int>>(quantitiesJson);

            if (loadedNames?.items != null && loadedQuantities?.items != null) {
                inventoryItems.Clear();

                for (int i = 0; i < loadedNames.items.Count && i < loadedQuantities.items.Count; i++) {
                    // Note: We can't save/load GameObject prefabs with PlayerPrefs easily
                    // You might want to use a more sophisticated save system for prefabs
                    // For now, we'll create items without prefabs and let the shop system handle prefab assignment
                    InventoryItem item = new InventoryItem(loadedNames.items[i], null);
                    item.quantity = loadedQuantities.items[i];
                    inventoryItems.Add(item);
                }

                Debug.Log($"Loaded {inventoryItems.Count} items from saved inventory.");
            }
        }
        catch (System.Exception e) {
            Debug.LogError($"Failed to load inventory: {e.Message}");
            inventoryItems.Clear(); // Start fresh if loading fails
        }
    }

    // Method to clear all inventory (useful for testing or reset functionality)
    public void ClearInventory() {
        inventoryItems.Clear();
        SaveInventory();
        OnInventoryChanged?.Invoke();
        Debug.Log("Inventory cleared.");
    }

    // Method to get inventory summary for debugging
    public void PrintInventorySummary() {
        Debug.Log($"=== INVENTORY SUMMARY ===");
        Debug.Log($"Total unique items: {GetUniqueItemCount()}");
        Debug.Log($"Total item count: {GetTotalItems()}");
        Debug.Log($"Max slots: {maxInventorySlots}");

        if (inventoryItems.Count > 0) {
            Debug.Log("Items:");
            foreach (var item in inventoryItems) {
                Debug.Log($"  - {item.itemName} x{item.quantity}");
            }
        }
        else {
            Debug.Log("Inventory is empty.");
        }
    }

    void OnDestroy() {
        // Unsubscribe from events to prevent memory leaks
        OnItemAdded = null;
        OnItemRemoved = null;
        OnInventoryChanged = null;
    }
}