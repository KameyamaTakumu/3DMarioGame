using UnityEngine;

/// <summary>
/// チェックポイントオブジェクトにアタッチする
/// プレイヤーが触れたらリスポーン地点を更新する
/// </summary>
[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Header("チェックポイント設定")]
    [Tooltip("リスポーン座標をこのオブジェクトの位置ではなく別の場所にしたい場合に指定")]
    public Transform spawnPoint;

    [Tooltip("一度だけ有効にするか（false なら何度でも再登録可能）")]
    public bool activateOnce = true;

    private bool isActivated = false;

    void Start()
    {
        // Collider を必ずトリガーに設定
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // プレイヤータグのみ反応
        if (!other.CompareTag("Player")) return;

        // 一度だけモードで既に起動済みならスキップ
        if (activateOnce && isActivated) return;

        Activate();
    }

    /// <summary>
    /// チェックポイントを有効化してリスポーン地点を登録する
    /// </summary>
    void Activate()
    {
        isActivated = true;

        // リスポーン座標の決定（spawnPoint が未設定なら自身の座標）
        Vector3 respawn = spawnPoint != null ? spawnPoint.position : transform.position;

        // CheckpointManager に登録
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SetCheckpoint(respawn);
        }
        else
        {
            Debug.LogWarning("CheckpointManager が見つかりません。シーンに配置してください。");
        }
    }
}