using UnityEngine;
using UnityEngine.UI;

public class TexturePainter : MonoBehaviour {
    public Camera cam;
    public RenderTexture paintTexture;
    public Shader paintShader;
    public static bool isPainting = false;

    [Header("Brush Settings")]
    public Color brushColor = Color.red;
    public float brushSize = 0.1f;
    public float brushOpacity = 1f;

    [Header("UI")]
    public Slider sizeSlider;
    public Slider opacitySlider;
    public Slider rSlider, gSlider, bSlider;
    public Image brushPreviewImage;

    private Material _runtimePaintMaterial;

    private void Start() {
        _runtimePaintMaterial = new Material(paintShader);

        RenderTexture.active = paintTexture;
        GL.Clear(true, true, Color.white);
        RenderTexture.active = null;

        if (sizeSlider) sizeSlider.onValueChanged.AddListener(v => brushSize = v);
        if (opacitySlider) opacitySlider.onValueChanged.AddListener(v => brushOpacity = v);

        if (rSlider) rSlider.onValueChanged.AddListener(v => UpdateColor());
        if (gSlider) gSlider.onValueChanged.AddListener(v => UpdateColor());
        if (bSlider) bSlider.onValueChanged.AddListener(v => UpdateColor());

        UpdateColor();
    }

    void UpdateColor() {
        brushColor = new Color(rSlider.value, gSlider.value, bSlider.value);
        if (brushPreviewImage != null)
            brushPreviewImage.color = brushColor;
    }

    void Update() {
        isPainting = false; // reset every frame

        if (Input.GetMouseButton(0)) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                // Only paint if the surface is facing the camera (avoids mirrored UVs)
                if (Vector3.Dot(hit.normal, -ray.direction) > 0.5f) {
                    if (hit.collider != null) {
                        isPainting = true; // let camera know we're painting

                        _runtimePaintMaterial.SetVector("_UV", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));
                        _runtimePaintMaterial.SetColor("_Color", brushColor);
                        _runtimePaintMaterial.SetFloat("_Size", brushSize);
                        _runtimePaintMaterial.SetFloat("_Opacity", brushOpacity);

                        RenderTexture temp = RenderTexture.GetTemporary(paintTexture.width, paintTexture.height, 0, paintTexture.format);
                        Graphics.Blit(paintTexture, temp);
                        _runtimePaintMaterial.SetTexture("_MainTex", temp);
                        Graphics.Blit(temp, paintTexture, _runtimePaintMaterial);
                        RenderTexture.ReleaseTemporary(temp);
                    }
                }
            }
        }
    }

}
