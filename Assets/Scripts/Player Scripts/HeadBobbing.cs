using UnityEngine;
using Photon.Pun;

public class HeadBobbing : MonoBehaviourPun
{
    [Header("Head Bobbing Settings")]
    [Range(0.001f, 0.05f)]
    public float amount = 0.002f;

    [Range(1f, 20f)]
    public float frequency = 10f;

    [Range(5f, 20f)]
    public float smoothness = 10f;

    [Header("Sprinting Settings")]
    [Range(1f, 3f)]
    public float sprintMultiplier = 1.6f;

    private PhotonView pv;
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private float timer;

    private bool isSprinting;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
    }

    void Update()
    {
        if (!pv.IsMine) return;
        isSprinting = Input.GetButton("Sprint");

        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        float speed = input.magnitude;

        // Apply sprint frequency boost
        float currentFrequency = isSprinting ? frequency * sprintMultiplier : frequency;

        if (speed > 0.01f)
            timer += Time.deltaTime * currentFrequency;
        else
            timer = 0f;

        // Head bobbing only
        Vector3 bobOffset = Vector3.zero;
        if (speed > 0.01f)
        {
            bobOffset.y = Mathf.Sin(timer) * amount * 1.4f;
            bobOffset.x = Mathf.Cos(timer / 2f) * amount;
        }

        // Smooth bob movement
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            originalLocalPos + bobOffset,
            smoothness * Time.deltaTime
        );

        // Keep original rotation (no tilt, no sway)
        transform.localRotation = originalLocalRot;
    }
}
