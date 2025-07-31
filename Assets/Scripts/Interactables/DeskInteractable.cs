using UnityEngine;
using UnityEngine.SceneManagement;

public class DeskInteractable : MonoBehaviour {
    public string tooltipText = "Press E to paint";
    public GameObject tooltipUI;

    private bool playerNearby = false;

    private void Start() {
        if (tooltipUI != null) {
            tooltipUI.SetActive(false);
        }
    }

    private void Update() {
        if (playerNearby && Input.GetKeyDown(KeyCode.E)) {
            SceneManager.LoadScene("PaintingScene");
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            playerNearby = true;
            if (tooltipUI != null)
                tooltipUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            playerNearby = false;
            if (tooltipUI != null)
                tooltipUI.SetActive(false);
        }
    }
}
