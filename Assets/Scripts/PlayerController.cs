using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public class MovementSettings
    {
        public float walkSpeed = 4f;
        public float runSpeed = 8f;
        public float crouchSpeed = 2f;
        public float acceleration = 10f;
        public float deceleration = 10f;
        public float airControl = 0.3f;
    }

    [System.Serializable]
    public class JumpSettings
    {
        public float jumpForce = 5f;
        public float gravity = -20f;
        public float maxFallSpeed = 40f;
        public float coyoteTime = 0.15f;
        public float jumpBufferTime = 0.1f;
    }

    [System.Serializable]
    public class CrouchSettings
    {
        public float crouchHeight = 1f;
        public float standHeight = 2f;
        public float crouchTransitionSpeed = 8f;
    }

    [System.Serializable]
    public class HeadBobSettings
    {
        public bool enableBob = true;
        public float walkBobSpeed = 8f;
        public float walkBobAmount = 0.03f;
        public float runBobSpeed = 12f;
        public float runBobAmount = 0.05f;
    }

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("Components")]
    public CharacterController controller;
    public Camera playerCamera;

    [Header("Settings")]
    public MovementSettings movement = new MovementSettings();
    public JumpSettings jump = new JumpSettings();
    public CrouchSettings crouch = new CrouchSettings();
    public HeadBobSettings headBob = new HeadBobSettings();

    private Vector3 velocity;
    private float currentSpeed;
    private Vector3 moveDirection;
    private Vector3 targetDirection;

    private bool isCrouching;
    private bool wantsToCrouch;
    private float standingHeight;
    private float cameraHeight;

    private float standingCameraY;
    private float crouchingCameraY;
    private float currentBaseCameraY;
    private float targetBaseCameraY;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private float bobTimer;
    private float verticalRotation = 0;

    void Start()
    {
        if (!controller) controller = GetComponent<CharacterController>();
        if (!playerCamera) playerCamera = GetComponentInChildren<Camera>();

        standingHeight = controller.height;
        cameraHeight = playerCamera.transform.localPosition.y;

        standingCameraY = cameraHeight;
        crouchingCameraY = cameraHeight - (standingHeight - crouch.crouchHeight) / 2f;

        currentBaseCameraY = standingCameraY;
        targetBaseCameraY = standingCameraY;

        Cursor.lockState = CursorLockMode.Locked;

        // Подписываемся на изменение настроек
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMouseSensitivityChanged += OnMouseSensitivityChanged;

            // Загружаем текущую чувствительность
            mouseSensitivity = SettingsManager.Instance.GetMouseSensitivity();
        }
    }

    void OnDestroy()
    {
        // Отписываемся при уничтожении
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMouseSensitivityChanged -= OnMouseSensitivityChanged;
        }
    }

    void OnMouseSensitivityChanged(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
        Debug.Log($"PlayerController: чувствительность обновлена до {newSensitivity}");
    }

    // Публичный метод для обновления чувствительности извне
    public void UpdateMouseSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
    }

    void Update()
    {
        HandleMouseLook();
        HandleTimers();
        HandleCrouchInput();
        HandleMovement();
        HandleGravity();
        HandleJump();

        currentBaseCameraY = Mathf.Lerp(currentBaseCameraY, targetBaseCameraY, crouch.crouchTransitionSpeed * Time.deltaTime);

        float bobOffset = 0f;
        if (headBob.enableBob && controller.isGrounded && currentSpeed > 0.1f)
        {
            bobOffset = CalculateHeadBob();
        }

        Vector3 cameraPos = playerCamera.transform.localPosition;
        cameraPos.y = currentBaseCameraY + bobOffset;
        playerCamera.transform.localPosition = cameraPos;

        ApplyMovement();

        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void HandleCrouchInput()
    {
        wantsToCrouch = Input.GetKey(KeyCode.LeftControl);

        if (wantsToCrouch != isCrouching)
        {
            isCrouching = wantsToCrouch;

            float targetHeight = isCrouching ? crouch.crouchHeight : standingHeight;
            controller.height = Mathf.Lerp(controller.height, targetHeight, crouch.crouchTransitionSpeed * Time.deltaTime);

            float centerY = controller.height / 2f;
            controller.center = new Vector3(0, centerY, 0);

            targetBaseCameraY = isCrouching ? crouchingCameraY : standingCameraY;
        }

        float targetHeight2 = isCrouching ? crouch.crouchHeight : standingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight2, crouch.crouchTransitionSpeed * Time.deltaTime);

        float centerY2 = controller.height / 2f;
        controller.center = new Vector3(0, centerY2, 0);
    }

    float CalculateHeadBob()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
        float bobSpeed = isRunning ? headBob.runBobSpeed : headBob.walkBobSpeed;
        float bobAmount = isRunning ? headBob.runBobAmount : headBob.walkBobAmount;

        bobTimer += Time.deltaTime * bobSpeed;
        return Mathf.Sin(bobTimer) * bobAmount;
    }

    void HandleTimers()
    {
        if (controller.isGrounded)
            coyoteTimeCounter = jump.coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jump.jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        targetDirection = (forward * vertical + right * horizontal).normalized;

        float targetSpeed = movement.walkSpeed;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching && vertical > 0;

        if (isRunning)
            targetSpeed = movement.runSpeed;
        else if (isCrouching)
            targetSpeed = movement.crouchSpeed;

        float acceleration = controller.isGrounded ? movement.acceleration : movement.acceleration * movement.airControl;
        float deceleration = controller.isGrounded ? movement.deceleration : movement.deceleration * movement.airControl;

        if (targetDirection.magnitude > 0.1f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            moveDirection = targetDirection * currentSpeed;

            if (!headBob.enableBob)
                bobTimer += Time.deltaTime;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.deltaTime);
            moveDirection = Vector3.zero;
            bobTimer = 0;
        }
    }

    void HandleGravity()
    {
        if (!controller.isGrounded)
        {
            velocity.y += jump.gravity * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, -jump.maxFallSpeed);
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void HandleJump()
    {
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            velocity.y = jump.jumpForce;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }
    }

    void ApplyMovement()
    {
        Vector3 horizontalMove = moveDirection;
        controller.Move(horizontalMove * Time.deltaTime);
        controller.Move(velocity * Time.deltaTime);
    }
}