using UnityEngine;
using System.Collections;

public class OneWayGround : MonoBehaviour
{
    private float disableDuration = 0.5f;
    private bool isIgnoring = false;
    private Collider platformCollider;

    // キャッシュ用
    private Transform player;
    private Rigidbody playerRb;
    private Collider playerCollider;

    private void Start()
    {
        platformCollider = GetComponent<Collider>();
        FindPlayer();
    }

    // プレイヤーを再取得するメソッド（死亡再生成後にも呼べる）
    public void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody>();       // キャッシュ
            playerCollider = playerObj.GetComponent<Collider>();        // キャッシュ
        }
        else
        {
            Debug.LogWarning("Playerタグのオブジェクトが見つかりません。");
        }
    }

    private void FixedUpdate()
    {
        // 参照が切れていたら再取得を試みる
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // Y方向のみで判定（横スクロールに適切）
        float yDiff = transform.position.y - player.position.y;

        if (yDiff > 0f &&           // プレイヤーが床より下にいる
            yDiff < 3f &&           // 一定距離以内
            playerRb.linearVelocity.y > 0f && // 上昇中
            !isIgnoring)
        {
            StartCoroutine(DisableCollisionTemporarily());
        }
    }

    private IEnumerator DisableCollisionTemporarily()
    {
        isIgnoring = true;
        Physics.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(disableDuration);
        Physics.IgnoreCollision(playerCollider, platformCollider, false);
        isIgnoring = false;
    }
}
