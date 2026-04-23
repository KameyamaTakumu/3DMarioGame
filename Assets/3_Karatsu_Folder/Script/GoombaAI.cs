using UnityEngine;

public class GoombaAI : MonoBehaviour
{
    [SerializeField] private float walkRange = 3f;
    [SerializeField] private float speed = 1.5f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    // AIの移動ベクトルを返すメソッド
    public Vector2 GetAIMovement()
    {
        // 単純な往復運動(仮) 
        float pingPong = Mathf.PingPong(Time.time * speed, walkRange * 2) - walkRange;
        return new Vector2(0, pingPong > 0 ? 1 : -1); // 前後に動く

    }
}
