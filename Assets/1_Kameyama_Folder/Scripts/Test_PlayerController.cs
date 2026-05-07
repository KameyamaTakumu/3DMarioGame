using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Rigidbody を使用した 3Dプレイヤー制御クラス。
/// カメラ基準移動・ジャンプ・ダッシュ・ヒップドロップに対応。
/// 惑星重力などの独自重力にも対応しやすい構成。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Test_PlayerController : MonoBehaviour
{
    //=====================================================
    // 移動設定
    //=====================================================

    // プレイヤーの移動速度
    [CustomLabel("移動速度"), SerializeField]
    private float moveSpeed = 15f;

    // ダッシュ時の移動速度
    [CustomLabel("ダッシュ速度"), SerializeField]
    private float dashSpeed = 25f;

    // 空中での操作影響度
    [Range(0f, 1f)]
    public float airControl = 0.5f;

    //=====================================================
    // 回転設定
    //=====================================================

    // プレイヤーの回転速度
    public float rotateSpeed = 5f;

    // プレイヤーの回転方向
    //  1  = 時計回り
    // -1 = 反時計回り
    private int rotateDirection = 0;

    //=====================================================
    // ジャンプ設定
    //=====================================================

    // ジャンプ力
    public float jumpPower = 10f;

    // コヨーテタイム
    public float coyoteTime = 0.15f;

    // 現在のコヨーテタイマー
    private float coyoteTimer;

    //=====================================================
    // ヒップドロップ設定
    //=====================================================

    [Header("ヒップドロップ")]

    // ヒップドロップ落下速度
    public float groundPoundSpeed = 35f;

    // 発動前停止時間
    public float groundPoundPauseTime = 0.15f;

    // 着地硬直
    public float landingLag = 0.2f;

    //=====================================================
    // 接地判定
    //=====================================================

    // 地面に接地しているか
    public bool grounded;

    //=====================================================
    // 内部状態
    //=====================================================

    // プレイヤーの Rigidbody
    private Rigidbody rb = null;

    // 入力
    private PlayerInputActions inputActions;

    private Vector2 moveInput;

    private bool jumpPressed;
    private bool dashPressed;
    private bool groundPoundPressed;

    // ヒップドロップ状態
    private bool groundPounding;
    private bool groundPoundStart;
    private bool landing;

    // タイマー
    private float pauseTimer;
    private float landingTimer;

    //=====================================================
    // 初期化
    //=====================================================

    void Awake()
    {
        // Rigidbody を取得
        rb = GetComponent<Rigidbody>();

        // Rigidbody の回転を固定
        rb.freezeRotation = true;

        // InputActions 作成
        inputActions = new PlayerInputActions();

        //==============================
        // 移動入力
        //==============================

        inputActions.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
        };

        inputActions.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
        };

        //==============================
        // ジャンプ入力
        //==============================

        inputActions.Player.Jump.performed += ctx =>
        {
            jumpPressed = true;
        };

        //==============================
        // ダッシュ入力
        //==============================

        inputActions.Player.Dash.performed += ctx =>
        {
            dashPressed = true;
        };

        inputActions.Player.Dash.canceled += ctx =>
        {
            dashPressed = false;
        };

        //==============================
        // ヒップドロップ入力
        //==============================

        inputActions.Player.GroundPound.performed += ctx =>
        {
            groundPoundPressed = true;
        };
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    //=====================================================
    // 毎フレーム更新
    //=====================================================

    void Update()
    {
        // コヨーテタイム更新
        if (grounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // ジャンプ
        Jump();

        // ヒップドロップ
        GroundPound();
    }

    //=====================================================
    // 物理更新
    //=====================================================

    private void FixedUpdate()
    {
        // 着地硬直中は動けない
        if (landing)
        {
            return;
        }

        // プレイヤー回転
        HorizontalRotate();

        // プレイヤー移動
        Move();
    }

    //=====================================================
    // 移動処理
    //=====================================================

    void Move()
    {
        // ヒップドロップ中は移動禁止
        if (groundPounding || groundPoundStart)
        {
            rb.linearVelocity = Vector3.Project(
                rb.linearVelocity,
                transform.up
            );

            return;
        }

        // 入力方向
        Vector3 moveDirection =
            new Vector3(moveInput.x, 0, moveInput.y).normalized;

        // プレイヤー基準方向へ変換
        Vector3 worldMove =
            transform.TransformDirection(moveDirection);

        // 移動速度
        float currentSpeed =
            dashPressed ? dashSpeed : moveSpeed;

        // 空中制御
        float control =
            grounded ? 1f : airControl;

        // 現在の速度を
        // 地面方向と水平移動に分解
        Vector3 verticalVelocity =
            Vector3.Project(rb.linearVelocity, transform.up);

        Vector3 horizontalVelocity =
            rb.linearVelocity - verticalVelocity;

        // 目標水平速度
        Vector3 targetVelocity =
            worldMove * currentSpeed;

        // 滑らかに加速
        horizontalVelocity = Vector3.Lerp(
            horizontalVelocity,
            targetVelocity,
            control * Time.fixedDeltaTime * 10f
        );

        // 合成
        rb.linearVelocity =
            horizontalVelocity + verticalVelocity;
    }

    //=====================================================
    // ジャンプ処理
    //=====================================================

    void Jump()
    {
        // 接地中かつジャンプ入力
        if (jumpPressed && coyoteTimer > 0)
        {
            grounded = false;

            // 現在のY速度をリセット
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0;
            rb.linearVelocity = velocity;

            // transform.up 方向へジャンプ
            rb.AddForce(
                transform.up * jumpPower,
                ForceMode.Impulse
            );

            jumpPressed = false;
            coyoteTimer = 0;
        }

        jumpPressed = false;
    }

    //=====================================================
    // ヒップドロップ
    //=====================================================

    void GroundPound()
    {
        //==============================
        // 着地硬直
        //==============================

        if (landing)
        {
            landingTimer -= Time.deltaTime;

            if (landingTimer <= 0)
            {
                landing = false;
            }

            return;
        }

        //==============================
        // 発動開始
        //==============================

        if (groundPoundPressed &&
            !grounded &&
            !groundPounding)
        {
            groundPounding = true;
            groundPoundStart = true;

            pauseTimer = groundPoundPauseTime;

            rb.linearVelocity = Vector3.zero;

            groundPoundPressed = false;
        }

        //==============================
        // 一瞬停止
        //==============================

        if (groundPoundStart)
        {
            pauseTimer -= Time.deltaTime;

            rb.linearVelocity = Vector3.zero;

            if (pauseTimer <= 0)
            {
                groundPoundStart = false;

                rb.linearVelocity =
                    -transform.up * groundPoundSpeed;
            }

            return;
        }

        //==============================
        // 落下中
        //==============================

        if (groundPounding)
        {
            rb.linearVelocity = new Vector3(
                0,
                rb.linearVelocity.y,
                0
            );

            // 接地したら終了
            if (grounded)
            {
                groundPounding = false;

                landing = true;
                landingTimer = landingLag;

                rb.linearVelocity = Vector3.zero;

                Debug.Log("ヒップドロップ着地！");
            }
        }

        groundPoundPressed = false;
    }

    //=====================================================
    // プレイヤー回転
    //=====================================================

    void HorizontalRotate()
    {
        // Qキー
        if (Keyboard.current.qKey.isPressed)
        {
            rotateDirection = -1;
        }
        // Eキー
        else if (Keyboard.current.eKey.isPressed)
        {
            rotateDirection = 1;
        }
        else
        {
            rotateDirection = 0;
        }

        // transform.up を軸として回転
        Quaternion rt =
            Quaternion.AngleAxis(
                rotateDirection * rotateSpeed,
                transform.up
            );

        Quaternion q = transform.rotation;

        transform.rotation = rt * q;
    }

    //=====================================================
    // 接地判定
    //=====================================================

    void OnCollisionEnter(Collision other)
    {
        // Planet または Stage に接触
        if (other.gameObject.CompareTag("Planet") ||
            other.gameObject.CompareTag("Stage"))
        {
            grounded = true;
        }
    }

    void OnCollisionStay(Collision other)
    {
        // 接地維持
        if (other.gameObject.CompareTag("Planet") ||
            other.gameObject.CompareTag("Stage"))
        {
            grounded = true;
        }
    }

    void OnCollisionExit(Collision other)
    {
        // 地面から離れた
        if (other.gameObject.CompareTag("Planet") ||
            other.gameObject.CompareTag("Stage"))
        {
            grounded = false;
        }
    }
}
