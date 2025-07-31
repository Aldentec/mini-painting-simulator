using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ModelSelectionUI : MonoBehaviour {
    public Transform spawnPoint;
    public GameObject modelButtonPrefab;
    public Transform buttonContainer;
    public List<PaintableModelSO> availableModels;
    public GameObject modelSelectionPanel; 

    private GameObject currentModel;

    void Start() {
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
        if (currentModel != null) Destroy(currentModel);
        Quaternion fixedRotation = Quaternion.Euler(-90f, 0f, 0f); // Adjust as needed
        currentModel = Instantiate(model.modelPrefab, spawnPoint.position, fixedRotation);


        // Ensure the object has its own texture to paint on
        Renderer rend = currentModel.GetComponentInChildren<Renderer>();
        if (rend != null) {
            Material mat = Instantiate(rend.material); // Clone the material
            Texture2D originalTex = mat.mainTexture as Texture2D;

            if (originalTex != null) {
                Texture2D runtimeTex = new Texture2D(originalTex.width, originalTex.height, originalTex.format, false);
                runtimeTex.SetPixels(originalTex.GetPixels());
                runtimeTex.Apply();

                mat.mainTexture = runtimeTex;
            }

            rend.material = mat;
        }

        // Hide panel, set OrbitCamera, etc.
        if (modelSelectionPanel != null)
            modelSelectionPanel.SetActive(false);

        OrbitCamera orbitCam = FindObjectOfType<OrbitCamera>();
        if (orbitCam != null)
            orbitCam.target = currentModel.transform;
    }

}
