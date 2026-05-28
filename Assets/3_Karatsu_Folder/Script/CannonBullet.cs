using UnityEngine;

public class CannonBullet : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // 重力の影響を受けるようにして、放物線を描かせる
        rb.useGravity = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 既に何かに当たっていたら処理しない
        if (hasHit) return;

        // ステージ（壁や床）に当たったら
        // ※Tagを"Stage"などに設定しておいてください
        if (collision.gameObject.CompareTag("Stage") || collision.gameObject.CompareTag("Wall"))
        {
            hasHit = true; // 衝突フラグを立てる
            StopAndReturn();
        }
    }

    private void StopAndReturn()
    {
        // 衝撃で少し跳ね返るのを防ぐため、動きを完全に止める
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero; // 回転も止める
        rb.isKinematic = true;            // 物理演算の影響をオフにする

        // CaptureManager取得
        CaptureManager capture =
            FindObjectOfType<CaptureManager>();

        if (capture != null)
        {
            // 少し上に出すと地面埋まり防止になる
            Vector3 spawnPos =
                transform.position + Vector3.up * 1.0f;

            // 上に出して、少し後ろに下げる
            spawnPos += -transform.forward * 1f;

            // プレイヤー復活
            capture.RespawnPlayer(spawnPos);
        }

        //Debug.Log("着弾：ここでマリオを出現させる座標：" + transform.position);
        //// ※マリオ担当の人へ：この座標 transform.position にマリオを移動させて復活させてね

        Destroy(gameObject, 0.1f); // 役目を終えたら消える
    }
}
