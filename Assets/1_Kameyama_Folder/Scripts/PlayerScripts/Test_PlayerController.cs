//using System;
//using System.Text.RegularExpressions;
//using UnityEngine;
//using UnityEngine.InputSystem;

///// <summary>
///// Rigidbody を使用した 3Dプレイヤー制御クラス。
///// カメラ基準移動・ジャンプ・ダッシュ・ヒップドロップに対応。
///// 惑星重力などの独自重力にも対応しやすい構成。
///// </summary>
//[RequireComponent(typeof(Rigidbody))]
//public class Test_PlayerController : MonoBehaviour
//{
//    // インスタンス
//    public static Test_PlayerController instance;

//    //=====================================================
//    // 移動設定
//    //=====================================================

//    // プレイヤーの移動速度
//    [CustomLabel("移動速度"), SerializeField]
//    private float moveSpeed = 15f;

//    // ダッシュ時の移動速度
//    [CustomLabel("ダッシュ速度"), SerializeField]
//    private float dashSpeed = 25f;

//    // 空中での操作影響度
//    [Range(0f, 1f)]
//    public float airControl = 0.5f;

//    //=====================================================
//    // 回転設定
//    //=====================================================

//    // プレイヤーの回転速度
//    public float rotateSpeed = 5f;

//    // プレイヤーの回転方向
//    //  1  = 時計回り
//    // -1 = 反時計回り
//    private int rotateDirection = 0;

//    //=====================================================
//    // ジャンプ設定
//    //=====================================================

//    // ジャンプ力
//    public float jumpPower = 10f;

//    // コヨーテタイム
//    public float coyoteTime = 0.15f;

//    // 現在のコヨーテタイマー
//    private float coyoteTimer;

//    //=====================================================
//    // ヒップドロップ設定
//    //=====================================================

//    [Header("ヒップドロップ")]

//    // ヒップドロップ落下速度
//    public float groundPoundSpeed = 35f;

//    // 発動前停止時間
//    public float groundPoundPauseTime = 0.15f;

//    // 着地硬直
//    public float landingLag = 0.2f;

//    //=====================================================
//    // 接地判定
//    //=====================================================

//    // 地面に接地しているか
//    public bool grounded;

//    //=====================================================
//    // 内部状態
//    //=====================================================

//    // プレイヤーの Rigidbody
//    private Rigidbody rb = null;

//    // 入力
//    private PlayerInputActions inputActions;

//    private Vector2 moveInput;

//    private bool jumpPressed;
//    private bool dashPressed;
//    private bool groundPoundPressed;

//    // ヒップドロップ状態
//    private bool groundPounding;
//    private bool groundPoundStart;
//    private bool landing;

//    // タイマー
//    private float pauseTimer;
//    private float landingTimer;

//    // キャプチャ状態
//    // true; キャプチャ状態 false: キャプチャ解除
//    public bool captureTrigger = false;

//    //=====================================================
//    // 初期化
//    //=====================================================

//    void Awake()
//    {
//        // インスタンス設定
//        if (instance == null)
//        {
//            instance = this;
//        }

//        // Rigidbody を取得
//        rb = GetComponent<Rigidbody>();

//        // Rigidbody の回転を固定
//        rb.freezeRotation = true;

//        // InputActions 作成
//        inputActions = new PlayerInputActions();

//        //==============================
//        // 移動入力
//        //==============================

//        inputActions.Player.Move.performed += ctx =>
//        {
//            moveInput = ctx.ReadValue<Vector2>();
//        };

//        inputActions.Player.Move.canceled += ctx =>
//        {
//            moveInput = Vector2.zero;
//        };

//        //==============================
//        // ジャンプ入力
//        //==============================

//        inputActions.Player.Jump.performed += ctx =>
//        {
//            jumpPressed = true;
//        };

//        //==============================
//        // ダッシュ入力
//        //==============================

//        inputActions.Player.Dash.performed += ctx =>
//        {
//            dashPressed = true;
//        };

//        inputActions.Player.Dash.canceled += ctx =>
//        {
//            dashPressed = false;
//        };

//        //==============================
//        // ヒップドロップ入力
//        //==============================

//        inputActions.Player.GroundPound.performed += ctx =>
//        {
//            groundPoundPressed = true;
//        };
//    }

//    void OnEnable()
//    {
//        inputActions.Enable();
//    }

//    void OnDisable()
//    {
//        inputActions.Disable();
//    }

//    //=====================================================
//    // 毎フレーム更新
//    //=====================================================

//    void Update()
//    {
//        // コヨーテタイム更新
//        if (grounded)
//        {
//            coyoteTimer = coyoteTime;
//        }
//        else
//        {
//            coyoteTimer -= Time.deltaTime;
//        }

