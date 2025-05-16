using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotateSpeed = 100f;

    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject meshObject;

    private CharacterController characterController;
    private Vector3 velocity;
    private Vector2 rotation;
    private bool isGrounded;

    private void Awake()
    {
        // Автоподключение компонентов
        characterController = GetComponent<CharacterController>();

        if (!playerCamera) playerCamera = Camera.main?.transform;
        if (!animator) animator = GetComponentInChildren<Animator>(true);
        if (!meshObject) meshObject = GetComponentInChildren<SkinnedMeshRenderer>()?.gameObject;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleCameraRotation();
        HandleJump();
        UpdateAnimator();
    }

    private void HandleGroundCheck()
    {
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
            Vector3 move = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            characterController.Move(move.normalized * speed * Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleCameraRotation()
    {
        rotation.y += Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
        rotation.x = Mathf.Clamp(rotation.x - Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        transform.rotation = Quaternion.Euler(0, rotation.y, 0);
    }

    private void HandleJump()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            if (animator) animator.SetTrigger("Jump");
        }
    }

    private void UpdateAnimator()
    {
        if (!animator) return;

        float speed = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude;
        animator.SetBool("isWalking", speed > 0.1f);
        animator.SetBool("isGrounded", isGrounded);
    }

    // Метод для события приземления
    private void OnLand()
    {
        if (animator) animator.Play("Land");
        // Дополнительная логика при приземлении
    }
    public class FootstepHandler : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float footstepInterval = 0.5f;
        [SerializeField] private float velocityThreshold = 0.1f;

        [Header("Events")]
        public UnityEvent<float> onFootstep;

        private float lastFootstepTime;
        private Vector3 lastPosition;
        private Animator animator;

        private void Awake()
        {
            // Автоматически находим Animator в дочерних объектах
            animator = GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogWarning("Animator not found in children!", this);
            }
        }

        // Метод для вызова из AnimationEvent
        public void OnFootstep(AnimationEvent evt)
        {
            HandleFootstep(1f); // 1f = полная скорость
        }

        private void UpdateFootstep()
        {
            if (animator == null) return;

            float movementSpeed = (transform.position - lastPosition).magnitude / Time.deltaTime;
            lastPosition = transform.position;

            if (movementSpeed > velocityThreshold &&
                Time.time - lastFootstepTime > footstepInterval)
            {
                HandleFootstep(movementSpeed);
                lastFootstepTime = Time.time;
            }
        }

        public void HandleFootstep(float speedNormalized)
        {
            onFootstep.Invoke(Mathf.Clamp01(speedNormalized / velocityThreshold));
        }
    }
}