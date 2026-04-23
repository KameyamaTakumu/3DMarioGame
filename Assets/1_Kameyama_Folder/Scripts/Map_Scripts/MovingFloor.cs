using UnityEngine;

public class MovingFloor : MonoBehaviour
{
    // オブジェクトの移動する速さ
    public int speed;

    // オブジェクトが移動する距離
    public int moveDistance;

    // オブジェクトの初期位置を代入する変数
    private Vector3 startPos;

    // オブジェクトについている Rigidbody を代入する変数
    private Rigidbody rb;

    void Start()
    {
        // オブジェクトの初期位置を startPos に代入する
        startPos = transform.position;

        // オブジェクトについている Rigidbody を取得して rb に代入する
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // オブジェクトの位置を時間に応じて変化させる
        // Mathf.PingPong は、0 から moveDistance までの値を時間に応じて繰り返し変化させる関数
        // moveDistance はオブジェクトが移動する距離を表す
        float posX = startPos.x + Mathf.PingPong(Time.time * speed, moveDistance); 

        // Rigidbody を使用してオブジェクトの位置を更新する
        rb.MovePosition(new Vector3(posX, startPos.y, startPos.z)); 
    }
}
