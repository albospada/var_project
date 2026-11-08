using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class FirstPersonDesktop : MonoBehaviour
{
    public Transform playerCamera;

    public float moveSpeed = 3f;
    public float mouseSensitivity = 0.1f;

    private CharacterController controller;
    private float verticalSpeed;
    private float cameraAngle;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            Debug.LogError("Assegna DesktopCamera al campo Player Camera.");
            enabled = false;
            return;
        }

        SetCursorLocked(true);
    }

    void Update()
    {
        Vector2 movement = Vector2.zero;
        Vector2 mouseMovement = Vector2.zero;

        bool escapePressed = false;
        bool clickPressed = false;

        // Legge i comandi usando il sistema di input attivo.
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard != null)
        {
            movement.x = (keyboard.dKey.isPressed ? 1f : 0f)
                       - (keyboard.aKey.isPressed ? 1f : 0f);

            movement.y = (keyboard.wKey.isPressed ? 1f : 0f)
                       - (keyboard.sKey.isPressed ? 1f : 0f);

            escapePressed = keyboard.escapeKey.wasPressedThisFrame;
        }

        if (mouse != null)
        {
            mouseMovement = mouse.delta.ReadValue();
            clickPressed = mouse.leftButton.wasPressedThisFrame;
        }

#elif ENABLE_LEGACY_INPUT_MANAGER
        movement.x = (Input.GetKey(KeyCode.D) ? 1f : 0f)
                   - (Input.GetKey(KeyCode.A) ? 1f : 0f);

        movement.y = (Input.GetKey(KeyCode.W) ? 1f : 0f)
                   - (Input.GetKey(KeyCode.S) ? 1f : 0f);

        mouseMovement = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        ) * 15f;

        escapePressed = Input.GetKeyDown(KeyCode.Escape);
        clickPressed = Input.GetMouseButtonDown(0);
#endif

        // Esc libera il cursore.
        if (escapePressed)
        {
            SetCursorLocked(false);
            return;
        }

        // Un clic riprende il controllo della visuale.
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (clickPressed)
                SetCursorLocked(true);

            return;
        }

        // Ruota il giocatore a destra e a sinistra.
        transform.Rotate(
            0f,
            mouseMovement.x * mouseSensitivity,
            0f
        );

        // Ruota soltanto la telecamera in alto e in basso.
        cameraAngle -= mouseMovement.y * mouseSensitivity;
        cameraAngle = Mathf.Clamp(cameraAngle, -80f, 80f);

        playerCamera.localRotation =
            Quaternion.Euler(cameraAngle, 0f, 0f);

        // Limita la velocità quando si cammina in diagonale.
        movement = Vector2.ClampMagnitude(movement, 1f);

        Vector3 direction =
            transform.right * movement.x +
            transform.forward * movement.y;

        // Applica la gravità.
        if (controller.isGrounded && verticalSpeed < 0f)
            verticalSpeed = -2f;

        verticalSpeed -= 9.81f * Time.deltaTime;

        Vector3 velocity =
            direction * moveSpeed +
            Vector3.up * verticalSpeed;

        controller.Move(velocity * Time.deltaTime);
    }

    void SetCursorLocked(bool locked)
    {
        Cursor.lockState =
            locked ? CursorLockMode.Locked : CursorLockMode.None;

        Cursor.visible = !locked;
    }

    void OnDisable()
    {
        SetCursorLocked(false);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SetCursorLocked(false);
    }
}