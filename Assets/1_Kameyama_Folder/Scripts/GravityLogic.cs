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

        return second;
    }

    // 現在惑星重力かどうか取得
    public bool IsPlanetGravity()
    {
        return isPlanetGravity;
    }
    //// Player の Transform
    //private Transform myTransform;

    //// Player の Rigidbody
    //private Rigidbody rb = null;

    ////重力減となる惑星
    //private GameObject planet;

    //// Planet タグがついているオブジェクトを格納する配列
    //private GameObject[] planets;

    //// 重力の強さ
    //public float gravity;

    //// 惑星に対する Player の向き
    //private Vector3 direction;

    //// Ray が接触した惑星のポリゴンの法線
    //private Vector3 normalVec = new Vector3(0, 0, 0);

    //// 現在の惑星
    //private GameObject currentPlanet;

    //// 惑星をジャンプする力
    //public float planetJumpForce = 5f;

    //// Player の球体回転の速さ
    //private int lerpRotationSpeed = 120;

    //void Start()
    //{
    //    // Player の Rigidbody を取得して rb に代入する
    //    rb = this.GetComponent<Rigidbody>();

    //    // すでに設定済みだが念のためにこちらでも設定する
    //    // Player の Rigidbody の回転を固定する
    //    rb.constraints = RigidbodyConstraints.FreezeRotation;
    //    // Player の Rigidbody の重力を無効にする
    //    rb.useGravity = false;

    //    // このオブジェクトの Transform を取得して myTransform に代入する
    //    myTransform = transform;

    //    // 最初の惑星を決定
    //    currentPlanet = ChoosePlanet();
    //}

    //void Update()
    //{        
    //    // 重力と Player の回転の制御を行う
    //    Attract();
    //    // Attract 関数内で使用する Normalvec に Ray が接触したポリゴンの法線を格納する役割
    //    RayTest();
    //}

    //// 重力と Player の回転の制御の処理
    //public void Attract()
    //{
    //    // 重力の逆ベクトルを gravityUp として定義する
    //    Vector3 gravityUp = normalVec;

    //    // Player の上方向を bodyUp として定義する
    //    Vector3 bodyUp = myTransform.up;

    //    // gravityUp に Gravity をかけて Player の Rigidbody を参照し力を加える
    //    // これにより、Player は惑星の表面に引き寄せられる
    //    // Gravity は負の数で指定する必要がある
    //    myTransform.GetComponent<Rigidbody>().AddForce(gravityUp * gravity);

    //    // 自身がどれだけ回転しているかの情報を myTransform.rotation で表し加算する
    //    // 現在の姿勢からどれだけ回転させるかを計算し targetRotation に格納している
    //    Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * myTransform.rotation;

    //    // Quaternion.Lerp を使用して第一引数と第二引数の間の角度を第三引数でしていた秒数で線形補完しながら回転させる
    //    // 線形補間は直線を意識した回転をする 球面補完は球面に沿うような回転をする
    //    // lerpRotationSpeed は回転の速さを表す数値で、これを大きくすると回転が速くなり、小さくすると回転が遅くなる
    //    myTransform.rotation = Quaternion.Lerp(myTransform.rotation, targetRotation, lerpRotationSpeed * Time.deltaTime);        
    //}

    //// Planet タグがついているオブジェクトの中で最も近いオブジェクトを返す関数
    //GameObject ChoosePlanet()
    //{
    //    // Planet タグがついているオブジェクトをすべて取得して
    //    planets = GameObject.FindGameObjectsWithTag("Planet");

    //    // Planet タグがついているオブジェクトの数だけの要素数を持つ Planetdistance 配列を定義する
    //    double[] planetDistance = new double[planets.Length];

    //    // Player と Planet タグがついているオブジェクトの距離を計算して Planetdistance 配列に格納する
    //    for (int i = 0; i < planets.Length; i++)
    //    {
    //        // Vector3.Distance 関数は第一引数と第二引数の距離を返す関数
    //        planetDistance[i] = Vector3.Distance(this.transform.position, planets[i].transform.position);
    //    }

    //    // Planetdistance 配列の中で最も小さい値を見つけて、そのインデックスを minindex に格納する
    //    int minIndex = 0;
    //    double minDistance = Mathf.Infinity;

    //    // Planetdistance 配列の中で最も小さい値を見つけて、そのインデックスを minndex に格納する
    //    for (int j = 0; j < planets.Length; j++)
    //    {
    //        if (planetDistance[j] < minDistance)
    //        {
    //            // Planetdistance[j] が mindistance より小さい場合、mindistance を Planetdistance[j] に更新し、minindex を j に更新する
    //            minDistance = planetDistance[j];
    //            minIndex = j;
    //        }
    //    }

    //    // 最も近い Planet タグがついているオブジェクトを返す
    //    return planets[minIndex];
    //}

    //// Planet タグがついているオブジェクトの中で最も近いオブジェクトと二番目に近いオブジェクトを返す関数
    //GameObject ChooseSecondPlanet()
    //{
    //    // Planet タグがついているオブジェクトをすべて取得
    //    planets = GameObject.FindGameObjectsWithTag("Planet");

    //    // 最も近いオブジェクトと二番目に近いオブジェクトを格納する変数を定義
    //    GameObject nearest = null;
    //    GameObject second = null;

    //    // 最も近いオブジェクトと二番目に近いオブジェクトを見つけるための距離を格納する変数を定義
    //    float minDist = Mathf.Infinity;
    //    float secondDist = Mathf.Infinity;

    //    // Planet タグがついているオブジェクトの中で最も近いオブジェクトと二番目に近いオブジェクトを見つける
    //    foreach (GameObject p in planets)
    //    {
    //        // Vector3.Distance 関数は第一引数と第二引数の距離を返す関数
    //        // これにより、Player と Planet タグがついているオブジェクトの距離を計算することができる
    //        float dist = Vector3.Distance(transform.position, p.transform.position);

    //        if (dist < minDist)
    //        {
    //            // dist が minDist より小さい場合、secondDist を minDist に更新し、second を nearest に更新する
    //            secondDist = minDist;
    //            second = nearest;

    //            // minDist を dist に更新し、nearest を p に更新する
    //            minDist = dist;
    //            nearest = p;
    //        }
    //        // dist が minDist より小さい場合、secondDist を minDist に更新し、second を nearest に更新する
    //        else if (dist < secondDist)
    //        {
    //            secondDist = dist;
    //            second = p;
    //        }
    //    }

    //    // 最も近いオブジェクトと二番目に近いオブジェクトを返す
    //    return second;
    //}

    //// Ray が接触したポリゴンの法線を格納する役割
    //void RayTest()
    //{
    //    if (currentPlanet == null) return;

    //    // Direction に Player から見た惑星中心のベクトルを代入しています
    //    direction = currentPlanet.transform.position - transform.position;

    //    // Ray の発射地点は Player の座標で Ray を飛ばす方向は Direction の方向
    //    Ray ray = new Ray(transform.position, direction);
    //    // Rayが当たったオブジェクトの情報を入れる
    //    RaycastHit hit;

    //    // もしRayにオブジェクトが衝突した場合
    //    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
    //    {
    //        // Ray が当たったオブジェクトの tag が Planet だったら
    //        if (hit.collider.CompareTag("Planet"))
    //        {
    //            // Ray が接触した惑星のポリゴンの法線を Normalvec に代入する
    //            normalVec = hit.normal;
    //        }
    //    }
    //}

    //// 重力を反転させる関数
    //public void ReverseGravity()
    //{
    //    Debug.Log("gravity Reversed!");

    //    // 現在の惑星から二番目に近い惑星を選ぶ
    //    GameObject nextPlanet = ChooseSecondPlanet();

    //    // 二番目に近い惑星が存在する場合、currentPlanet を nextPlanet に更新する
    //    if (nextPlanet != null)
    //    {
    //        currentPlanet = nextPlanet;

    //        // 軽くジャンプを加える
    //        Vector3 jumpDir = (currentPlanet.transform.position - transform.position).normalized;
    //        // jumpDir に planetJumpForce 分の力をかけて Player をジャンプさせる
    //        rb.AddForce(jumpDir * planetJumpForce, ForceMode.Impulse);
    //    }
    //}
}