//        // ジャンプ
//        Jump();

//        // ヒップドロップ
//        GroundPound();

//        // キャプチャチェック
//        CaptureCheck();

//        // キャプチャ状態時
//        if(captureTrigger)
//        {
//            if (Keyboard.current.tKey.wasPressedThisFrame)
//            {
//                // キャプチャ状態を解除
//                captureTrigger = false;

//                // 操作をプレイヤーに戻す
//                PlaySwitch();
//            }
//        }
//    }

//    //=====================================================
//    // 物理更新
//    //=====================================================

//    private void FixedUpdate()
//    {
//        // 着地硬直中は動けない
//        if (landing)
//        {
//            return;
//        }

//        // プレイヤー回転
//        HorizontalRotate();

//        // プレイヤー移動
//        Move();
//    }

//    //=====================================================
//    // 移動処理
//    //=====================================================

//    void Move()
//    {
//        // ヒップドロップ中は移動禁止
//        if (groundPounding || groundPoundStart)
//        {
//            rb.linearVelocity = Vector3.Project(
//                rb.linearVelocity,
//                transform.up
//            );

//            return;
//        }

//        // 入力方向
//        Vector3 moveDirection =
//            new Vector3(moveInput.x, 0, moveInput.y).normalized;

//        // プレイヤー基準方向へ変換
//        Vector3 worldMove =
//            transform.TransformDirection(moveDirection);

//        // プレイヤー移動方向へ回転
//        if (moveDirection != Vector3.zero)
//        {
//            Quaternion targetRotation =
//                Quaternion.LookRotation(worldMove, transform.up);

//            transform.rotation = Quaternion.Slerp(
//                transform.rotation,
//                targetRotation,
//                rotateSpeed * Time.fixedDeltaTime
//            );
//        }

//        // 移動速度
//        float currentSpeed =
//            dashPressed ? dashSpeed : moveSpeed;

//        // 空中制御
//        float control =
//            grounded ? 1f : airControl;

//        // 現在の速度を
//        // 地面方向と水平移動に分解
//        Vector3 verticalVelocity =
//            Vector3.Project(rb.linearVelocity, transform.up);

//        Vector3 horizontalVelocity =
//            rb.linearVelocity - verticalVelocity;

//        // 目標水平速度
//        Vector3 targetVelocity =
//            worldMove * currentSpeed;

//        // 滑らかに加速
//        horizontalVelocity = Vector3.Lerp(
//            horizontalVelocity,
//            targetVelocity,
//            control * Time.fixedDeltaTime * 10f
//        );

//        // 合成
//        rb.linearVelocity =
//            horizontalVelocity + verticalVelocity;
//    }

//    //=====================================================
//    // ジャンプ処理
//    //=====================================================

//    void Jump()
//    {
//        // 接地中かつジャンプ入力
//        if (jumpPressed && coyoteTimer > 0)
//        {
//            grounded = false;

//            // 現在のY速度をリセット
//            Vector3 velocity = rb.linearVelocity;
//            velocity.y = 0;
//            rb.linearVelocity = velocity;

//            // transform.up 方向へジャンプ
//            rb.AddForce(
//                transform.up * jumpPower,
//                ForceMode.Impulse
//            );

//            jumpPressed = false;
//            coyoteTimer = 0;
//        }

//        jumpPressed = false;
//    }

//    //=====================================================
//    // ヒップドロップ
//    //=====================================================

//    void GroundPound()
//    {
//        //==============================
//        // 着地硬直
//        //==============================

//        if (landing)
//        {
//            landingTimer -= Time.deltaTime;

//            if (landingTimer <= 0)
//            {
//                landing = false;
//            }

//            return;
//        }

//        //==============================
//        // 発動開始
//        //==============================

//        if (groundPoundPressed &&
//            !grounded &&
//            !groundPounding)
//        {
//            groundPounding = true;
//            groundPoundStart = true;

//            pauseTimer = groundPoundPauseTime;

//            rb.linearVelocity = Vector3.zero;

//            groundPoundPressed = false;
//        }

//        //==============================
//        // 一瞬停止
//        //==============================

//        if (groundPoundStart)
//        {
//            pauseTimer -= Time.deltaTime;

//            rb.linearVelocity = Vector3.zero;

//            if (pauseTimer <= 0)
//            {
//                groundPoundStart = false;

//                rb.linearVelocity =
//                    -transform.up * groundPoundSpeed;
//            }

//            return;
//        }

//        //==============================
//        // 落下中
//        //==============================

//        if (groundPounding)
//        {
//            rb.linearVelocity = new Vector3(
//                0,
//                rb.linearVelocity.y,
//                0
//            );

//            // 接地したら終了
//            if (grounded)
//            {
//                groundPounding = false;

