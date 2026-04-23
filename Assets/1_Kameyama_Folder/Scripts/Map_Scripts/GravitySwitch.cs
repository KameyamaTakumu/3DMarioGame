using UnityEngine;

public class GravitySwitch : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 衝突したオブジェクトが Player タグを持っているかどうかを確認する
        if (other.CompareTag("Player"))
        {
            // Player タグがついているオブジェクトの Gravity_Logic を取得する
            Gravity_Logic gravity = other.GetComponent<Gravity_Logic>();

            if (gravity != null)
            {
                // Gravity_Logic クラスの ReverseGravity 関数を呼び出す
                gravity.ReverseGravity();
            }
        }
    }
}
