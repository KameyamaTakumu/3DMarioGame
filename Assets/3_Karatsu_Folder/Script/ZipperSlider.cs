using UnityEngine;
using UnityEngine.InputSystem;

public class ZipperSlider : MonoBehaviour
{
    [Header("参照")]
    public Transform[] railPoints;    //レールのポイント（チャックが移動するポイント）
    public ZipperWall targetWall;    //連動する壁のスクリプト

    [Header("設定")]
    public float moveSpeed = 5.0f;    //チャックの移動速度

    [Header("操作設定")]
    [SerializeField] private bool invertY = false; // 上下入力の反転
    [SerializeField] private bool invertX = false; // 左右入力の反転

    private float currentProgress = 0;    // 0.0（始点）〜 1.0（終点）
    private bool isCaptured = false;    //チャックが捕まっているかどうか
    private bool isCompleted = false;    //チャックが終点に到達したかどうか

    private Vector2 rawInput;//スティックの生の入力値


    void Update()
    {
        // 【テスト用】Tキーでキャプチャー切り替え
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (isCaptured) OnReleased();
            else OnCaptured();
        }

        if (!isCaptured || isCompleted) return;

        // --- 入力とレールの向きを計算して進捗を更新 ---
        HandleMovementInput();

        // 終点チェック
        if (currentProgress >= 0.99f)
        {
            CompleteZip();
        }
    }

    //スティックの入力を処理してチャックの位置を更新する関数
    private void HandleMovementInput()
    {
        // 入力がない、またはレールが設定されていないなら何もしない
        if (rawInput.sqrMagnitude < 0.01f || railPoints.Length < 2) return;

        // 現在のセグメントのインデックスを計算
        float floatIndex = currentProgress * (railPoints.Length - 1);
        int startIndex = Mathf.FloorToInt(floatIndex);
        int endIndex = Mathf.CeilToInt(floatIndex);

        // 始点と終点が重なっている場合の補正
        if (startIndex == endIndex)
        {
            if (endIndex < railPoints.Length - 1) endIndex++;
            else startIndex--;
        }

        // レールの向き（ベクトル）を取得
        Vector3 railDir = (railPoints[endIndex].position - railPoints[startIndex].position).normalized;

        // 入力値(moveAmount)の決定
        // レールが「上下」に強いか「左右/奥行」に強いかで、使うスティックの軸を自動選択する
        float moveAmount = 0f;

        // レールが「縦方向」か「横/奥行方向」かで判定
        if (Mathf.Abs(railDir.y) > 0.7f)
        {
            // --- 縦レールの処理 ---
            moveAmount = (railDir.y > 0) ? rawInput.y : -rawInput.y;
            if (invertY) moveAmount *= -1f; // 縦専用の反転
        }
        else
        {
            // --- 横・奥行レールの処理 ---
            float horizontalDot = railDir.x + railDir.z; // 簡易的な向き判定
            moveAmount = (horizontalDot > 0) ? rawInput.x : -rawInput.x;
            
            if (invertX) moveAmount *= -1f; // 横専用の反転
        }

        // 進捗（0〜1）の更新
        // レールの総数で割ることで、点の数に関わらず一定の速度で移動させる
        currentProgress += moveAmount * moveSpeed * Time.deltaTime / (railPoints.Length - 1);
        currentProgress = Mathf.Clamp01(currentProgress);

        // 座標と回転を更新
        UpdateSliderPosition();
    }


    public void OnMove(InputValue value)
    {
        if (!isCaptured || isCompleted) return;

        // スティックの上下（または左右）の入力を取得
        Vector2 input = value.Get<Vector2>();
        rawInput = value.Get<Vector2>();
    }


    //チャックの位置をレールポイントに沿って更新する関数
    void UpdateSliderPosition()
    {
        if (railPoints.Length < 2) return;

        // 全体の進捗から、今どの点とどの点の間にいるかを計算
        float floatIndex = currentProgress * (railPoints.Length - 1);
        int startIndex = Mathf.FloorToInt(floatIndex);
        int endIndex = Mathf.CeilToInt(floatIndex);
        float segmentProgress = floatIndex - startIndex;

        // 2点間を線形補完して位置を決める
        transform.position = Vector3.Lerp(
            railPoints[startIndex].position,
            railPoints[endIndex].position,
            segmentProgress
        );

        // つまみの向きをレールの方向に合わせる
        if (startIndex != endIndex)
        {
            // 次のポイントの座標を取得
            Vector3 targetPos = railPoints[endIndex].position;

            // 自分の位置と次のポイントが重なっていないときだけ向く
            if (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.LookAt(targetPos);
            }
        }
    }

    //チャックが終点に到達したときの処理
    void CompleteZip()
    {
        isCompleted = true;    //終点に到達した状態にする
        Debug.Log("チャック全開！");

        if (targetWall != null)
        {
            //連動する壁のスクリプトを呼び出して、壁を開く処理を開始する
            targetWall.OpenZipperWall();
        }
    }

    public void OnCaptured()
    {
        isCaptured = true;
        Debug.Log("チャック：キャプチャー開始");
    }

    public void OnReleased()
    {
        isCaptured = false;
        rawInput = Vector2.zero;
        Debug.Log("チャック：キャプチャー解除");
    }

    public bool IsCaptured() => isCaptured;
}
