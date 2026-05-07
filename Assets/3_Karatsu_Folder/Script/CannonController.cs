using UnityEngine;
using UnityEngine.InputSystem;

public class CannonController : MonoBehaviour
{
    [Header("Cannon Settings")]
    [SerializeField] private float rotationSpeed = 30f; // 回転速度
    [SerializeField] private float horizontalRange = 45f; // 水平方向の回転範囲
    [SerializeField] private float verticalRange = 30f; // 垂直方向の回転範囲

    [Header("Launch Settings")]
    [SerializeField] private GameObject marioBulletPrefab; // 発射する弾のプレハブ
    [SerializeField] private Transform shootPoint; // 弾を発射する位置
    [SerializeField] private float shootForce = 40f; // 発射する力

    [Header("キャプチャーテスト")]
    [SerializeField] private bool isCaptured = false; // キャプチャー中かどうか

    private Vector2 lookInput;      // プレイヤーの入力を格納する変数
    private float hRotation = 0f;   // 水平方向の回転角度
    private float vRotation = 0f;   // 垂直方向の回転角度


    void Update()
    {
        // 【テスト用】Tキーで切り替え
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (isCaptured) OnReleased();
            else OnCaptured();
        }

    }

    // InputSystemからの入力を受け取る
    public void OnAim(InputValue value)
    {
        if (isCaptured)
        {
            lookInput = value.Get<Vector2>();
        }
    }

    public void OnLaunch(InputValue value)
    {
        // 押された瞬間のみ実行
        if (isCaptured && value.isPressed)
        {
            Shoot();
        }
    }

    void LateUpdate()
    {
        if (!isCaptured) return;

        // 大砲の向きを計算
        hRotation += lookInput.x * rotationSpeed * Time.deltaTime;
        vRotation -= lookInput.y * rotationSpeed * Time.deltaTime;

        hRotation = Mathf.Clamp(hRotation, -horizontalRange, horizontalRange);
        vRotation = Mathf.Clamp(vRotation, -verticalRange, verticalRange);

        transform.localRotation = Quaternion.Euler(vRotation, hRotation, 0);
    }

    private void Shoot()
    {
        // プレハブや発射位置が設定されていない場合は処理しない
        if (marioBulletPrefab == null || shootPoint == null) return;

        // 弾を生成して飛ばす
        GameObject bullet = Instantiate(marioBulletPrefab, shootPoint.position, shootPoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(shootPoint.forward * shootForce, ForceMode.Impulse);
        }

        // 発射したら自分自身のキャプチャーは解除
        OnReleased();
        Debug.Log("大砲発射：キャプチャー解除");

        // ※マリオ担当の人へ：ここでマリオのモデルを非表示にする等の処理を呼んでもらう
    }

    public void OnCaptured()
    {
        isCaptured = true;
        lookInput = Vector2.zero;
        Debug.Log("大砲キャプチャー：InputActionで操作可能");
    }

    public void OnReleased()
    {
        isCaptured = false;
        lookInput = Vector2.zero;
    }
}