//                landing = true;
//                landingTimer = landingLag;

//                rb.linearVelocity = Vector3.zero;

//                Debug.Log("ヒップドロップ着地！");
//            }
//        }

//        groundPoundPressed = false;
//    }

//    //=====================================================
//    // プレイヤー回転
//    //=====================================================

//    void HorizontalRotate()
//    {
//        // Qキー
//        if (Keyboard.current.qKey.isPressed)
//        {
//            rotateDirection = -1;
//        }
//        // Eキー
//        else if (Keyboard.current.eKey.isPressed)
//        {
//            rotateDirection = 1;
//        }
//        else
//        {
//            rotateDirection = 0;
//        }

//        // transform.up を軸として回転
//        Quaternion rt =
//            Quaternion.AngleAxis(
//                rotateDirection * rotateSpeed,
//                transform.up
//            );

//        Quaternion q = transform.rotation;

//        transform.rotation = rt * q;
//    }

//    //=====================================================
//    // 接地判定
//    //=====================================================

//    void OnCollisionEnter(Collision other)
//    {
//        // Planet または Stage に接触
//        if (other.gameObject.CompareTag("Planet") ||
//            other.gameObject.CompareTag("Stage"))
//        {
//            grounded = true;
//        }
//    }

//    void OnCollisionStay(Collision other)
//    {
//        // 接地維持
//        if (other.gameObject.CompareTag("Planet") ||
//            other.gameObject.CompareTag("Stage"))
//        {
//            grounded = true;
//        }
//    }

//    void OnCollisionExit(Collision other)
//    {
//        // 地面から離れた
//        if (other.gameObject.CompareTag("Planet") ||
//            other.gameObject.CompareTag("Stage"))
//        {
//            grounded = false;
//        }
//    }

//    //=====================================================
//    // キャプチャ処理
//    //=====================================================

//    // キャプチャトリガーをチェックする関数
//    private void CaptureCheck()
//    {
//        if (!captureTrigger){ return; }
//        else 
//        {
//            // プレイヤーを操作できないようにする
//            PlaySwitch();
//        }
//    }

//    // プレイヤーを操作できないようにする関数
//    private void PlaySwitch()
//    {        
//        if (captureTrigger)
//        {
//            // inputActionを止める
//            OnDisable();

//            Debug.Log("プレイヤー操作不能");
//        }
//        else if (!captureTrigger) 
//        {
//            // inputActionを動かす
//            OnEnable();

