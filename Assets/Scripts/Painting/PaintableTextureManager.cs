using UnityEngine;

public class PaintableTextureManager : MonoBehaviour {
    [Header("Texture Settings")]
    public bool enableDebugLogs = true;

    public void SetupPaintableTexture(GameObject model) {
        if (model == null) {
            LogDebug("Model is null, cannot setup paintable texture");
            return;
        }

        Renderer rend = model.GetComponentInChildren<Renderer>();
        if (rend == null) {
            LogDebug("No renderer found on model");
            return;
        }

        // Clone the material so we don't modify the original
        Material mat = Instantiate(rend.material);
        Texture2D originalTex = mat.mainTexture as Texture2D;

        if (originalTex != null) {
            // Create a runtime copy of the texture for painting
            Texture2D runtimeTex = new Texture2D(originalTex.width, originalTex.height, originalTex.format, false);
            runtimeTex.SetPixels(originalTex.GetPixels());
            runtimeTex.Apply();
            mat.mainTexture = runtimeTex;

            LogDebug($"Created paintable texture: {originalTex.width}x{originalTex.height}");
        }
        else {
            LogDebug("No main texture found on material");
        }

        rend.material = mat;
    }

    public Texture2D GetPaintableTexture(GameObject model) {
        if (model == null) return null;

        Renderer rend = model.GetComponentInChildren<Renderer>();
        if (rend == null) return null;

        return rend.material.mainTexture as Texture2D;
    }

    public Material GetPaintableMaterial(GameObject model) {
        if (model == null) return null;

        Renderer rend = model.GetComponentInChildren<Renderer>();
        return rend?.material;
    }

    void LogDebug(string message) {
        if (enableDebugLogs) {
            Debug.Log($"[PaintableTexture] {message}");
        }
    }
}