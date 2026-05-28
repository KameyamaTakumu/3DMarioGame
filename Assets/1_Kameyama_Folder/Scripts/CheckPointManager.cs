//using UnityEngine;

///// <summary>
///// チェックポイントの座標を管理するシングルトン
///// </summary>
//public class CheckpointManager : MonoBehaviour
//{
//    public static CheckpointManager Instance { get; private set; }

//    [Header("初期スポーン設定")]
//    [Tooltip("ゲーム開始時のスポーン座標（未設定時はプレイヤーの初期位置を使用）")]
//    public Transform defaultSpawnPoint;

//    // 現在有効なリスポーン座標
//    private Vector3 respawnPosition;
//    private bool hasCheckpoint = false;

//    void Awake()
//    {
//        // シングルトン設定
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//        DontDestroyOnLoad(gameObject); // シーンをまたいでも維持

//        // デフォルトスポーン座標を設定
//        if (defaultSpawnPoint != null)
//        {
//            respawnPosition = defaultSpawnPoint.position;
//            hasCheckpoint = true;
//        }
//    }

//    /// <summary>
//    /// チェックポイントを更新する
//    /// </summary>
//    /// <param name="position">新しいリスポーン座標</param>
//    public void SetCheckpoint(Vector3 position)
//    {
//        respawnPosition = position;
//        hasCheckpoint = true;
//        Debug.Log($"チェックポイント更新: {position}");
//    }

//    /// <summary>
//    /// 現在のリスポーン座標を取得する
//    /// </summary>
//    /// <param name="fallback">チェックポイント未設定時の代替座標</param>
//    public Vector3 GetRespawnPosition(Vector3 fallback)
//    {
//        return hasCheckpoint ? respawnPosition : fallback;
//    }

//    /// <summary>
//    /// チェックポイントが設定済みかどうか
//    /// </summary>
//    public bool HasCheckpoint => hasCheckpoint;
//}
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// チェックポイントの座標を管理するシングルトン
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("初期スポーン設定")]
    [Tooltip("ゲーム開始時のスポーン座標（未設定時はプレイヤーの初期位置を使用）")]
    public Transform defaultSpawnPoint;

    [Header("リセット設定")]
    [Tooltip("同じステージをやり直したとき（同シーンリロード）もリセットするか")]
    public bool resetOnSameScene = true;

    // 現在有効なリスポーン座標
    private Vector3 respawnPosition;
    private bool hasCheckpoint = false;

    // チェックポイントを記録したシーン名
    private string checkpointScene = "";

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

        // シーンロードイベントを登録
        SceneManager.sceneLoaded += OnSceneLoaded;

        // デフォルトスポーン座標を設定
        if (defaultSpawnPoint != null)
        {
            respawnPosition = defaultSpawnPoint.position;
            hasCheckpoint = true;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// シーンがロードされるたびに呼ばれる
    /// </summary>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isSameScene = (scene.name == checkpointScene);

        // 同シーンリロード → resetOnSameScene の設定に従う
        // 別シーンへの遷移 → 常にリセット
        if (!isSameScene || resetOnSameScene)
        {
            ResetCheckpoint();
            Debug.Log($"チェックポイントリセット（シーン: {scene.name}）");
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
        checkpointScene = SceneManager.GetActiveScene().name; // 記録したシーンを保存
        Debug.Log($"チェックポイント更新: {position}（シーン: {checkpointScene}）");
    }

    /// <summary>
    /// チェックポイントをリセットする
    /// </summary>
    public void ResetCheckpoint()
    {
        hasCheckpoint = false;
        checkpointScene = "";
        respawnPosition = Vector3.zero;
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