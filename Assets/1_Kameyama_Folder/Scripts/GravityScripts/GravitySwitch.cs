using UnityEngine;

// プレイヤーが入ると指定惑星へ飛ばす
public class GravitySwitch : MonoBehaviour
{
    [Header("飛ばす先の惑星")]
    public GameObject targetPlanet;

    private void OnTriggerEnter(Collider other)
    {
        // Player 判定
        if (other.CompareTag("Player"))
        {
            // GravityLogic 取得
            GravityLogic gravity =
                other.GetComponent<GravityLogic>();

            if (gravity != null)
            {
                // 指定惑星へ切り替え
                gravity.SetPlanet(targetPlanet);
            }
        }
    }
}
