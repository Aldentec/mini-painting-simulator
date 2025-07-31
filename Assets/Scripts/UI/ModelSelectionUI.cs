using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ModelSelectionUI : MonoBehaviour {
    [Header("UI References")]
    public Transform spawnPoint;
    public GameObject modelButtonPrefab;
    public Transform buttonContainer;
    public List<PaintableModelSO> availableModels;
    public GameObject modelSelectionPanel;

    [Header("Model Settings")]
    public ModelType defaultModelType = ModelType.Character;

    private GameObject currentModel;
    private ModelRotationController rotationController;
    private PaintableTextureManager textureManager;

    void Start() {
        SetupComponents();
        CreateModelButtons();
    }

    void SetupComponents() {
        // Get or create rotation controller
        rotationController = FindObjectOfType<ModelRotationController>();
        if (rotationController == null) {
            rotationController = gameObject.AddComponent<ModelRotationController>();
        }

        // Get or create texture manager
        textureManager = FindObjectOfType<PaintableTextureManager>();
        if (textureManager == null) {
            textureManager = gameObject.AddComponent<PaintableTextureManager>();
        }
    }

    void CreateModelButtons() {
        foreach (var model in availableModels) {
            GameObject buttonObj = Instantiate(modelButtonPrefab, buttonContainer);
            Button btn = buttonObj.GetComponent<Button>();
            Image img = buttonObj.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI label = buttonObj.transform.Find("Label").GetComponent<TextMeshProUGUI>();

            img.sprite = model.modelIcon;
            label.text = model.modelName;
            btn.onClick.AddListener(() => SpawnModel(model));
        }
    }

    void SpawnModel(PaintableModelSO model) {
        // Clean up previous model
        if (currentModel != null)
            Destroy(currentModel);

        // Spawn new model
        currentModel = Instantiate(model.modelPrefab, spawnPoint.position, Quaternion.identity);

        // Setup rotation
        rotationController.SetModel(currentModel, defaultModelType);

        // Setup paintable texture
        textureManager.SetupPaintableTexture(currentModel);

        // Hide selection panel
        if (modelSelectionPanel != null)
            modelSelectionPanel.SetActive(false);

        // Setup camera
        SetupOrbitCamera();

        Debug.Log($"Spawned model: {model.modelName}");
    }

    void SetupOrbitCamera() {
        OrbitCamera orbitCam = FindObjectOfType<OrbitCamera>();
        if (orbitCam != null && currentModel != null)
            orbitCam.target = currentModel.transform;
    }

    // Public methods for UI buttons
    public void RotateX() => rotationController?.RotateX();
    public void RotateY() => rotationController?.RotateY();
    public void RotateZ() => rotationController?.RotateZ();
    public void RotateXNeg() => rotationController?.RotateXNeg();
    public void RotateYNeg() => rotationController?.RotateYNeg();
    public void RotateZNeg() => rotationController?.RotateZNeg();

    public void FaceCamera() => rotationController?.FaceCamera();
    public void StandUpright() => rotationController?.StandUpright();
    public void MakeUprightFacingCamera() => rotationController?.MakeUprightFacingCamera();
    public void ResetRotation() => rotationController?.ResetRotation();

    // Fine adjustment methods
    public void AdjustForward() => rotationController?.AdjustForward();
    public void AdjustBackward() => rotationController?.AdjustBackward();
    public void AdjustLeft() => rotationController?.AdjustLeft();
    public void AdjustRight() => rotationController?.AdjustRight();

    public void TinyAdjustForward() => rotationController?.TinyAdjustForward();
    public void TinyAdjustBackward() => rotationController?.TinyAdjustBackward();
    public void TinyAdjustLeft() => rotationController?.TinyAdjustLeft();
    public void TinyAdjustRight() => rotationController?.TinyAdjustRight();

    // Getters for other systems
    public GameObject CurrentModel => currentModel;
}