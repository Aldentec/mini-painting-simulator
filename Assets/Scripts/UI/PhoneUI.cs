using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Linq;

public class PhoneUIManager : MonoBehaviour {
    [Header("Phone UI References")]
    public GameObject phoneCanvas;
    public Button homeButton;
    public Transform appContainer;
    public GameObject appButtonPrefab;

    [Header("Screen References")]
    public GameObject homeScreen;
    public GameObject shopScreen;
    public GameObject inventoryScreen;
    public GameObject upgradesScreen;
    public GameObject settingsScreen;

    [Header("Phone Animation")]
    public Animator phoneAnimator;
    public string openAnimationTrigger = "Open";
    public string closeAnimationTrigger = "Close";

    [Header("Audio")]
    public AudioSource phoneAudioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip buttonClickSound;

    private bool isPhoneOpen = false;
    private GameObject currentScreen;
    private Stack<GameObject> screenHistory = new Stack<GameObject>();

    [Header("Phone Apps")]
    public List<AppIconSO> phoneApps = new List<AppIconSO>();

    // Events
    public static event Action<bool> OnPhoneToggled;
    public static event Action<string> OnAppOpened;

    void Start() {
        Debug.Log("=== PhoneUIManager Start ===");
        Debug.Log($"ShopScreen children at start: {shopScreen.transform.childCount}");
        for (int i = 0; i < shopScreen.transform.childCount; i++) {
            Debug.Log($"  ShopScreen child {i}: {shopScreen.transform.GetChild(i).name}");
        }

        InitializePhone();
        SetupApps();
        ClosePhone();

        Debug.Log("=== PhoneUIManager Start Complete ===");
    }

    void Update() {
        HandleInput();
    }

