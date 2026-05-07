using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("ƒ^[ƒQƒbƒg")]
    public Transform target;

    [Header("‹——£")]
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 6f;

    [Header("‰ñ“]")]
    public float mouseSensitivity = 2f;
    public float minY = -30f;
    public float maxY = 60f;

    [Header("’Ç]")]
    public float smoothSpeed = 10f;

    [Header("Õ“Ë")]
    public LayerMask obstacleMask;

    private float yaw;
    private float pitch;

    private Vector2 lookInput;

    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        RotateCamera();
        FollowTarget();
    }

    void RotateCamera()
    {
        yaw += lookInput.x * mouseSensitivity;
        pitch -= lookInput.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minY, maxY);
    }

    void FollowTarget()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 targetPosition = target.position;
        Vector3 desiredPosition = targetPosition - rotation * Vector3.forward * distance;

        // •Ç”»’è
        RaycastHit hit;
        if (Physics.Linecast(targetPosition, desiredPosition, out hit, obstacleMask))
        {
            desiredPosition = hit.point;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(targetPosition);
    }
}