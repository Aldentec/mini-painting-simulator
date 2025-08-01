using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Helper script to make setting up app buttons easier
[System.Serializable]
public class AppButtonHelper : MonoBehaviour {
    [Header("Auto-Find Components")]
    public bool autoFindComponents = true;

    [Header("Components")]
    public Button button;
    public Image background;
    public Image icon;
    public TextMeshProUGUI label;
    public Image border;

    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color pressedColor = Color.gray;
    public Color disabledColor = Color.gray;

    void Awake() {
        if (autoFindComponents) {
            FindComponents();
        }
    }

    void FindComponents() {
        // Find button component
        if (button == null)
            button = GetComponent<Button>();

        // Find background image
        if (background == null)
            background = GetComponent<Image>();

        // Find icon (look for child named "Icon")
        if (icon == null) {
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
                icon = iconTransform.GetComponent<Image>();
        }

        // Find label (look for child named "Label")
        if (label == null) {
            Transform labelTransform = transform.Find("Label");
            if (labelTransform != null)
                label = labelTransform.GetComponent<TextMeshProUGUI>();
        }

        // Find border (look for child named "Border")
        if (border == null) {
            Transform borderTransform = transform.Find("Border");
            if (borderTransform != null)
                border = borderTransform.GetComponent<Image>();
        }
    }

    public void SetupButton(string appName, Sprite appIcon, System.Action onClickAction) {
        if (label != null)
            label.text = appName;

        if (icon != null)
            icon.sprite = appIcon;

        if (button != null && onClickAction != null)
            button.onClick.AddListener(() => onClickAction.Invoke());
    }

    public void SetBorderColor(Color color) {
        if (border != null)
            border.color = color;
    }

    public void SetInteractable(bool interactable) {
        if (button != null)
            button.interactable = interactable;
    }

    // Method to create the prefab structure programmatically
    [ContextMenu("Create App Button Structure")]
    void CreateAppButtonStructure() {
        GameObject buttonObj = this.gameObject;

        // Ensure we have a Button component
        if (buttonObj.GetComponent<Button>() == null)
            buttonObj.AddComponent<Button>();

        // Ensure we have an Image component
        if (buttonObj.GetComponent<Image>() == null)
            buttonObj.AddComponent<Image>();

        // Create Icon child
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(buttonObj.transform);
        Image iconImage = iconObj.AddComponent<Image>();
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.6f);
        iconRect.anchorMax = new Vector2(0.5f, 0.6f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(60, 60);

        // Create Label child
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(buttonObj.transform);
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = "App Name";
        labelText.fontSize = 12;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.white;
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0.4f);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = Vector2.zero;

        // Create Border child
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(buttonObj.transform);
        Image borderImage = borderObj.AddComponent<Image>();
        borderImage.color = Color.clear;
        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = Vector2.zero;

        // Set up the main button RectTransform
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(80, 100);

        Debug.Log("App Button structure created!");
    }
}