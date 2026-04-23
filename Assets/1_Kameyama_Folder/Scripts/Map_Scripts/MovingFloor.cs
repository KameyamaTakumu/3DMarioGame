using UnityEngine;

// オブジェクトが左右に移動するクラス
public class MovingFloor : MonoBehaviour
{
    // オブジェクトの移動する速さ
    public int _speed;

    // オブジェクトが移動する距離
    public int _moveDistance;

    // オブジェクトの初期位置を代入する変数
    private Vector3 _startPos;

    // オブジェクトについている Rigidbody を代入する変数
    private Rigidbody _rb;

    void Start()
    {
        // オブジェクトの初期位置を startPos に代入する
        _startPos = transform.position;

        // オブジェクトについている Rigidbody を取得して rb に代入する
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // オブジェクトの位置を時間に応じて変化させる
        // Mathf.PingPong は、0 から moveDistance までの値を時間に応じて繰り返し変化させる関数
        // moveDistance はオブジェクトが移動する距離を表す
        float posX = _startPos.x + Mathf.PingPong(Time.time * _speed, _moveDistance); 

        // Rigidbody を使用してオブジェクトの位置を更新する
        _rb.MovePosition(new Vector3(posX, _startPos.y, _startPos.z)); 
    }
}
