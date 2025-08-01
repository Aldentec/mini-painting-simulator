using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Helper script for shop item prefabs
public class ShopItemHelper : MonoBehaviour {
    [Header("Auto-Find Components")]
    public bool autoFindComponents = true;

    [Header("Components")]
    public Button mainButton;
    public Image background;
    public Image icon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI description;
    public TextMeshProUGUI price;
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;
    public Image border;

    [Header("Layout Settings")]
    public float itemHeight = 120f;
    public float iconSize = 80f;

    void Awake() {
        if (autoFindComponents) {
            FindComponents();
        }
    }

    void FindComponents() {
        // Main button
        if (mainButton == null)
            mainButton = GetComponent<Button>();

        // Background
        if (background == null)
            background = GetComponent<Image>();

        // Find icon
        if (icon == null) {
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
                icon = iconTransform.GetComponent<Image>();
        }

        // Find name text
        if (itemName == null) {
            Transform nameTransform = transform.Find("InfoPanel/Name");
            if (nameTransform != null)
                itemName = nameTransform.GetComponent<TextMeshProUGUI>();
        }

        // Find description text
        if (description == null) {
            Transform descTransform = transform.Find("InfoPanel/Description");
            if (descTransform != null)
                description = descTransform.GetComponent<TextMeshProUGUI>();
        }

        // Find price text
        if (price == null) {
            Transform priceTransform = transform.Find("InfoPanel/Price");
            if (priceTransform != null)
                price = priceTransform.GetComponent<TextMeshProUGUI>();
        }

        // Find action button
        if (actionButton == null) {
            Transform actionTransform = transform.Find("ActionButton");
            if (actionTransform != null)
                actionButton = actionTransform.GetComponent<Button>();
        }

        // Find action button text
        if (actionButtonText == null && actionButton != null) {
            actionButtonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        // Find border
        if (border == null) {
            Transform borderTransform = transform.Find("Border");
            if (borderTransform != null)
                border = borderTransform.GetComponent<Image>();
        }
    }

    public void SetupShopItem(PhoneShopSystem.ShopItem item, bool isBuyMode, System.Action onActionClick) {
        if (itemName != null)
            itemName.text = item.itemName;

        if (description != null)
            description.text = item.description;

        if (icon != null)
            icon.sprite = item.itemIcon;

        // Set price
        int displayPrice = isBuyMode ? item.basePrice :
                          Mathf.RoundToInt(item.basePrice * item.sellMultiplier);
        if (price != null)
            price.text = $"${displayPrice}";

        // Setup action button
        if (actionButton != null && onActionClick != null) {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => onActionClick.Invoke());
        }

        if (actionButtonText != null)
            actionButtonText.text = isBuyMode ? "Buy" : "Sell";

        // Set border color based on rarity
        SetRarityBorder(item.rarity);
    }

    public void SetRarityBorder(PhoneShopSystem.ItemRarity rarity) {
        if (border == null) return;

        Color rarityColor = GetRarityColor(rarity);
        border.color = rarityColor;
    }

    Color GetRarityColor(PhoneShopSystem.ItemRarity rarity) {
        switch (rarity) {
            case PhoneShopSystem.ItemRarity.Common: return Color.white;
            case PhoneShopSystem.ItemRarity.Uncommon: return Color.green;
            case PhoneShopSystem.ItemRarity.Rare: return Color.blue;
            case PhoneShopSystem.ItemRarity.Epic: return Color.magenta;
            case PhoneShopSystem.ItemRarity.Legendary: return Color.yellow;
            default: return Color.white;
        }
    }

    public void SetActionButtonInteractable(bool interactable) {
        if (actionButton != null)
            actionButton.interactable = interactable;
    }

    // Method to create the prefab structure programmatically
    [ContextMenu("Create Shop Item Structure")]
    void CreateShopItemStructure() {
        GameObject itemObj = this.gameObject;

        // Ensure we have required components
        if (itemObj.GetComponent<Button>() == null)
            itemObj.AddComponent<Button>();

        if (itemObj.GetComponent<Image>() == null) {
            Image bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark background
        }

        // Add Horizontal Layout Group for main layout
        HorizontalLayoutGroup hlg = itemObj.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null) {
            hlg = itemObj.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 10f;
            hlg.padding = new RectOffset(10, 10, 10, 10);
        }

        // Create Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(itemObj.transform);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = Color.white;
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);

        // Add Layout Element to icon
        LayoutElement iconLayout = iconObj.AddComponent<LayoutElement>();
        iconLayout.preferredWidth = iconSize;
        iconLayout.preferredHeight = iconSize;

        // Create Info Panel
        GameObject infoPanel = new GameObject("InfoPanel");
        infoPanel.transform.SetParent(itemObj.transform);
        VerticalLayoutGroup vlg = infoPanel.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 5f;

        // Add Layout Element to info panel
        LayoutElement infoLayout = infoPanel.AddComponent<LayoutElement>();
        infoLayout.flexibleWidth = 1f;

        // Create Name text
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(infoPanel.transform);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "Item Name";
        nameText.fontSize = 16;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = Color.white;

        // Create Description text
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(infoPanel.transform);
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = "Item description goes here...";
        descText.fontSize = 12;
        descText.color = new Color(0.8f, 0.8f, 0.8f);

        // Create Price text
        GameObject priceObj = new GameObject("Price");
        priceObj.transform.SetParent(infoPanel.transform);
        TextMeshProUGUI priceText = priceObj.AddComponent<TextMeshProUGUI>();
        priceText.text = "$100";
        priceText.fontSize = 14;
        priceText.fontStyle = FontStyles.Bold;
        priceText.color = Color.green;

        // Create Action Button
        GameObject actionObj = new GameObject("ActionButton");
        actionObj.transform.SetParent(itemObj.transform);
        Button actionBtn = actionObj.AddComponent<Button>();
        Image actionBg = actionObj.GetComponent<Image>();
        actionBg.color = new Color(0.3f, 0.6f, 1f); // Blue button

        // Action button layout
        LayoutElement actionLayout = actionObj.AddComponent<LayoutElement>();
        actionLayout.preferredWidth = 80f;
        actionLayout.preferredHeight = 40f;

        // Action button text
        GameObject actionTextObj = new GameObject("Text");
        actionTextObj.transform.SetParent(actionObj.transform);
        TextMeshProUGUI actionText = actionTextObj.AddComponent<TextMeshProUGUI>();
        actionText.text = "Buy";
        actionText.fontSize = 12;
        actionText.color = Color.white;
        actionText.alignment = TextAlignmentOptions.Center;

        // Set action text rect to fill button
        RectTransform actionTextRect = actionTextObj.GetComponent<RectTransform>();
        actionTextRect.anchorMin = Vector2.zero;
        actionTextRect.anchorMax = Vector2.one;
        actionTextRect.anchoredPosition = Vector2.zero;
        actionTextRect.sizeDelta = Vector2.zero;

        // Create Border
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(itemObj.transform);
        Image borderImage = borderObj.AddComponent<Image>();
        borderImage.color = Color.clear;
        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = Vector2.zero;

        // Set main item size
        RectTransform itemRect = itemObj.GetComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(0, itemHeight);

        Debug.Log("Shop Item structure created!");
    }
}