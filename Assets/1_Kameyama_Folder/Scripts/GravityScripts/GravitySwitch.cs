using UnityEngine;

// プレイヤーが重力の反転するエリアに入ったときに、GravityLogic クラスの ReverseGravity 関数を呼び出すクラス
public class GravitySwitch : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 衝突したオブジェクトが Player タグを持っているかどうかを確認する
        if (other.CompareTag("Player"))
        {
            // Player タグがついているオブジェクトの GravityLogic を取得する
            GravityLogic gravity = other.GetComponent<GravityLogic>();

            if (gravity != null)
            {
                // GravityLogic クラスの ReverseGravity 関数を呼び出す
                gravity.ReverseGravity();
            }
        }
    }
}
