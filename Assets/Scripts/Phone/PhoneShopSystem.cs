using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class PhoneShopSystem : MonoBehaviour {
    [Header("Shop UI References")]
    public Transform shopItemContainer;
    public GameObject shopItemPrefab;
    public Button buyTabButton;
    public Button sellTabButton;
    public TextMeshProUGUI currencyText;
    public ScrollRect shopScrollRect;

    [Header("Confirmation Dialog")]
    public GameObject confirmationDialog;
    public TextMeshProUGUI confirmationText;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Filters")]
    public TMP_Dropdown categoryFilter;
    public TMP_Dropdown rarityFilter;
    public TMP_InputField searchField;
    public Button refreshButton;

    private bool isBuyMode = true;
    private ShopItem pendingTransaction;

    // Shop item data structure
    [System.Serializable]
    public class ShopItem {
        public string itemName;
        public string description;
        public Sprite itemIcon;
        public int basePrice;
        public ItemCategory category;
        public ItemRarity rarity;
        public GameObject miniaturePrefab;
        public bool isAvailable = true;
        public int stockQuantity = -1; // -1 for unlimited
        public float sellMultiplier = 0.5f; // Sell for 50% of buy price
    }

    public enum ItemCategory {
        Characters,
        Vehicles,
        Buildings,
        Terrain,
        Accessories,
        All
    }

    public enum ItemRarity {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        All
    }

    [Header("Shop Data")]
    public List<ShopItem> shopItems = new List<ShopItem>();

    // Events
    public static event Action<ShopItem, bool> OnItemTransacted; // item, isBought
    // REMOVED: public static event Action<int> OnCurrencyChanged; - This was duplicate/unused

    // References to other systems
    private PlayerInventory playerInventory;
    private CurrencyManager currencyManager;

    void Start() {
        Debug.Log("=== PhoneShopSystem Start() called ===");
        InitializeShop();
        SetupEventListeners();
        RefreshShop();
        Debug.Log("=== PhoneShopSystem Start() finished ===");
    }

    void InitializeShop() {
        // Get references to other systems
        playerInventory = FindObjectOfType<PlayerInventory>();
        currencyManager = FindObjectOfType<CurrencyManager>();

        if (playerInventory == null) {
            Debug.LogWarning("PlayerInventory not found! Creating temporary reference.");
            // You'll need to ensure PlayerInventory exists in your scene
        }

        if (currencyManager == null) {
            Debug.LogWarning("CurrencyManager not found! Creating temporary reference.");
            // You'll need to ensure CurrencyManager exists in your scene
        }

        // Setup category filter
        if (categoryFilter != null) {
            categoryFilter.ClearOptions();
            List<string> categories = new List<string>();
            foreach (ItemCategory category in Enum.GetValues(typeof(ItemCategory))) {
                categories.Add(category.ToString());
            }
            categoryFilter.AddOptions(categories);
            categoryFilter.onValueChanged.AddListener(_ => RefreshShop());
        }

        // Setup rarity filter
        if (rarityFilter != null) {
            rarityFilter.ClearOptions();
            List<string> rarities = new List<string>();
            foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity))) {
                rarities.Add(rarity.ToString());
            }
            rarityFilter.AddOptions(rarities);
            rarityFilter.onValueChanged.AddListener(_ => RefreshShop());
        }

        // Setup search field
        if (searchField != null) {
            searchField.onValueChanged.AddListener(_ => RefreshShop());
        }
    }

    void SetupEventListeners() {
        if (buyTabButton != null)
            buyTabButton.onClick.AddListener(() => SetShopMode(true));

        if (sellTabButton != null)
            sellTabButton.onClick.AddListener(() => SetShopMode(false));

        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshShop);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmTransaction);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelTransaction);

        // FIXED: Listen to currency changes with correct signature
        if (currencyManager != null)
            CurrencyManager.OnCurrencyChanged += UpdateCurrencyDisplay;
    }

    void SetShopMode(bool buyMode) {
        isBuyMode = buyMode;

        // Update tab button states
        if (buyTabButton != null)
            buyTabButton.interactable = !buyMode;
        if (sellTabButton != null)
            sellTabButton.interactable = buyMode;

        RefreshShop();
    }

    void RefreshShop() {
        Debug.Log("=== RefreshShop() called ===");
        Debug.Log($"isBuyMode: {isBuyMode}");
        Debug.Log($"Shop Item Container: {shopItemContainer != null}");
        Debug.Log($"Shop Item Prefab: {shopItemPrefab != null}");
        Debug.Log($"Shop Items count: {shopItems.Count}");

        ClearShopItems();

        List<ShopItem> itemsToShow = isBuyMode ? GetBuyableItems() : GetSellableItems();
        Debug.Log($"Items to show count: {itemsToShow.Count}");

        itemsToShow = FilterItems(itemsToShow);
        Debug.Log($"Items after filtering: {itemsToShow.Count}");

        foreach (ShopItem item in itemsToShow) {
            Debug.Log($"Creating shop item: {item.itemName}");
            CreateShopItemUI(item);
        }

        UpdateCurrencyDisplay(currencyManager != null ? currencyManager.GetCurrency() : 0);
        Debug.Log("=== RefreshShop() finished ===");
    }

    List<ShopItem> GetBuyableItems() {
        List<ShopItem> buyableItems = new List<ShopItem>();

        foreach (ShopItem item in shopItems) {
            if (item.isAvailable && (item.stockQuantity != 0)) {
                buyableItems.Add(item);
            }
        }

        return buyableItems;
    }

    List<ShopItem> GetSellableItems() {
        List<ShopItem> sellableItems = new List<ShopItem>();

        if (playerInventory != null) {
            // Get items from player inventory that can be sold
            foreach (var inventoryItem in playerInventory.GetInventoryItems()) {
                ShopItem shopItem = shopItems.Find(si => si.itemName == inventoryItem.itemName);
                if (shopItem != null) {
                    sellableItems.Add(shopItem);
                }
            }
        }

        return sellableItems;
    }

    List<ShopItem> FilterItems(List<ShopItem> items) {
        List<ShopItem> filteredItems = new List<ShopItem>(items);

        // Category filter
        if (categoryFilter != null && categoryFilter.value != 0) // 0 is "All"
        {
            ItemCategory selectedCategory = (ItemCategory)(categoryFilter.value - 1);
            filteredItems.RemoveAll(item => item.category != selectedCategory);
        }

        // Rarity filter
        if (rarityFilter != null && rarityFilter.value != 0) // 0 is "All"
        {
            ItemRarity selectedRarity = (ItemRarity)(rarityFilter.value - 1);
            filteredItems.RemoveAll(item => item.rarity != selectedRarity);
        }

        // Search filter
        if (searchField != null && !string.IsNullOrEmpty(searchField.text)) {
            string searchTerm = searchField.text.ToLower();
            filteredItems.RemoveAll(item =>
                !item.itemName.ToLower().Contains(searchTerm) &&
                !item.description.ToLower().Contains(searchTerm));
        }

        return filteredItems;
    }

    void ClearShopItems() {
        foreach (Transform child in shopItemContainer) {
            Destroy(child.gameObject);
        }
    }

    void CreateShopItemUI(ShopItem item) {
        GameObject itemUI = Instantiate(shopItemPrefab, shopItemContainer);

        // Get UI components using the correct hierarchy from your prefab
        Image itemIcon = itemUI.transform.Find("Icon")?.GetComponent<Image>();

        // Try to find name/description/price in InfoPanel first, then direct children
        Transform infoPanel = itemUI.transform.Find("InfoPanel");
        TextMeshProUGUI itemName = null;
        TextMeshProUGUI itemDescription = null;
        TextMeshProUGUI itemPrice = null;

        if (infoPanel != null) {
            itemName = infoPanel.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            itemDescription = infoPanel.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            itemPrice = infoPanel.transform.Find("Price")?.GetComponent<TextMeshProUGUI>();
        }
        else {
            // Fallback to direct children
            itemName = itemUI.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            itemDescription = itemUI.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            itemPrice = itemUI.transform.Find("Price")?.GetComponent<TextMeshProUGUI>();
        }

        Button actionButton = itemUI.transform.Find("ActionButton")?.GetComponent<Button>();
        TextMeshProUGUI buttonText = actionButton?.transform.Find("ButtonText")?.GetComponent<TextMeshProUGUI>();

        // Fallback for button text
        if (buttonText == null) {
            buttonText = actionButton?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
        }

        // Set item data
        if (itemIcon != null) itemIcon.sprite = item.itemIcon;
        if (itemName != null) itemName.text = item.itemName;
        if (itemDescription != null) itemDescription.text = item.description;

        int displayPrice = isBuyMode ? item.basePrice : Mathf.RoundToInt(item.basePrice * item.sellMultiplier);
        if (itemPrice != null) itemPrice.text = $"${displayPrice}";

        // Setup action button
        if (actionButton != null) {
            if (buttonText != null) buttonText.text = isBuyMode ? "Buy" : "Sell";
            actionButton.onClick.RemoveAllListeners(); // Clear existing listeners
            actionButton.onClick.AddListener(() => InitiateTransaction(item));

            // Check if player can afford/has item
            bool canTransact = isBuyMode ? CanAfford(item) : HasItem(item);
            actionButton.interactable = canTransact;
        }

        // Add rarity coloring
        ApplyRarityColoring(itemUI, item.rarity);
    }

    void ApplyRarityColoring(GameObject itemUI, ItemRarity rarity) {
        Color rarityColor = GetRarityColor(rarity);

        // Apply color to border or background
        Image border = itemUI.transform.Find("Border")?.GetComponent<Image>();
        if (border != null) {
            border.color = rarityColor;
        }
    }

    Color GetRarityColor(ItemRarity rarity) {
        switch (rarity) {
            case ItemRarity.Common: return Color.white;
            case ItemRarity.Uncommon: return Color.green;
            case ItemRarity.Rare: return Color.blue;
            case ItemRarity.Epic: return Color.magenta;
            case ItemRarity.Legendary: return Color.yellow;
            default: return Color.white;
        }
    }

    bool CanAfford(ShopItem item) {
        return currencyManager != null && currencyManager.GetCurrency() >= item.basePrice;
    }

    bool HasItem(ShopItem item) {
        return playerInventory != null && playerInventory.HasItem(item.itemName);
    }

    void InitiateTransaction(ShopItem item) {
        pendingTransaction = item;
        ShowConfirmationDialog();
    }

    void ShowConfirmationDialog() {
        if (confirmationDialog == null) return;

        string action = isBuyMode ? "buy" : "sell";
        int price = isBuyMode ? pendingTransaction.basePrice :
                   Mathf.RoundToInt(pendingTransaction.basePrice * pendingTransaction.sellMultiplier);

        if (confirmationText != null) {
            confirmationText.text = $"Are you sure you want to {action} {pendingTransaction.itemName} for ${price}?";
        }

        confirmationDialog.SetActive(true);
    }

    void ConfirmTransaction() {
        if (pendingTransaction == null) return;

        bool success = false;

        if (isBuyMode) {
            success = BuyItem(pendingTransaction);
        }
        else {
            success = SellItem(pendingTransaction);
        }

        if (success) {
            OnItemTransacted?.Invoke(pendingTransaction, isBuyMode);
            RefreshShop();
        }

        HideConfirmationDialog();
    }

    void CancelTransaction() {
        pendingTransaction = null;
        HideConfirmationDialog();
    }

    void HideConfirmationDialog() {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
    }

    bool BuyItem(ShopItem item) {
        if (!CanAfford(item)) return false;

        // Deduct currency
        if (currencyManager != null) {
            currencyManager.SpendCurrency(item.basePrice);
        }

        // Add to inventory
        if (playerInventory != null) {
            playerInventory.AddItem(item.itemName, item.miniaturePrefab);
        }

        // Update stock
        if (item.stockQuantity > 0) {
            item.stockQuantity--;
        }

        Debug.Log($"Bought {item.itemName} for ${item.basePrice}");
        return true;
    }

    bool SellItem(ShopItem item) {
        if (!HasItem(item)) return false;

        int sellPrice = Mathf.RoundToInt(item.basePrice * item.sellMultiplier);

        // Add currency
        if (currencyManager != null) {
            currencyManager.AddCurrency(sellPrice);
        }

        // Remove from inventory
        if (playerInventory != null) {
            playerInventory.RemoveItem(item.itemName);
        }

        Debug.Log($"Sold {item.itemName} for ${sellPrice}");
        return true;
    }

    // FIXED: Now accepts int parameter to match Action<int> signature
    void UpdateCurrencyDisplay(int newAmount) {
        if (currencyText != null) {
            currencyText.text = $"${newAmount}";
        }
    }

    void OnDestroy() {
        if (currencyManager != null)
            CurrencyManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
    }
}