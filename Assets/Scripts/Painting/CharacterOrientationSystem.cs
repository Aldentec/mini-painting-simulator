using UnityEngine;

[System.Serializable]
public class CharacterOrientationSettings {
    [Header("Character Model Settings")]
    [Tooltip("For character models - should face camera and stand upright")]
    public bool forceCharacterOrientation = true;
    public Vector3 characterRotation = new Vector3(0f, 180f, 0f);

    [Header("Fine-Tuning for Upright Stance")]
    [Range(-45f, 45f)]
    public float uprightAdjustmentX = 0f;
    [Range(-45f, 45f)]
    public float uprightAdjustmentZ = 0f;
    public bool analyzeVerticalAlignment = true;
    public bool realTimeAdjustment = true;

    [Header("Precision Controls")]
    [Range(0.1f, 5f)]
    public float fineAdjustmentStep = 0.5f;

    [Header("Debug")]
    public bool enableDebugLogs = true;
    public bool showBoundsGizmo = false;
    public bool showOrientationArrows = true;
}

public enum CharacterOrientation {
    StandingCorrect,  // Y is tallest, upright
    LyingOnBack,      // Z is tallest, lying flat
    LyingOnSide,      // X is tallest, on side
    UpsideDown,       // Y is tallest but upside down
    FacingAway        // Correct orientation but facing wrong direction
}

public enum ModelType {
    Character,    // Person, animal - should stand upright facing camera
    Object,       // General objects - use smart detection
    Vehicle,      // Cars, etc - usually need specific orientation
    Furniture     // Tables, chairs - usually need to be upright
}

public class CharacterOrientationSystem : MonoBehaviour {
    [SerializeField] private CharacterOrientationSettings settings;

    private Bounds lastModelBounds;

    public CharacterOrientationSettings Settings => settings;

    public Vector3 DetermineOptimalRotation(GameObject model, ModelType modelType) {
        if (settings.forceCharacterOrientation || modelType == ModelType.Character) {
            return SetupCharacterOrientation(model);
        }

        return DetectOptimalRotation(model);
    }

    Vector3 SetupCharacterOrientation(GameObject model) {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) {
            LogDebug("No renderers found, using default character rotation");
            return ApplyFinalAdjustments(settings.characterRotation);
        }

