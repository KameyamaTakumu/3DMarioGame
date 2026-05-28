//using UnityEngine;
//using UnityEngine.InputSystem;

//public class CameraController : MonoBehaviour
//{
//    [Header("ターゲット")]
//    public Transform target;

//    [Header("距離")]
//    public float distance = 5f;
//    public float minDistance = 2f;
//    public float maxDistance = 6f;
//    [CustomLabel("カメラの高さ"), SerializeField]
//    private float cameraHeight = 2f;
//    [CustomLabel("カメラの角度"), SerializeField]
//    private float cameraRotation = 2f;

//    [Header("回転")]
//    public float mouseSensitivity = 2f;
//    public float minY = -30f;
//    public float maxY = 60f;

//    [Header("追従")]
//    public float smoothSpeed = 10f;

//    [Header("衝突")]
//    public LayerMask obstacleMask;

//    private bool cannonView = false;

//    private Transform customCameraPoint;

//    private PlayerInputActions inputActions;

//    void Awake()
//    {
//        inputActions = new PlayerInputActions();
//    }

//    void OnEnable() => inputActions.Enable();
//    void OnDisable() => inputActions.Disable();

//    void LateUpdate()
//    {
//        FollowTarget();
//    }

//    void FollowTarget()
//    {
//        // target が消えていたら処理しない
//        if (target == null)
//        {
//            return;
//        }

//        // プレイヤーの後ろ方向
//        Vector3 desiredPosition;

//        if (customCameraPoint != null)
//        {
//            // 指定位置を使う
//            desiredPosition =
//                customCameraPoint.position;
//        }
//        else
//        {
//            // 通常カメラ
//            Vector3 backward = -target.forward;

//            // プレイヤーの少し上から見る
//            Vector3 offset =
//                backward * distance +
//                target.up * cameraHeight;

//            desiredPosition =
//                target.position + offset;
//        }

//        // 障害物判定
//        RaycastHit hit;
//        if (Physics.Linecast(
//            target.position,
//            desiredPosition,
//            out hit,
//            obstacleMask))
//        {
//            desiredPosition = hit.point;
//        }

//        // なめらかに追従
//        transform.position = Vector3.Lerp(
//            transform.position,
//            desiredPosition,
//            smoothSpeed * Time.deltaTime
//        );

//        if (cannonView)
//        {
//            // 大砲から見る
//            transform.rotation = target.rotation;
//        }
//        else
//        {
//            // プレイヤーを見る
//            transform.LookAt(
//                target.position + target.up * cameraRotation
//            );
//        }
//    }

//    public void SetTarget(
//    Transform newTarget,
//    Transform cameraPoint = null,
//    bool isCannonView = false)
//    {
//        target = newTarget;
//        customCameraPoint = cameraPoint;
//        cannonView = isCannonView;
//    }

//    public void SetFrontView(bool value)
//    {
//        bool frontView = value;
//    }
//}
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

    [CustomLabel("カメラの高さ"), SerializeField]
    private float cameraHeight = 2f;

    [CustomLabel("カメラの注視点オフセット"), SerializeField]
    private float lookAtHeightOffset = 1f;

    [Header("追従")]
    [Tooltip("位置補間の滑らかさ（大きいほど速く追従）")]
    public float positionSmoothSpeed = 8f;

    [Tooltip("回転補間の滑らかさ（大きいほど速く追従）")]
    public float rotationSmoothSpeed = 8f;

    [Header("衝突")]
    public LayerMask obstacleMask;

    // ─────────────────────────────────────
    // 内部状態
    // ─────────────────────────────────────

    private bool cannonView = false;
    private Transform customCameraPoint;
    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void LateUpdate()
    {
        FollowTarget();
    }

    void FollowTarget()
    {
        if (target == null) return;

        // ─────────────────────────────────
        // 惑星上方向を基準にする
        // GravityBody により target.up が
        // 惑星法線方向を向いている前提
        // ─────────────────────────────────
        Vector3 planetUp = target.up;

        // ─────────────────────────────────
        // 目標位置を計算
        // ─────────────────────────────────
        Vector3 desiredPosition;

        if (customCameraPoint != null)
        {
            desiredPosition = customCameraPoint.position;
        }
        else
        {
            // PlayerController と同様に
            // 「後方 + 上方向オフセット」で目標位置を求める
            Vector3 backward = -target.forward;

            Vector3 offset =
                backward * distance
                + planetUp * cameraHeight;

            desiredPosition = target.position + offset;
        }

        // ─────────────────────────────────
        // 障害物による距離クリップ
        // ─────────────────────────────────
        if (Physics.Linecast(
            target.position,
            desiredPosition,
            out RaycastHit hit,
            obstacleMask))
        {
            desiredPosition = hit.point;
        }

        // ─────────────────────────────────
        // 位置補間（PlayerController と同方式）
        // Lerp で滑らかに追従
        // ─────────────────────────────────
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            Time.deltaTime * positionSmoothSpeed
        );

        // ─────────────────────────────────
        // 回転補間
        // LookAt() による即時回転をやめ、
        // Quaternion.Slerp で滑らか補間する
        // planetUp を Up 方向に渡すことで
        // 球体歩行時のガクつきを防ぐ
        // ─────────────────────────────────
        if (cannonView)
        {
            // 大砲視点：ターゲットの回転に合わせる
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                target.rotation,
                Time.deltaTime * rotationSmoothSpeed
            );
        }
        else
        {
            // 通常視点：注視点 → 回転目標を Slerp
            Vector3 lookAtPoint =
                target.position
                + planetUp * lookAtHeightOffset;

            Vector3 dirToTarget =
                lookAtPoint - transform.position;

            // ゼロベクトルは LookRotation に渡せないのでガード
            if (dirToTarget.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(
                        dirToTarget,
                        planetUp      // ← Up に惑星法線を使う
                    );

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * rotationSmoothSpeed
                );
            }
        }
    }

    /// <summary>
    /// 追従ターゲットと視点モードを切り替える
    /// </summary>
    public void SetTarget(
        Transform newTarget,
        Transform cameraPoint = null,
        bool isCannonView = false)
    {
        target = newTarget;
        customCameraPoint = cameraPoint;
        cannonView = isCannonView;
    }

    /// <summary>
    /// 前方視点切り替え（将来実装用）
    /// </summary>
    public void SetFrontView(bool value)
    {
        // TODO: 前方視点への切り替え処理
        _ = value;
    }
}