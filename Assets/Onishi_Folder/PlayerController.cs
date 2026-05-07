using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// プレイヤーの移動・ジャンプ・ダッシュを管理するクラス。
/// CharacterControllerを使用し、物理演算に依存しない安定した挙動を実現する。
/// 入力は新Input Systemを使用し、カメラ基準の移動を行う。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController3D : MonoBehaviour
{
    // ===== 参照 =====
    [Header("参照")]
    [Tooltip("移動方向の基準となるカメラ")]
    public Transform cameraTransform;

    // ===== 移動設定 =====
    [Header("移動設定")]
    [Tooltip("通常移動速度")]
    public float walkSpeed = 3f;

    [Tooltip("ダッシュ時の最大速度")]
    public float dashSpeed = 7f;

    [Tooltip("加速・減速の滑らかさ")]
    public float acceleration = 10f;

    [Tooltip("回転速度")]
    public float rotationSpeed = 10f;

    // ===== ジャンプ設定 =====
    [Header("ジャンプ設定")]
    [Tooltip("ジャンプ初速")]
    public float jumpForce = 8f;

    [Tooltip("重力値（負の値）")]
    public float gravity = -20f;

    [Header("ヒップドロップ")]
    public float groundPoundSpeed = -35f;

    [Tooltip("停止時間")]
    public float groundPoundPauseTime = 0.15f;

    [Tooltip("着地硬直")]
    public float landingLag = 0.2f;

    private bool groundPounding;
    private bool groundPoundStart;
    private bool landing;

    private float pauseTimer;
    private float landingTimer;

    private bool groundPoundPressed;

    // ===== 空中制御 =====
    [Header("空中制御")]
    [Tooltip("空中での操作影響度（0〜1）")]
    public float airControl = 0.5f;

    // ===== コヨーテタイム =====
    [Header("ジャンプ補助")]
    [Tooltip("地面を離れてからジャンプ可能な猶予時間")]
    public float coyoteTime = 0.15f;

    private float coyoteTimer;

    // ===== 内部状態 =====
    private CharacterController controller;
    private Vector3 velocity; // 現在の速度（Yは重力含む）

    // ===== 入力関連 =====
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    public bool jumpPressed;
    private bool dashPressed;

    /// <summary>
    /// 初期化処理
    /// コンポーネント取得とInput Systemのバインドを行う
    /// </summary>
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();

        // --- 移動入力 ---
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // --- ジャンプ入力 ---
        inputActions.Player.Jump.performed += ctx => jumpPressed = true;

        // --- ダッシュ入力 ---
        inputActions.Player.Dash.performed += ctx => dashPressed = true;
        inputActions.Player.Dash.canceled += ctx => dashPressed = false;
    }

    /// <summary>
    /// Input System有効化
    /// </summary>
    void OnEnable() => inputActions.Enable();

    /// <summary>
    /// Input System無効化
    /// </summary>
    void OnDisable() => inputActions.Disable();

    /// <summary>
    /// 毎フレーム更新
    /// </summary>
    void Update()
    {
        GroundPound();

        if (!landing)
        {
            Jump();
            MoveAndGravity();
        }
    }

    void MoveAndGravity()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;

        float targetSpeed = dashPressed ? dashSpeed : walkSpeed;
        Vector3 targetVelocity = moveDir * targetSpeed;

        float control = controller.isGrounded ? 1f : airControl;

        velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, acceleration * control * Time.deltaTime);
        velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, acceleration * control * Time.deltaTime);

        // ヒップドロップ中は移動を無効化
        if (groundPounding || groundPoundStart)
        {
            controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
            return;
        }

        // 回転
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // 重力
        velocity.y += gravity * Time.deltaTime;

        // 全部まとめてMove
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// ジャンプ処理
    /// コヨーテタイムを利用し、操作性を向上させる
    /// </summary>
    void Jump()
    {
        if (controller.isGrounded)
        {
            // 地上にいる間は猶予時間をリセット
            coyoteTimer = coyoteTime;

            // 接地時にY速度をリセット（地面に吸い付くようにする）
            if (velocity.y < 0)
                velocity.y = -2f;
        }
        else
        {
            // 空中では猶予時間を減少
            coyoteTimer -= Time.deltaTime;
        }

        // ジャンプ入力かつ猶予時間内ならジャンプ
        if (jumpPressed && coyoteTimer > 0)
        {
            velocity.y = jumpForce;
            jumpPressed = false;
        }

        // 入力フラグをリセット（多重ジャンプ防止）
        //jumpPressed = false;
    }

    void GroundPound()
    {
        // 着地硬直
        if (landing)
        {
            landingTimer -= Time.deltaTime;

            if (landingTimer <= 0)
            {
                landing = false;
            }

            return;
        }

        // 発動開始
        if (groundPoundPressed && !controller.isGrounded && !groundPounding)
        {
            groundPounding = true;
            groundPoundStart = true;

            pauseTimer = groundPoundPauseTime;

            velocity = Vector3.zero;

            groundPoundPressed = false;
        }

        // 一瞬停止
        if (groundPoundStart)
        {
            pauseTimer -= Time.deltaTime;

            if (pauseTimer <= 0)
            {
                groundPoundStart = false;

                velocity.y = groundPoundSpeed;
            }

            controller.Move(Vector3.zero);
            return;
        }

        // 落下中
        if (groundPounding)
        {
            velocity.x = 0;
            velocity.z = 0;

            // 接地したら終了
            if (controller.isGrounded)
            {
                groundPounding = false;

                landing = true;
                landingTimer = landingLag;

                velocity = Vector3.zero;

                Debug.Log("ヒップドロップ着地！");
            }
        }
    }
}