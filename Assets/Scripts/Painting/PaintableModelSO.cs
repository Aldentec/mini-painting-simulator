using UnityEngine;

[CreateAssetMenu(menuName = "Painting/PaintableModel")]
public class PaintableModelSO : ScriptableObject {
    public string modelName;
    public GameObject modelPrefab;
    public Sprite modelIcon;
}
