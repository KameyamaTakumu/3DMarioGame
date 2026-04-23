using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(Rigidbody))]
public class GoombaController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float rotationSpeed = 10.0f;

    [Header("Test Mode")]
    [SerializeField] private bool isCaptured = false; 

    private Rigidbody rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // 物理演算で転ばないように軸を固定
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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
            // 慣性で滑らないように速度を落とす
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        // 1. 移動：現在の重力を維持しつつX,Z軸を動かす
        Vector3 targetVelocity = new Vector3(moveInput.x * moveSpeed, rb.linearVelocity.y, moveInput.y * moveSpeed);
        rb.linearVelocity = targetVelocity;

        // 2. 回転：入力があるときだけ、進行方向を向く
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 lookDirection = new Vector3(moveInput.x, 0, moveInput.y);
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // --- 本実装で使う用のメソッド（マリオ担当が呼ぶ想定） ---
    public void OnCaptured()
    {
        isCaptured = true;
    }

    public void OnReleased()
    {
        isCaptured = false;
        moveInput = Vector2.zero;
    }
}
