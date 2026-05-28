using UnityEngine;

/// <summary>
/// チェックポイントの座標を管理するシングルトン
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("初期スポーン設定")]
    [Tooltip("ゲーム開始時のスポーン座標（未設定時はプレイヤーの初期位置を使用）")]
    public Transform defaultSpawnPoint;

    // 現在有効なリスポーン座標
    private Vector3 respawnPosition;
    private bool hasCheckpoint = false;

    void Awake()
    {
        // シングルトン設定
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // シーンをまたいでも維持

        // デフォルトスポーン座標を設定
        if (defaultSpawnPoint != null)
        {
            respawnPosition = defaultSpawnPoint.position;
            hasCheckpoint = true;
        }
    }

    /// <summary>
    /// チェックポイントを更新する
    /// </summary>
    /// <param name="position">新しいリスポーン座標</param>
    public void SetCheckpoint(Vector3 position)
    {
        respawnPosition = position;
        hasCheckpoint = true;
        Debug.Log($"チェックポイント更新: {position}");
    }

    /// <summary>
    /// 現在のリスポーン座標を取得する
    /// </summary>
    /// <param name="fallback">チェックポイント未設定時の代替座標</param>
    public Vector3 GetRespawnPosition(Vector3 fallback)
    {
        return hasCheckpoint ? respawnPosition : fallback;
    }

    /// <summary>
    /// チェックポイントが設定済みかどうか
    /// </summary>
    public bool HasCheckpoint => hasCheckpoint;
}