using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 一定高度より下に落下したら死亡させる
/// チェックポイントが設定されている場合はその地点から復活する
/// </summary>
public class PlayerDeath : MonoBehaviour
{
    [Header("落下死亡設定")]
    [CustomLabel("死亡判定高度")]
    public float deathY = -100f;

    [Header("リスポーン設定")]
    [Tooltip("チェックポイントが未設定の場合にシーン全体をリロードするか")]
    public bool reloadSceneIfNoCheckpoint = true;

    // 起動時のプレイヤー座標を初期リスポーン地点として保持
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        // プレイヤーのY座標が一定以下になったら
        if (transform.position.y <= deathY)
        {
            Die();
        }
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    void Die()
    {
        Debug.Log("落下死");

        // CheckpointManager が存在する場合はリスポーン
        if (CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint)
        {
            Respawn();
        }
        else if (reloadSceneIfNoCheckpoint)
        {
            // チェックポイント未設定 → シーンリロード（従来の挙動）
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            // チェックポイント未設定 → 初期座標へ戻す
            Respawn();
        }
    }

    /// <summary>
    /// チェックポイントまたは初期位置にリスポーンする
    /// </summary>
    void Respawn()
    {
        Vector3 respawnPos = CheckpointManager.Instance != null
            ? CheckpointManager.Instance.GetRespawnPosition(initialPosition)
            : initialPosition;

        transform.position = respawnPos;
        Debug.Log($"リスポーン: {respawnPos}");

        // Rigidbody がある場合は速度をリセット
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}