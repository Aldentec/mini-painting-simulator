using UnityEngine;

public class ModelRotationController : MonoBehaviour {
    [Header("Manual Controls")]
    public KeyCode rotateXKey = KeyCode.X;
    public KeyCode rotateYKey = KeyCode.Y;
    public KeyCode rotateZKey = KeyCode.Z;
    public KeyCode resetKey = KeyCode.R;
    public float manualRotationStep = 15f;

    [Header("Precision Controls")]
    public KeyCode adjustForwardKey = KeyCode.T;    // Lean forward
    public KeyCode adjustBackwardKey = KeyCode.G;   // Lean backward  
    public KeyCode adjustLeftKey = KeyCode.F;       // Lean left
    public KeyCode adjustRightKey = KeyCode.H;      // Lean right
    public KeyCode perfectUprightKey = KeyCode.U;   // Perfect upright

    private GameObject currentModel;
    private Vector3 currentRotation;
    private CharacterOrientationSystem orientationSystem;
    private ModelType currentModelType;

    public GameObject CurrentModel => currentModel;
    public Vector3 CurrentRotation => currentRotation;

    void Start() {
        orientationSystem = FindObjectOfType<CharacterOrientationSystem>();
        if (orientationSystem == null) {
            orientationSystem = gameObject.AddComponent<CharacterOrientationSystem>();
        }
    }

    void Update() {
        if (currentModel == null) return;

        HandleManualControls();
        HandlePrecisionControls();
        HandleRealTimeAdjustments();
    }

    void HandleManualControls() {
        if (Input.GetKeyDown(rotateXKey))
            RotateModel(manualRotationStep, 0, 0);
        if (Input.GetKeyDown(rotateYKey))
            RotateModel(0, manualRotationStep, 0);
        if (Input.GetKeyDown(rotateZKey))
            RotateModel(0, 0, manualRotationStep);
        if (Input.GetKeyDown(resetKey))
            ResetRotation();
    }

    void HandlePrecisionControls() {
        var settings = orientationSystem.Settings;

        if (Input.GetKeyDown(adjustForwardKey))
            AdjustUpright(-settings.fineAdjustmentStep, 0);
        if (Input.GetKeyDown(adjustBackwardKey))
            AdjustUpright(settings.fineAdjustmentStep, 0);
        if (Input.GetKeyDown(adjustLeftKey))
            AdjustUpright(0, -settings.fineAdjustmentStep);
        if (Input.GetKeyDown(adjustRightKey))
            AdjustUpright(0, settings.fineAdjustmentStep);
        if (Input.GetKeyDown(perfectUprightKey))
            SetPerfectUpright();
    }

    void HandleRealTimeAdjustments() {
        var settings = orientationSystem.Settings;

        if (settings.realTimeAdjustment) {
            Vector3 targetRotation = new Vector3(
                settings.uprightAdjustmentX,
                currentRotation.y,
                settings.uprightAdjustmentZ
            );

            if (Vector3.Distance(currentRotation, targetRotation) > 0.1f) {
                currentRotation = targetRotation;
                currentModel.transform.rotation = Quaternion.Euler(currentRotation);
            }
        }
    }

    public void SetModel(GameObject model, ModelType modelType = ModelType.Character) {
        currentModel = model;
        currentModelType = modelType;

        if (model != null) {
            Vector3 optimalRotation = orientationSystem.DetermineOptimalRotation(model, modelType);
            currentRotation = optimalRotation;
            model.transform.rotation = Quaternion.Euler(optimalRotation);
        }
    }

    void RotateModel(float x, float y, float z) {
        if (currentModel == null) return;

        currentRotation += new Vector3(x, y, z);
        currentModel.transform.rotation = Quaternion.Euler(currentRotation);
        Debug.Log($"Manual rotation applied. New rotation: {currentRotation}");
    }

