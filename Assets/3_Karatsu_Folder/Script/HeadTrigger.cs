using UnityEngine;

public class HeadTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //// 当たった相手が「BreakableBlock」というコンポーネントを持っていたら実行
        //if (other.TryGetComponent<BreakableBlock>(out var block))
        //{
        //    block.Break(); // ブロックを壊す関数を呼ぶ
        //}
    }
}