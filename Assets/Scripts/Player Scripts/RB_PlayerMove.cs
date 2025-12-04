using Photon.Pun;
using UnityEngine;

public class RB_PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float maxVelocityChange = 10f;
    public float jumpForce = 30f;


    [Header("Animation")]
    public Animator animator;
    public Transform modelRoot;

    private bool isSprinting;
    private bool isJumping;
    private bool isGrounded;
    private bool jumpTriggered;

    private Vector2 input;
    private Rigidbody rb;
    private PhotonView pv;

    public float groundCheckRadius = 0.35f;
    public float groundCheckDistance = 0.6f;
    public LayerMask groundMask;

    private SoundType? currentMoveSound = null;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();

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
        isJumping = Input.GetButtonDown("Jump");

        // --- Jump ---
        if (isJumping && isGrounded && !jumpTriggered)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            SoundManager.PlaySound(SoundType.JUMP);

            if (animator)
                animator.SetTrigger("Jump");

            jumpTriggered = true;
        }

        // --- Animator ---
        if (animator)
        {
            float speedPercent = rb.linearVelocity.magnitude / sprintSpeed;
            animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);
            animator.SetBool("IsGrounded", isGrounded);
        }

        // --- Movement Sounds ---
        if (animator && isGrounded && input.magnitude > 0.1f)
        {
            SoundType targetSound = isSprinting ? SoundType.RUN : SoundType.WALK;

            if (currentMoveSound != targetSound)
            {
                if (currentMoveSound.HasValue)
                    SoundManager.StopLoopingSound(currentMoveSound.Value);

                SoundManager.PlayLoopingSound(targetSound, 1);
                currentMoveSound = targetSound;
            }
        }
        else
        {
            if (currentMoveSound.HasValue)
            {
                SoundManager.StopLoopingSound(currentMoveSound.Value);
                currentMoveSound = null;
            }
        }

        // Rotate model
        if (modelRoot && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Vector3 look = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Quaternion targetRot = Quaternion.LookRotation(look);
            modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    private void FixedUpdate()
    {
        if (!pv.IsMine) return;

        Vector3 groundCheckPos = transform.position + Vector3.down * 0.1f;
        isGrounded = Physics.CheckSphere(groundCheckPos, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        rb.AddForce(Physics.gravity * 3f, ForceMode.Acceleration);
        rb.AddForce(CalculateMovement(isSprinting ? sprintSpeed : walkSpeed), ForceMode.VelocityChange);

        if (isGrounded && rb.linearVelocity.y <= 0.01f)
            jumpTriggered = false;
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
        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 0, groundCheckRadius);
    }
}
