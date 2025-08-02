using UnityEngine;

[CreateAssetMenu(menuName = "UI/AppIcon")]
public class AppIconSO : ScriptableObject {
    public string appName;
    public Sprite appIcon;
    public GameObject targetScreen;
}