    void HandleInput() {
        // Toggle phone with P key or controller button
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.JoystickButton0)) {
            TogglePhone();
        }

        // Back button functionality
        if (isPhoneOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))) {
            if (screenHistory.Count > 0) {
                GoBack();
            }
            else {
                ClosePhone();
            }
        }
    }

    void InitializePhone() {
        if (phoneCanvas == null) {
            Debug.LogError("Phone Canvas not assigned!");
            return;
        }

        // Setup home button
        if (homeButton != null) {
            homeButton.onClick.AddListener(GoToHomeScreen);
        }

        // Initialize screens
        currentScreen = homeScreen;

        // Setup audio source
        if (phoneAudioSource == null) {
            phoneAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void SetupApps() {
        Debug.Log("=== SetupApps() called ===");
        Debug.Log($"appContainer is null: {appContainer == null}");
        Debug.Log($"appButtonPrefab is null: {appButtonPrefab == null}");
        Debug.Log($"phoneApps count: {phoneApps.Count}");

        if (appContainer == null || appButtonPrefab == null) {
            Debug.LogError("Missing references! Cannot create apps.");
            return;
        }

        // Clear existing app buttons
        foreach (Transform child in appContainer) {
            Destroy(child.gameObject);
        }

        // Create app buttons
        foreach (AppIconSO app in phoneApps) {
            Debug.Log($"Processing app: '{app.appName}'");
            Debug.Log($"Creating button for: {app.appName}");
            CreateAppButton(app);
        }

        Debug.Log("=== SetupApps() finished ===");
    }

    void CreateAppButton(AppIconSO app) {
        Debug.Log($"CreateAppButton called for: {app.appName}");

        GameObject appButton = Instantiate(appButtonPrefab, appContainer);
        Debug.Log($"Button instantiated: {appButton != null}");
        Debug.Log($"Button name: {appButton.name}");

        // Setup button components
        Button btn = appButton.GetComponent<Button>();
        Image icon = appButton.transform.Find("Icon")?.GetComponent<Image>();
        TextMeshProUGUI label = appButton.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();

        Debug.Log($"Button component found: {btn != null}");
        Debug.Log($"Icon component found: {icon != null}");
        Debug.Log($"Label component found: {label != null}");

        if (icon != null && app.appIcon != null) {
            icon.sprite = app.appIcon;
            icon.color = Color.white; // Default color since AppIconSO doesn't have iconColor
        }

        if (label != null) {
            label.text = app.appName;
            Debug.Log($"Set label text to: {app.appName}");
        }

        // Add click listener
        if (btn != null) {
            btn.onClick.AddListener(() => OpenApp(app));
            // Add button click sound
            btn.onClick.AddListener(() => PlaySound(buttonClickSound));
            Debug.Log($"Click listener added for: {app.appName}");
        }

        Debug.Log($"App button setup complete for: {app.appName}");
    }

    public void TogglePhone() {
        if (isPhoneOpen) {
            ClosePhone();
        }
        else {
            OpenPhone();
        }
    }

    public void OpenPhone() {
        if (isPhoneOpen) return;

        isPhoneOpen = true;
        phoneCanvas.SetActive(true);

        // Play animation
        if (phoneAnimator != null) {
            phoneAnimator.SetTrigger(openAnimationTrigger);
        }

        // Play sound
        PlaySound(openSound);

        // Show home screen
        ShowScreen(homeScreen);

        // Pause game time (optional)
        Time.timeScale = 0f;

        // Enable cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnPhoneToggled?.Invoke(true);
    }

    public void ClosePhone() {
        if (!isPhoneOpen) return;

        isPhoneOpen = false;

        // Play animation
        if (phoneAnimator != null) {
            phoneAnimator.SetTrigger(closeAnimationTrigger);
        }

        // Play sound
        PlaySound(closeSound);

        // Clear screen history
        screenHistory.Clear();

        // Resume game time
        Time.timeScale = 1f;

        // Hide cursor (adjust based on your game's needs)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Delayed deactivation for animation
        Invoke(nameof(DeactivatePhoneCanvas), 0.5f);

        OnPhoneToggled?.Invoke(false);
    }

    void DeactivatePhoneCanvas() {
        phoneCanvas.SetActive(false);
    }

    void OpenApp(AppIconSO app) {
        Debug.Log($"OpenApp called for: {app.appName}");
        Debug.Log($"Target screen is null: {app.targetScreen == null}");

        if (app.targetScreen != null) {
            ShowScreen(app.targetScreen);
            OnAppOpened?.Invoke(app.appName);
            Debug.Log($"Opened app: {app.appName}");
        }
        else {
            Debug.LogWarning($"No target screen assigned for app: {app.appName}");
        }
    }

    void ShowScreen(GameObject screen) {
        Debug.Log($"ShowScreen called for: {screen?.name}");
        Debug.Log($"Screen has {screen.transform.childCount} children");

        // List all children
        for (int i = 0; i < screen.transform.childCount; i++) {
            Debug.Log($"  Child {i}: {screen.transform.GetChild(i).name}");
        }

        if (currentScreen != null && currentScreen != screen) {
            screenHistory.Push(currentScreen);
            currentScreen.SetActive(false);
            Debug.Log($"Deactivated previous screen: {currentScreen.name}");
        }

        currentScreen = screen;
        currentScreen.SetActive(true);
        Debug.Log($"Activated new screen: {currentScreen.name}");
    }

    public void GoToHomeScreen() {
        screenHistory.Clear();
        ShowScreen(homeScreen);
    }

    public void GoBack() {
        if (screenHistory.Count > 0) {
            currentScreen.SetActive(false);
            currentScreen = screenHistory.Pop();
            currentScreen.SetActive(true);
        }
    }

    void PlaySound(AudioClip clip) {
        if (phoneAudioSource != null && clip != null) {
            phoneAudioSource.PlayOneShot(clip);
        }
    }

    // Public utility methods
    public void UnlockApp(string appName) {
        AppIconSO app = phoneApps.FirstOrDefault(a => a.appName == appName);
        if (app != null) {
            // Since AppIconSO doesn't have unlock state, we'll just log it
            Debug.Log($"App unlocked (no-op for AppIconSO): {appName}");
        }
    }

    public void LockApp(string appName) {
        AppIconSO app = phoneApps.FirstOrDefault(a => a.appName == appName);
        if (app != null) {
            // Since AppIconSO doesn't have lock state, we'll just log it
            Debug.Log($"App locked (no-op for AppIconSO): {appName}");
        }
    }

    public bool IsPhoneOpen() {
        return isPhoneOpen;
    }

    public void SetPhoneTime(bool pauseTime) {
        Time.timeScale = pauseTime ? 0f : 1f;
    }

    // Method to add new apps dynamically
    public void AddApp(AppIconSO newApp) {
        if (!phoneApps.Contains(newApp)) {
            phoneApps.Add(newApp);
            CreateAppButton(newApp);
        }
    }
}