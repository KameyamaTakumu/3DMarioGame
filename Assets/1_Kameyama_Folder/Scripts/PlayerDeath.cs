using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 一定高度より下に落下したら死亡させる
/// </summary>
public class PlayerDeath : MonoBehaviour
{
    [Header("落下死亡設定")]
    [CustomLabel("死亡判定高度")]
    public float deathY = -100f;

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

        // シーンリロード
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
