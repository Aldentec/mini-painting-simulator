using UnityEngine;
using Seagull.Interior_04E.SceneProps;

public class PaintingSceneManager : MonoBehaviour {
    public GlowLight deskLamp;

    void Start() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (deskLamp != null) {
            deskLamp.turnOn();
        }
    }
}
