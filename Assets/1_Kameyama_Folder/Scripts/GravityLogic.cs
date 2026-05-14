using UnityEngine;

// プレイヤーが惑星の表面に引き寄せられるような重力と、惑星の表面に沿って回転するような処理を行うクラス
public class GravityLogic : MonoBehaviour
{
    // Player の Transform
    private Transform myTransform;

    // Player の Rigidbody
    private Rigidbody rb = null;

    // 重力源となる惑星
    private GameObject currentPlanet;

    // Planet タグがついているオブジェクトを格納する配列
    private GameObject[] planets;

    [Header("惑星重力")]
    public float gravity = -30f;

    [Header("通常重力")]
    public float normalGravity = -9.81f;

    // 惑星ジャンプ時の力
    public float planetJumpForce = 5f;

    // 回転速度
    private int lerpRotationSpeed = 120;

    // 惑星表面の法線
    private Vector3 normalVec = Vector3.up;

    // 現在惑星モードかどうか
    private bool isPlanetGravity = false;

    void Start()
    {
        // Rigidbody を取得
        rb = GetComponent<Rigidbody>();

        // Rigidbody の回転を固定
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Unity標準重力を使わない
        rb.useGravity = false;

        // Transform を保存
        myTransform = transform;

        // 最初の惑星を取得
        currentPlanet = ChoosePlanet();
    }

    void FixedUpdate()
    {
        // Planet が存在するなら距離チェック
        CheckPlanetGravity();

        // 重力処理
        ApplyGravity();

        // 惑星上なら法線取得
        if (isPlanetGravity)
        {
            RayTest();
        }
    }

    // =========================================
    // 重力モード切り替え
    // =========================================
    void CheckPlanetGravity()
    {
        if (currentPlanet == null)
        {
            isPlanetGravity = false;
            return;
        }

        // 惑星との距離
        float distance =
            Vector3.Distance(transform.position, currentPlanet.transform.position);

        // 惑星半径っぽい値
        float planetRadius =
            currentPlanet.transform.localScale.x * 0.5f;

        // 惑星に近ければ惑星重力
        if (distance <= planetRadius + 5f)
        {
            isPlanetGravity = true;
        }
        else
        {
            isPlanetGravity = false;
        }
    }

    // =========================================
    // 重力適用
    // =========================================
    void ApplyGravity()
    {
        // ===== 惑星重力 =====
        if (isPlanetGravity)
        {
            // 法線方向
            Vector3 gravityUp = normalVec;

            // 現在の上方向
            Vector3 bodyUp = myTransform.up;

            // 惑星方向へ重力
            rb.AddForce(gravityUp * gravity);

            // 惑星表面に合わせて回転
            Quaternion targetRotation =
                Quaternion.FromToRotation(bodyUp, gravityUp) * myTransform.rotation;

            myTransform.rotation = Quaternion.Lerp(
                myTransform.rotation,
                targetRotation,
                lerpRotationSpeed * Time.deltaTime
            );
        }

        // ===== 通常重力 =====
        else
        {
            // 通常重力
            rb.AddForce(Vector3.up * normalGravity);

            // 姿勢を徐々に戻す
            Quaternion targetRotation =
                Quaternion.Euler(0, transform.eulerAngles.y, 0);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                5f * Time.deltaTime
            );
        }
    }

    // =========================================
    // 最も近い惑星を取得
    // =========================================
    GameObject ChoosePlanet()
    {
        planets = GameObject.FindGameObjectsWithTag("Planet");

        if (planets.Length == 0)
        {
            return null;
        }

        GameObject nearestPlanet = null;

        float minDistance = Mathf.Infinity;

        foreach (GameObject p in planets)
        {
            float dist =
                Vector3.Distance(transform.position, p.transform.position);

            if (dist < minDistance)
            {
                minDistance = dist;
                nearestPlanet = p;
            }
        }

        return nearestPlanet;
    }

    // =========================================
    // 惑星法線取得
    // =========================================
    void RayTest()
    {
        if (currentPlanet == null) return;

        // 惑星中心方向
        Vector3 direction =
            currentPlanet.transform.position - transform.position;

        // Ray 発射
        Ray ray = new Ray(transform.position, direction);

        RaycastHit hit;

        // Ray が当たった場合
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            // Planet に当たったら法線取得
            if (hit.collider.CompareTag("Planet"))
            {
                normalVec = hit.normal;
            }
        }
    }

    // =========================================
    // 重力切り替え
    // =========================================
    public void ReverseGravity()
    {
        Debug.Log("gravity Reversed!");

        GameObject nextPlanet = ChooseSecondPlanet();

        if (nextPlanet != null)
        {
            currentPlanet = nextPlanet;

            Vector3 jumpDir =
                (currentPlanet.transform.position - transform.position).normalized;

            rb.AddForce(jumpDir * planetJumpForce, ForceMode.Impulse);
        }
    }

    // =========================================
    // 二番目に近い惑星取得
    // =========================================
    GameObject ChooseSecondPlanet()
    {
        planets = GameObject.FindGameObjectsWithTag("Planet");

        GameObject nearest = null;
        GameObject second = null;

        float minDist = Mathf.Infinity;
        float secondDist = Mathf.Infinity;

        foreach (GameObject p in planets)
        {
            float dist =
                Vector3.Distance(transform.position, p.transform.position);

            if (dist < minDist)
            {
                secondDist = minDist;
                second = nearest;

                minDist = dist;
                nearest = p;
            }
            else if (dist < secondDist)
            {
                secondDist = dist;
                second = p;
            }
        }

        Debug.Log(second.gameObject.name);

        return second;
    }

    // 現在惑星重力かどうか取得
    public bool IsPlanetGravity()
    {
        return isPlanetGravity;
    }
}
