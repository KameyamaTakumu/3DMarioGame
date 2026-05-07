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
        // プレイヤーの回転の処理
        HorizontalRotate();

        // プレイヤーの進行方向を取得
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        // プレイヤーの進行方向に速さをかけて、プレイヤーの位置を更新する
        // Time.deltaTime を掛けることでフレームレートに依存しなくスムーズに動く
        // transform.TransformDirection はローカルな座標をワールドな座標に変換する関数
        rb.MovePosition(rb.position + transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);
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

    void OnCollisionEnter(Collision other)//  他オブジェクトに触れた時の処理
    {
        if (other.gameObject.tag == "planet")//  もし Planet というタグがついたオブジェクトに触れたら(地面に触れたら)
        {
            grounded = true;// grounded を true にする(ジャンプを復活)
        }
    }

    // プレイヤーの回転の処理
    void HorizontalRotate()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            // プレイヤーから見て反時計回りに回転させる
            rotateDirection = -1;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            // プレイヤーから見て時計回りに回転させる
            rotateDirection = 1;
        }
        else
        {
            rotateDirection = 0;
        }

        // オブジェクトからみて垂直方向を軸として回転させる Quaternion を作成
        Quaternion rt = Quaternion.AngleAxis(rotateDirection * rotateSpeed, transform.up);
        // 現在の自信の回転の情報を取得する
        Quaternion q = this.transform.rotation;
        // 合成して自身に設定
        this.transform.rotation = rt * q;
    }
}
