using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("ターゲット")]
    public Transform target;

    [Header("距離")]
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 6f;

    [Header("回転")]
    public float mouseSensitivity = 2f;
    public float minY = -30f;
    public float maxY = 60f;

    [Header("追従")]
    public float smoothSpeed = 10f;

    [Header("衝突")]
    public LayerMask obstacleMask;

    private float yaw;
    private float pitch;

    private bool cannonView = false;

    private Transform customCameraPoint;

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
        // プレイヤーの後ろ方向
        Vector3 desiredPosition;

        if (customCameraPoint != null)
        {
            // 指定位置を使う
            desiredPosition =
                customCameraPoint.position;
        }
        else
        {
            // 通常カメラ
            Vector3 backward = -target.forward;

            // プレイヤーの少し上から見る
            Vector3 offset =
                backward * distance +
                target.up * 2f;

            desiredPosition =
                target.position + offset;
        }

        // 障害物判定
        RaycastHit hit;
        if (Physics.Linecast(
            target.position,
            desiredPosition,
            out hit,
            obstacleMask))
        {
            desiredPosition = hit.point;
        }

        // なめらかに追従
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        if (cannonView)
        {
            // 大砲から見る
            transform.rotation = target.rotation;
        }
        else
        {
            // プレイヤーを見る
            transform.LookAt(
                target.position + target.up * 1.5f
            );
        }
    }

    public void SetTarget(
    Transform newTarget,
    Transform cameraPoint = null,
    bool isCannonView = false)
    {
        target = newTarget;
        customCameraPoint = cameraPoint;
        cannonView = isCannonView;
    }

    public void SetFrontView(bool value)
    {
        bool frontView = value;
    }
}