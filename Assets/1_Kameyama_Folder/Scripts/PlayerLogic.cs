using UnityEngine;

// テスト用に仮で作成したクラス
// プレイヤーの移動や回転、ジャンプの処理を行うクラス
public class PlayerLogic : MonoBehaviour
{
    // プレイヤーの移動する速さ
    [CustomLabel("移動速度"),SerializeField] 
    float moveSpeed = 15;

    // プレイヤーの回転する速さ
    public float rotateSpeed = 5;

    // プレイヤーの回転する向き
    //  1(プレイヤーから見て)  時計回り
    // -1(プレイヤーから見て)反時計回り
    private int rotateDirection = 0;

    // プレイヤーの Rigidbody
    private Rigidbody rb = null;

    // 地面に着地しているか判定する変数
    public bool grounded;

    // ジャンプ力
    public float jumpPower;

    void Start()
    {
        // オブジェクトについている Rigidbody を取得して rb に代入する
        rb = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        // ジャンプの処理
        Jump();
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Transform cam = Camera.main.transform;

        // 惑星表面に沿った方向へ補正
        Vector3 camForward =
            Vector3.ProjectOnPlane(
                cam.forward,
                transform.up
            ).normalized;

        Vector3 camRight =
            Vector3.ProjectOnPlane(
                cam.right,
                transform.up
            ).normalized;

        Vector3 moveDirection =
            camForward * v +
            camRight * h;

        moveDirection.Normalize();

        // 回転
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    moveDirection,
                    transform.up
                );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );
        }

        // 移動
        rb.MovePosition(
            rb.position +
            moveDirection *
            moveSpeed *
            Time.deltaTime
        );
    }

    void Jump()
    {
        if (grounded == true)//  もし grounded が true なら
        {
            if (Input.GetKeyDown(KeyCode.Space))//  もし スペースキーが押されたなら  
            {
                grounded = false;// grounded を false にする(無限ジャンプ防止)
                rb.AddForce(transform.up * jumpPower * 100);//  上に jumpPower 分垂直に力をかける
                // 重力の影響が一定ではないためプレイヤーの垂直に飛ばせる
            }
        }
    }

    //  他オブジェクトに触れた時の処理
    void OnCollisionEnter(Collision other)
    {
        //  もし Planet というタグがついたオブジェクトに触れたら(地面に触れたら)
        if (other.gameObject.CompareTag("Planet") || other.gameObject.CompareTag("Stage"))
        {
            grounded = true;// grounded を true にする(ジャンプを復活)
        }
    }
}
