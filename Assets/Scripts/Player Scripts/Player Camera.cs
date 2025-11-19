using UnityEngine;
using Photon.Pun;

public class PlayerCamera : MonoBehaviour
{
    [Header("Player Camera Settings")]
    public Transform cameraRoot;
    public float sensitivity = 200f;
    public float minPitch = -80f;
    public float maxPitch = 80f;
    public float mouseSmoothTime = 0.02f;

    private float pitch;
    private Vector2 smoothMouseInput;
    private Vector2 smoothMouseVelocity;

    private PhotonView pv;

    void Awake()
    {
        pv = GetComponentInParent<PhotonView>();
        // playerCamera is a child object, so we get the PhotonView from the parent object which is the player
    }

    void Start()
    {
        if (!pv.IsMine)
        {
            // disable camera for remote players
            Camera cam = cameraRoot.GetComponentInChildren<Camera>();
            if (cam != null) cam.enabled = false;

            AudioListener listener = cameraRoot.GetComponentInChildren<AudioListener>();
            if (listener != null) listener.enabled = false;

            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!pv.IsMine) return;  // ensures only local player controls the camera

        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        smoothMouseInput = Vector2.SmoothDamp(
            smoothMouseInput,
            mouseInput,
            ref smoothMouseVelocity,
            mouseSmoothTime
        );

        transform.Rotate(Vector3.up * smoothMouseInput.x * sensitivity * Time.deltaTime);

        pitch -= smoothMouseInput.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
