using UnityEngine;

public class Player_Logic : MonoBehaviour
{
    //プレイヤーの移動する速さ
    public float move_speed = 15;

    //プレイヤーの回転する速さ
    public float rotate_speed = 5;

    //プレイヤーの回転する向き
    //1 -> （プレイヤーから見て）時計回り
    //-1 -> （プレイヤーから見て）反時計回り
    private int rotate_direction = 0;

    //プレイヤーのRigidbody
    private Rigidbody Rig = null;

    //地面に着地しているか判定する変数
    public bool Grounded;

    //ジャンプ力
    public float Jumppower;


    void Start()
    {
        // オブジェクトについている Rigidbody を取得して Rigに代入する
        Rig = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        // ジャンプの処理
        Jump();
    }

    private void FixedUpdate()
    {
        // プレイヤーの回転の処理
        Horizontal_Rotate();

        // プレイヤーの進行方向を取得
        Vector3 move_direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        // プレイヤーの進行方向に速さをかけて、プレイヤーの位置を更新する
        // Time.deltaTime を掛けることでフレームレートに依存しなくスムーズに動く
        // transform.TransformDirection はローカルな座標をワールドな座標に変換する関数
        Rig.MovePosition(Rig.position + transform.TransformDirection(move_direction) * move_speed * Time.deltaTime);
    }

    void Jump()
    {
        if (Grounded == true)//  もし Grounded が true なら
        {
            if (Input.GetKeyDown(KeyCode.Space))//  もし、スペースキーが押されたなら  
            {
                Grounded = false;//  Groundedをfalseにする(無限ジャンプ防止)
                Rig.AddForce(transform.up * Jumppower * 100);//  上に JumpPower 分垂直に力をかける
                // 重力の影響が一定ではないためプレイヤーの垂直に飛ばせる
            }
        }
    }

    void OnCollisionEnter(Collision other)//  他オブジェクトに触れた時の処理
    {
        if (other.gameObject.tag == "Planet")//  もし Planet というタグがついたオブジェクトに触れたら(地面に触れたら)
        {
            Grounded = true;//  Groundedをtrueにする(ジャンプを復活)
        }
    }

    // プレイヤーの回転の処理
    void Horizontal_Rotate()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            // プレイヤーから見て反時計回りに回転させる
            rotate_direction = -1;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            // プレイヤーから見て時計回りに回転させる
            rotate_direction = 1;
        }
        else
        {
            rotate_direction = 0;
        }

        // オブジェクトからみて垂直方向を軸として回転させる Quaternionを 作成
        Quaternion rot = Quaternion.AngleAxis(rotate_direction * rotate_speed, transform.up);
        // 現在の自信の回転の情報を取得する。
        Quaternion q = this.transform.rotation;
        // 合成して自身に設定
        this.transform.rotation = rot * q;
    }
}