        // Calculate combined bounds
        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }

        lastModelBounds = combinedBounds;
        Vector3 size = combinedBounds.size;

        LogDebug($"Character bounds: X={size.x:F2}, Y={size.y:F2}, Z={size.z:F2}");

        Vector3 rotation = settings.characterRotation;

        // Analyze if the character is lying down or in wrong orientation
        CharacterOrientation orientation = AnalyzeCharacterOrientation(size);

        switch (orientation) {
            case CharacterOrientation.StandingCorrect:
                rotation = new Vector3(0f, 180f, 0f);
                LogDebug("Character is standing correctly");
                break;

            case CharacterOrientation.LyingOnBack:
                rotation = new Vector3(-90f, 180f, 0f);
                LogDebug("Character lying on back, rotating upright");
                break;

            case CharacterOrientation.LyingOnSide:
                if (size.x > size.z) {
                    rotation = new Vector3(0f, 180f, 90f);
                }
                else {
                    rotation = new Vector3(90f, 180f, 0f);
                }
                LogDebug("Character lying on side, rotating upright");
                break;

            case CharacterOrientation.UpsideDown:
                rotation = new Vector3(180f, 180f, 0f);
                LogDebug("Character upside down, flipping");
                break;

            case CharacterOrientation.FacingAway:
                rotation = new Vector3(0f, 0f, 0f);
                LogDebug("Character facing away, turning around");
                break;
        }

        // Fine-tune based on actual geometry
        Vector3 geometryAdjustment = AnalyzeCharacterGeometry(model);
        rotation += geometryAdjustment;

        // Apply vertical alignment analysis
        if (settings.analyzeVerticalAlignment) {
            Vector3 verticalAdjustment = AnalyzeVerticalAlignment(model);
            rotation += verticalAdjustment;
            LogDebug($"Applied vertical alignment adjustment: {verticalAdjustment}");
        }

        return ApplyFinalAdjustments(rotation);
    }

    Vector3 ApplyFinalAdjustments(Vector3 baseRotation) {
        Vector3 finalRotation = baseRotation;
        finalRotation.x += settings.uprightAdjustmentX;
        finalRotation.z += settings.uprightAdjustmentZ;

        LogDebug($"Base rotation: {baseRotation}, Final with adjustments: {finalRotation}");
        return finalRotation;
    }

    Vector3 AnalyzeVerticalAlignment(GameObject model) {
        MeshFilter[] meshFilters = model.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0) return Vector3.zero;

        Vector3 topPoint = Vector3.negativeInfinity;
        Vector3 bottomPoint = Vector3.positiveInfinity;

        foreach (var meshFilter in meshFilters) {
            if (meshFilter.mesh == null) continue;

            Vector3[] vertices = meshFilter.mesh.vertices;
            Transform meshTransform = meshFilter.transform;

            foreach (var vertex in vertices) {
                Vector3 worldVertex = meshTransform.TransformPoint(vertex);
                if (worldVertex.y > topPoint.y) topPoint = worldVertex;
                if (worldVertex.y < bottomPoint.y) bottomPoint = worldVertex;
            }
        }

        Vector3 characterVertical = (topPoint - bottomPoint).normalized;
        Vector3 adjustment = Vector3.zero;

        // Check if character is leaning forward/backward
        float forwardLean = Vector3.Dot(characterVertical, Vector3.forward);
        if (Mathf.Abs(forwardLean) > 0.1f) {
            adjustment.x = -Mathf.Asin(forwardLean) * Mathf.Rad2Deg;
            LogDebug($"Detected forward/backward lean: {forwardLean:F3}, adjusting X by {adjustment.x:F1}°");
        }

        // Check if character is leaning left/right
        float sideLean = Vector3.Dot(characterVertical, Vector3.right);
        if (Mathf.Abs(sideLean) > 0.1f) {
            adjustment.z = Mathf.Asin(sideLean) * Mathf.Rad2Deg;
            LogDebug($"Detected left/right lean: {sideLean:F3}, adjusting Z by {adjustment.z:F1}°");
        }

        return adjustment;
    }

    CharacterOrientation AnalyzeCharacterOrientation(Vector3 size) {
        if (size.y > size.x * 1.3f && size.y > size.z * 1.3f) {
            return CharacterOrientation.StandingCorrect;
        }
        else if (size.x > size.y * 1.3f && size.x > size.z * 1.3f) {
            return CharacterOrientation.LyingOnSide;
        }
        else if (size.z > size.y * 1.3f && size.z > size.x * 1.3f) {
            return CharacterOrientation.LyingOnBack;
        }

        return CharacterOrientation.StandingCorrect;
    }

    Vector3 AnalyzeCharacterGeometry(GameObject model) {
        MeshFilter[] meshFilters = model.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0) return Vector3.zero;

        Vector3 averageNormal = Vector3.zero;
        Vector3 lowestPoint = Vector3.positiveInfinity;
        Vector3 highestPoint = Vector3.negativeInfinity;
        int normalCount = 0;

        foreach (var meshFilter in meshFilters) {
            if (meshFilter.mesh == null) continue;

            Vector3[] vertices = meshFilter.mesh.vertices;
            Vector3[] normals = meshFilter.mesh.normals;
            Transform meshTransform = meshFilter.transform;

            foreach (var vertex in vertices) {
                Vector3 worldVertex = meshTransform.TransformPoint(vertex);
                if (worldVertex.y < lowestPoint.y) lowestPoint = worldVertex;
                if (worldVertex.y > highestPoint.y) highestPoint = worldVertex;
            }

            for (int i = 0; i < normals.Length; i++) {
                Vector3 worldNormal = meshTransform.TransformDirection(normals[i]);
                Vector3 worldVertex = meshTransform.TransformPoint(vertices[i]);

                float heightWeight = Mathf.InverseLerp(lowestPoint.y, highestPoint.y, worldVertex.y);
                if (heightWeight > 0.6f) {
                    averageNormal += worldNormal * heightWeight;
                    normalCount++;
                }
            }
        }

        Vector3 adjustment = Vector3.zero;

        if (normalCount > 0) {
            averageNormal.Normalize();
            LogDebug($"Character front direction: {averageNormal}");

            if (averageNormal.z > 0.5f) {
                adjustment.y += 180f;
                LogDebug("Character facing away, adding 180° Y rotation");
            }
            else if (Mathf.Abs(averageNormal.x) > 0.5f) {
                adjustment.y += -90f * Mathf.Sign(averageNormal.x);
                LogDebug($"Character facing side, adjusting Y rotation by {adjustment.y}°");
            }
        }

        return adjustment;
    }

    Vector3 DetectOptimalRotation(GameObject model) {
        Renderer rend = model.GetComponentInChildren<Renderer>();
        if (rend == null) return Vector3.zero;

        Vector3 size = rend.bounds.size;

        if (size.y > size.x * 1.5f && size.y > size.z * 1.5f) {
            return new Vector3(0f, 180f, 0f);
        }
        else if (size.x > size.y * 1.5f) {
            return new Vector3(0f, 90f, 0f);
        }

        return new Vector3(0f, 180f, 0f);
    }

    public void DrawDebugGizmos(GameObject model) {
        if (model == null) return;

        if (settings.showBoundsGizmo) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(lastModelBounds.center, lastModelBounds.size);
        }

        if (settings.showOrientationArrows) {
            Vector3 center = model.transform.position + Vector3.up * (lastModelBounds.size.y * 0.5f);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(center, model.transform.right * 1f);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(center, model.transform.up * 2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(center, model.transform.forward * 1f);

            Gizmos.color = Color.white;
            Vector3 bottom = model.transform.position;
            Vector3 top = bottom + model.transform.up * lastModelBounds.size.y;
            Gizmos.DrawLine(bottom, top);
        }
    }

    void LogDebug(string message) {
        if (settings.enableDebugLogs) {
            Debug.Log($"[CharacterOrientation] {message}");
        }
    }
}