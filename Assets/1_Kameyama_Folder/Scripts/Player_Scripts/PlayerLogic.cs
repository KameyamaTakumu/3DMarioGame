using UnityEngine;

// プレイヤーの移動や回転、ジャンプの処理を行うクラス
public class PlayerLogic : MonoBehaviour
{
    // プレイヤーの移動する速さ
    [CustomLabel("移動速度"),SerializeField] 
    float _moveSpeed = 15;

    // プレイヤーの回転する速さ
    public float _rotateSpeed = 5;

    // プレイヤーの回転する向き
    //  1(プレイヤーから見て)  時計回り
    // -1(プレイヤーから見て)反時計回り
    private int _rotateDirection = 0;

    // プレイヤーの Rigidbody
    private Rigidbody _rb = null;

    // 地面に着地しているか判定する変数
    public bool _grounded;

    // ジャンプ力
    public float _jumpPower;

    void Start()
    {
        // オブジェクトについている Rigidbody を取得して rb に代入する
        _rb = this.GetComponent<Rigidbody>();
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
        Vector3 _moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        // プレイヤーの進行方向に速さをかけて、プレイヤーの位置を更新する
        // Time.deltaTime を掛けることでフレームレートに依存しなくスムーズに動く
        // transform.TransformDirection はローカルな座標をワールドな座標に変換する関数
        _rb.MovePosition(_rb.position + transform.TransformDirection(_moveDirection) * _moveSpeed * Time.deltaTime);
    }

    void Jump()
    {
        if (_grounded == true)//  もし grounded が true なら
        {
            if (Input.GetKeyDown(KeyCode.Space))//  もし スペースキーが押されたなら  
            {
                _grounded = false;// grounded を false にする(無限ジャンプ防止)
                _rb.AddForce(transform.up * _jumpPower * 100);//  上に jumpPower 分垂直に力をかける
                // 重力の影響が一定ではないためプレイヤーの垂直に飛ばせる
            }
        }
    }

    void OnCollisionEnter(Collision other)//  他オブジェクトに触れた時の処理
    {
        if (other.gameObject.tag == "_planet")//  もし Planet というタグがついたオブジェクトに触れたら(地面に触れたら)
        {
            _grounded = true;// grounded を true にする(ジャンプを復活)
        }
    }

    // プレイヤーの回転の処理
    void Horizontal_Rotate()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            // プレイヤーから見て反時計回りに回転させる
            _rotateDirection = -1;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            // プレイヤーから見て時計回りに回転させる
            _rotateDirection = 1;
        }
        else
        {
            _rotateDirection = 0;
        }

        // オブジェクトからみて垂直方向を軸として回転させる Quaternion を作成
        Quaternion _rt = Quaternion.AngleAxis(_rotateDirection * _rotateSpeed, transform.up);
        // 現在の自信の回転の情報を取得する
        Quaternion _q = this.transform.rotation;
        // 合成して自身に設定
        this.transform.rotation = _rt * _q;
    }
}
