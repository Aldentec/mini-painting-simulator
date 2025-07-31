using UnityEngine;
using UnityEngine.EventSystems;

public class OrbitCamera : MonoBehaviour {
    public Transform target;

    [Header("Distance")]
    public float distance = 5f;
    public float zoomSpeed = 10f;
    public float minDistance = 0.001f;
    public float maxDistance = 10f;

    [Header("Rotation")]
    public float xSpeed = 300f;
    public float ySpeed = 300f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    [Header("Panning")]
    public float panSpeed = 1.5f;

    [Header("Smoothing")]
    public float smoothTime = 0.1f;

    private float x = 0f;
    private float y = 0f;

    private float xVelocity = 0f;
    private float yVelocity = 0f;
    private float currentDistance;
    private float distanceVelocity = 0f;

    private Vector3 panOffset = Vector3.zero;

    void Start() {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        currentDistance = distance;

        // No need to log a warning — we'll wait until target is set
    }

    void LateUpdate() {
        if (target == null) return;

        // Block input if over UI or actively painting
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (TexturePainter.isPainting) return;

        // Orbit with right mouse drag
        if (Input.GetMouseButton(1)) {
            float xInput = Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
            float yInput = Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;

            x += xInput;
            y -= yInput;
            y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
        }

        // Pan with left mouse drag
        if (Input.GetMouseButton(0)) {
            float panX = -Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime * 1000f;
            float panY = -Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime * 1000f;

            Vector3 pan = transform.right * panX + transform.up * panY;
            panOffset += pan;
        }

        // Zoom with scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Smooth interpolation
        x = Mathf.SmoothDampAngle(transform.eulerAngles.y, x, ref xVelocity, smoothTime);
        y = Mathf.SmoothDampAngle(transform.eulerAngles.x, y, ref yVelocity, smoothTime);
        currentDistance = Mathf.SmoothDamp(currentDistance, distance, ref distanceVelocity, smoothTime);

        // Final position and rotation
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -currentDistance);
        transform.position = target.position + panOffset + rotation * negDistance;
        transform.rotation = rotation;
    }
}