//            Debug.Log("プレイヤー操作可能");
//        }
//    }
//}
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Rigidbody ???g?p???? 3D?v???C???[????N???X?B
/// ?J?????????E?W?????v?E?_?b?V???E?q?b?v?h???b?v?????B
/// ?f???d????????d??????????????\???B
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Test_PlayerController : MonoBehaviour
{
    //=====================================================
    // ??????
    //=====================================================

    // ?v???C???[???????x
    [CustomLabel("??????x"), SerializeField]
    private float moveSpeed = 15f;

    // ?_?b?V???????????x
    [CustomLabel("?_?b?V?????x"), SerializeField]
    private float dashSpeed = 25f;

    // ????????e???x
    [Range(0f, 1f)]
    public float airControl = 0.5f;

    //=====================================================
    // ??]???
    //=====================================================

    // ?v???C???[???]???x
    public float rotateSpeed = 5f;

    // ?v???C???[???]????
    //  1  = ???v???
    // -1 = ?????v???
    private int rotateDirection = 0;

    //=====================================================
    // ?W?????v???
    //=====================================================

    // ?W?????v??
    public float jumpPower = 10f;

    // ?R???[?e?^?C??
    public float coyoteTime = 0.15f;

    // ?????R???[?e?^?C?}?[
    private float coyoteTimer;

    //=====================================================
    // ?q?b?v?h???b?v???
    //=====================================================

    [Header("?q?b?v?h???b?v")]

    // ?q?b?v?h???b?v???????x
    public float groundPoundSpeed = 35f;

    // ?????O??~????
    public float groundPoundPauseTime = 0.15f;

    // ???n?d??
    public float landingLag = 0.2f;

    //=====================================================
    // ??n????
    //=====================================================

    // ?n????n???????
    public bool grounded;

    //=====================================================
    // ???????
    //=====================================================

    // ?v???C???[?? Rigidbody
    private Rigidbody rb = null;

    // ????
    private PlayerInputActions inputActions;

    private Vector2 moveInput;

    private bool jumpPressed;
    private bool dashPressed;
    private bool groundPoundPressed;

    // ?q?b?v?h???b?v???
    private bool groundPounding;
    private bool groundPoundStart;
    private bool landing;

    // ?^?C?}?[
    private float pauseTimer;
    private float landingTimer;

    //=====================================================
    // ??????
    //=====================================================

    void Awake()
    {
        // Rigidbody ???擾
        rb = GetComponent<Rigidbody>();

        // Rigidbody ???]?????
        rb.freezeRotation = true;

        // InputActions ??
        inputActions = new PlayerInputActions();

        //==============================
        // ???????
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
        // ?W?????v????
        //==============================

        inputActions.Player.Jump.performed += ctx =>
        {
            jumpPressed = true;
        };

        //==============================
        // ?_?b?V??????
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
        // ?q?b?v?h???b?v????
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
    // ???t???[???X?V
    //=====================================================

    void Update()
    {
        // ?R???[?e?^?C???X?V
        if (grounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // ?W?????v
        Jump();

        // ?q?b?v?h???b?v
        GroundPound();
    }

    //=====================================================
    // ?????X?V
    //=====================================================

    private void FixedUpdate()
    {
        // ???n?d????????????
        if (landing)
        {
            return;
        }

        // ?v???C???[??]
        HorizontalRotate();

        // ?v???C???[???
        Move();
    }

    //=====================================================
    // ???????
    //=====================================================

    void Move()
    {
        // ?q?b?v?h???b?v????????~
        if (groundPounding || groundPoundStart)
        {
            rb.linearVelocity = Vector3.Project(
                rb.linearVelocity,
                transform.up
            );

            return;
        }

        // ???????
        Vector3 moveDirection =
            new Vector3(moveInput.x, 0, moveInput.y).normalized;

        // ?v???C???[?????????
        Vector3 worldMove =
            transform.TransformDirection(moveDirection);

        // ??????x
        float currentSpeed =
            dashPressed ? dashSpeed : moveSpeed;

        // ??????
        float control =
            grounded ? 1f : airControl;

        // ???????x??
        // ?n??????????????????
        Vector3 verticalVelocity =
            Vector3.Project(rb.linearVelocity, transform.up);

        Vector3 horizontalVelocity =
            rb.linearVelocity - verticalVelocity;

        // ??W???????x
        Vector3 targetVelocity =
            worldMove * currentSpeed;

        // ?????????
        horizontalVelocity = Vector3.Lerp(
            horizontalVelocity,
            targetVelocity,
            control * Time.fixedDeltaTime * 10f
        );

        // ????
        rb.linearVelocity =
            horizontalVelocity + verticalVelocity;
    }

    //=====================================================
    // ?W?????v????
    //=====================================================

    void Jump()
    {
        // ??n??????W?????v????
        if (jumpPressed && coyoteTimer > 0)
        {
            grounded = false;

            // ?????Y???x?????Z?b?g
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0;
            rb.linearVelocity = velocity;

            // transform.up ??????W?????v
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
    // ?q?b?v?h???b?v
    //=====================================================

    void GroundPound()
    {
        //==============================
        // ???n?d??
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
        // ?????J?n
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
        // ??u??~
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
        // ??????
        //==============================

        if (groundPounding)
        {
            rb.linearVelocity = new Vector3(
                0,
                rb.linearVelocity.y,
                0
            );

            // ??n??????I??
            if (grounded)
            {
                groundPounding = false;

                landing = true;
                landingTimer = landingLag;

                rb.linearVelocity = Vector3.zero;

                Debug.Log("?q?b?v?h???b?v???n?I");
            }
        }

        groundPoundPressed = false;
    }

    //=====================================================
    // ?v???C???[??]
    //=====================================================

    void HorizontalRotate()
    {
        // Q?L?[
        if (Keyboard.current.qKey.isPressed)
        {
            rotateDirection = -1;
        }
        // E?L?[
        else if (Keyboard.current.eKey.isPressed)
        {
            rotateDirection = 1;
        }
        else
        {
            rotateDirection = 0;
        }

        // transform.up ??????????]
        Quaternion rt =
            Quaternion.AngleAxis(
                rotateDirection * rotateSpeed,
                transform.up
            );

        Quaternion q = transform.rotation;

        transform.rotation = rt * q;
    }

    //=====================================================
    // ??n????
    //=====================================================

    void OnCollisionEnter(Collision other)
    {
        // Planet ????? Stage ???G
        if (other.gameObject.CompareTag("Planet") ||
            other.gameObject.CompareTag("Stage"))
        {
            grounded = true;
        }
    }

    void OnCollisionStay(Collision other)
    {
        // ??n???
        if (other.gameObject.CompareTag("Planet") ||
            other.gameObject.CompareTag("Stage"))
        {
            grounded = true;
        }
    }

    void OnCollisionExit(Collision other)
    {
        // ?n????痣??
        if (other.gameObject.CompareTag("Planet") ||
            other.gameObject.CompareTag("Stage"))
        {
            grounded = false;
        }
    }
}
