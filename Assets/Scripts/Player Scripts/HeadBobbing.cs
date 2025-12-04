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

    [Header("Mouse Look Tilt Settings")]
    [Range(0f, 5f)]
    public float tiltAmount = 2f;
    [Range(1f, 20f)]
    public float tiltSmoothness = 8f;

    private PhotonView pv;
    private Vector3 originalLocalPos;
    private float timer;

    private float currentTilt;
    private float targetTilt;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        originalLocalPos = transform.localPosition;
        currentTilt = 0f;
        targetTilt = 0f;
    }

    void Update()
    {
        if (!pv.IsMine) return;
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        float speed = input.magnitude;

        if (speed > 0.01f)
            timer += Time.deltaTime * frequency;
        else
            timer = 0f;

        Vector3 bobOffset = Vector3.zero;
        if (speed > 0.01f)
        {
            bobOffset.y = Mathf.Sin(timer) * amount * 1.4f;
            bobOffset.x = Mathf.Cos(timer / 2f) * amount;
        }

        float mouseX = Input.GetAxis("Mouse X");
        targetTilt = -mouseX * tiltAmount;

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSmoothness * Time.deltaTime);

        transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPos + bobOffset, smoothness * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, currentTilt);
    }
}