    void AdjustUpright(float xAdjust, float zAdjust) {
        var settings = orientationSystem.Settings;

        settings.uprightAdjustmentX += xAdjust;
        settings.uprightAdjustmentZ += zAdjust;

        settings.uprightAdjustmentX = Mathf.Clamp(settings.uprightAdjustmentX, -45f, 45f);
        settings.uprightAdjustmentZ = Mathf.Clamp(settings.uprightAdjustmentZ, -45f, 45f);

        ApplyCurrentAdjustments();
        Debug.Log($"Upright adjusted - X: {settings.uprightAdjustmentX:F1}°, Z: {settings.uprightAdjustmentZ:F1}°");
    }

    void SetPerfectUpright() {
        var settings = orientationSystem.Settings;
        settings.uprightAdjustmentX = 0f;
        settings.uprightAdjustmentZ = 0f;
        ApplyCurrentAdjustments();
        Debug.Log("Set to perfect upright (0, Y, 0)");
    }

    void ApplyCurrentAdjustments() {
        if (currentModel == null) return;

        var settings = orientationSystem.Settings;
        currentRotation.x = settings.uprightAdjustmentX;
        currentRotation.z = settings.uprightAdjustmentZ;
        currentModel.transform.rotation = Quaternion.Euler(currentRotation);
    }

    public void ResetRotation() {
        if (currentModel == null) return;

        currentModel.transform.rotation = Quaternion.identity;
        Vector3 detectedRotation = orientationSystem.DetermineOptimalRotation(currentModel, currentModelType);
        currentRotation = detectedRotation;
        currentModel.transform.rotation = Quaternion.Euler(detectedRotation);
        Debug.Log($"Reset to optimal orientation: {detectedRotation}");
    }

    // UI Methods
    public void RotateX() => RotateModel(manualRotationStep, 0, 0);
    public void RotateY() => RotateModel(0, manualRotationStep, 0);
    public void RotateZ() => RotateModel(0, 0, manualRotationStep);
    public void RotateXNeg() => RotateModel(-manualRotationStep, 0, 0);
    public void RotateYNeg() => RotateModel(0, -manualRotationStep, 0);
    public void RotateZNeg() => RotateModel(0, 0, -manualRotationStep);

    public void FaceCamera() {
        if (currentModel == null) return;

        currentRotation.y = 180f;
        currentModel.transform.rotation = Quaternion.Euler(currentRotation);
        Debug.Log("Character set to face camera");
    }

    public void StandUpright() {
        if (currentModel == null) return;

        var settings = orientationSystem.Settings;
        currentRotation = new Vector3(settings.uprightAdjustmentX, currentRotation.y, settings.uprightAdjustmentZ);
        currentModel.transform.rotation = Quaternion.Euler(currentRotation);
        Debug.Log("Character set to stand upright with adjustments");
    }

    public void MakeUprightFacingCamera() {
        if (currentModel == null) return;

        var settings = orientationSystem.Settings;
        currentRotation = new Vector3(settings.uprightAdjustmentX, 180f, settings.uprightAdjustmentZ);
        currentModel.transform.rotation = Quaternion.Euler(currentRotation);
        Debug.Log($"Character set to upright facing camera: {currentRotation}");
    }

    // Fine adjustment methods for UI buttons
    public void AdjustForward() => AdjustUpright(-orientationSystem.Settings.fineAdjustmentStep, 0);
    public void AdjustBackward() => AdjustUpright(orientationSystem.Settings.fineAdjustmentStep, 0);
    public void AdjustLeft() => AdjustUpright(0, -orientationSystem.Settings.fineAdjustmentStep);
    public void AdjustRight() => AdjustUpright(0, orientationSystem.Settings.fineAdjustmentStep);

    public void TinyAdjustForward() => AdjustUpright(-0.1f, 0);
    public void TinyAdjustBackward() => AdjustUpright(0.1f, 0);
    public void TinyAdjustLeft() => AdjustUpright(0, -0.1f);
    public void TinyAdjustRight() => AdjustUpright(0, 0.1f);

    void OnDrawGizmos() {
        if (orientationSystem != null && currentModel != null) {
            orientationSystem.DrawDebugGizmos(currentModel);
        }
    }
}