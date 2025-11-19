using Photon.Pun;
using UnityEngine;

public class RB_PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float maxVelocityChange = 10f;
    public float jumpForce = 30f;

    private bool isSprinting;
    private bool isJumping;
    private bool isGrounded;

    private Vector2 input;
    private Rigidbody rb;
    private PhotonView pv;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();

        // local-only setup
        if (pv.IsMine)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        rb.linearDamping = 2.5f;
    }

    void Update()
    {
        if (!pv.IsMine) return;

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Normalize();

        isSprinting = Input.GetButton("Sprint");
        isJumping = Input.GetButton("Jump");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!pv.IsMine) return;
        isGrounded = true;
    }

    private void FixedUpdate()
    {
        if (!pv.IsMine) return;

        rb.AddForce(Physics.gravity * 3f, ForceMode.Acceleration);
        rb.AddForce(CalculateMovement(isSprinting ? sprintSpeed : walkSpeed), ForceMode.VelocityChange);

        if (isJumping && isGrounded)
        {
            Vector3 jump = new Vector3(0, jumpForce, 0);
            rb.AddForce(jump, ForceMode.Impulse);
        }

        isGrounded = false;
    }

    Vector3 CalculateMovement(float _speed)
    {
        Vector3 targetVelocity = new Vector3(input.x, 0, input.y);
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= _speed;

        Vector3 velocity = rb.linearVelocity;

        if (input.magnitude > 0.5f)
        {
            Vector3 velocityChange = targetVelocity - velocity;

            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;

            return velocityChange;
        }
        else
        {
            return Vector3.zero;
        }
    }
}
