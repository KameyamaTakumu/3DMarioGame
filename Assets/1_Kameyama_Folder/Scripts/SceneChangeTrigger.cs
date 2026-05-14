using UnityEngine;
using UnityEngine.SceneManagement;

// オブジェクトにあるトリガーに触れるとシーン遷移するクラス
public class SceneChangeTrigger : MonoBehaviour
{
    [SerializeField,CustomLabel("遷移したいシーン名")]
    private SceneObject sceneName;

    private void OnTriggerEnter(Collider other)
    {
        // 衝突したオブジェクトが Player タグを持っているかどうかを確認する
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneName);

            Debug.Log("シーン遷移しました");
        }
    }
}
