using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(Rigidbody))]
public class GoombaController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float rotationSpeed = 10.0f;   // 回転の速さ（大きいほど素早く向きを変える）

    [Header("キャプチャーテスト")]
    [SerializeField] private bool isCaptured = false; 

    private Rigidbody rb;
    private Vector2 moveInput;
    private GoombaAI aiScript;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // 物理演算で転ばないように軸を固定
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        aiScript = GetComponent<GoombaAI>();
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
    }

    // --- PlayerInput (Send Messages) から呼ばれるメソッド ---
    public void OnMove(InputValue value)
    {
        if (isCaptured)
        {
            moveInput = value.Get<Vector2>();
        }
    }

    void FixedUpdate()
    {
        if (!isCaptured)
        {
            // --- AI自動走行の呼び出し (コメントアウトしたら止まったまま) ---
            //if (aiScript != null)
            //{
            //    moveInput = aiScript.GetAIMovement();
            //    ApplyMovement();
            //    return; // 下の停止処理をスキップ
            //}

            // 慣性で滑らないように速度を落とす
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        if (moveInput.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity =
                new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        //==============================
        // カメラ基準方向
        //==============================

        Transform cam = Camera.main.transform;

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        // 上下方向を除去
        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        // 入力方向
        Vector3 moveDirection =
            camForward * moveInput.y +
            camRight * moveInput.x;

        moveDirection.Normalize();

        //==============================
        // 移動
        //==============================

        Vector3 targetVelocity =
            moveDirection * moveSpeed;

        targetVelocity.y = rb.linearVelocity.y;

        rb.linearVelocity = targetVelocity;

        //==============================
        // 回転
        //==============================

        Quaternion targetRotation =
            Quaternion.LookRotation(moveDirection);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );
    }

    // --- 本実装で使う用のメソッド（マリオ担当が呼ぶ想定） ---
    public void OnCaptured()
    {
        isCaptured = true;
        moveInput = Vector2.zero;
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    // マリオ担当がキャプチャー解除したときに呼ぶ想定
    public void OnReleased()
    {
        isCaptured = false;
        moveInput = Vector2.zero;
    }

    // isCaptured の現在の状況を確認することができる読み取り関数
    public bool IsCaptured() => isCaptured;
}
