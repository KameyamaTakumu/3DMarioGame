using UnityEngine;
using UnityEngine.InputSystem;

public class UprootController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 3.0f; // 移動速度
    [SerializeField] private float rotationSpeed = 10; // 回転速度

    [Header("伸縮設定")]
    [SerializeField] private float maxLegLength = 2.0f; // 足の最大伸縮長
    [SerializeField] private float normalLegLength = 1.0f; // 足の通常長
    [SerializeField] private float stretchSpeed = 8.0f; // 足の伸びる速さ
    [SerializeField] private float shrinkSpeed = 30.0f; // 足の縮む速さ

    [Header("ジャンプ設定")]
    [SerializeField] private float jumpForce = 5.0f; // ジャンプの力
    [SerializeField] private float groundCheckDistance = 0.2f;  // 地面との距離（ジャンプできるかの判定に使用）

    [Header("参照")]
    [SerializeField] private Transform bodyMesh; // 伸びるモデル部分
    [SerializeField] private Transform headPart; // 頭（当たり判定の基準）

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isCaptured = false;
    private float currentLegLength = 1.0f;  // 現在の足の長さ
    private bool isStretching = false; // 今ボタンを押して伸びているか
    private bool canStretch = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // 回転は自分で制御するので物理演算で回転させないようにする
        rb.freezeRotation = true;
        currentLegLength = normalLegLength;
    }

    void Update()
    {
        // 【テスト用】Tキーを押すたびにキャプチャー状態を切り替える
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (isCaptured)
            {
                OnReleased();
                Debug.Log("テスト解除: 操作不可");
            }
            else
            {
                OnCaptured();
                Debug.Log("テスト開始: 操作可能");
            }
        }

        HandleStretching();
    }

    private void HandleStretching()
    {
        // キャプチャー中のみボタン入力を有効にする
        bool isPressing = isCaptured && Keyboard.current.spaceKey.isPressed;

        // 伸ばし直し禁止中は伸ばせない
        if (!canStretch)
            isPressing = false;

        // --- 離した瞬間判定 ---
        bool wasStretching = isStretching;

        // 現在の伸縮状態を更新
        isStretching = isPressing;
        // 伸ばしている状態から離した瞬間を判定
        bool released = wasStretching && !isPressing;
        
        bool isOnGround = Mathf.Abs(rb.linearVelocity.y) < 0.01f;

        // ボタンを放した瞬間のジャンプ判定
        if (isCaptured && released && isOnGround)
        {
                PrepareJump();
        }

        // 伸び始めた瞬間に伸ばし直し禁止
        if (!wasStretching && isPressing)
        {
            canStretch = false;
        }

        // 地面に着いたら伸ばし直し解除
        if (IsGrounded())
        {
            canStretch = true;
        }

        // 伸縮の目標長さと速度を決定
        float targetLength = isStretching ? maxLegLength : normalLegLength;
        float currentSpeed = isStretching ? stretchSpeed : shrinkSpeed;

        // 足の長さを滑らかに変化させる
        currentLegLength = Mathf.MoveTowards(
            currentLegLength,
            targetLength,
            Time.deltaTime * currentSpeed
        );

        // 頭は基準点なので常に localPosition = 0
        if (headPart != null)
            headPart.localPosition = Vector3.zero;

        // 足の見た目を更新（頭が基準）
        if (bodyMesh != null)
        {
            bodyMesh.localScale = new Vector3(1, currentLegLength, 1);
            bodyMesh.localPosition = new Vector3(0, -currentLegLength, 0);
        }

    }

    private void PrepareJump()
    {
        // 足が縮む反動でジャンプ
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            jumpForce,
            rb.linearVelocity.z
        );

        // 足を通常長に戻す（見た目だけ）
        currentLegLength = normalLegLength;

        if (bodyMesh != null)
            bodyMesh.localScale = new Vector3(1, currentLegLength, 1);

        if (headPart != null)
            headPart.localPosition = new Vector3(0, 0, 0); // 頭は基準点なので常に0

        Debug.Log("ジャンプ！");
    }

    private bool IsGrounded()
    {
        // 頭の位置から足の長さ分下にある点を計算
        Vector3 footPos = headPart.position - Vector3.up * currentLegLength;

        // 足の先端から少し上に出して、そこから下にレイを飛ばして地面をチェック
        return Physics.Raycast(
                  footPos + Vector3.up * 0.02f,
                  Vector3.down,
                  groundCheckDistance
              );
    }

    void FixedUpdate()
    {
        // キャプチャーされていないときは慣性で動く（空中での微調整を防止）
        if (!isCaptured)
        {
            // 慣性消し
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        // 移動（空中でも少し動けるように調整）
        Vector3 targetVelocity = new Vector3(moveInput.x * moveSpeed, rb.linearVelocity.y, moveInput.y * moveSpeed);
        rb.linearVelocity = targetVelocity;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 lookDirection = new Vector3(moveInput.x, 0, moveInput.y);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), rotationSpeed * Time.fixedDeltaTime);
        }
    }


    // --- インターフェース ---
    public void OnMove(InputValue value) => moveInput = isCaptured ? value.Get<Vector2>() : Vector2.zero;

    public void OnCaptured() { isCaptured = true; }
    public void OnReleased() { isCaptured = false; isStretching = false; }
    public bool IsCaptured() => isCaptured;

    // デバッグ用の線を表示
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * (groundCheckDistance + 0.1f));
    }
}
