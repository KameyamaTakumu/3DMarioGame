using UnityEngine;

// プレイヤーが重力の反転するエリアに入ったときに、Gravity_Logic クラスの ReverseGravity 関数を呼び出すクラス
public class GravitySwitch : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 衝突したオブジェクトが Player タグを持っているかどうかを確認する
        if (other.CompareTag("Player"))
        {
            // Player タグがついているオブジェクトの Gravity_Logic を取得する
            GravityLogic _gravity = other.GetComponent<GravityLogic>();

            if (_gravity != null)
            {
                // Gravity_Logic クラスの ReverseGravity 関数を呼び出す
                _gravity.ReverseGravity();
            }
        }
    }
}
