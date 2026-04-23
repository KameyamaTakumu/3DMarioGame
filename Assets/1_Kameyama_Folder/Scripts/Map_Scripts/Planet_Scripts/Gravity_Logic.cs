using UnityEngine;

// プレイヤーが惑星の表面に引き寄せられるような重力と、惑星の表面に沿って回転するような処理を行うクラス
public class Gravity_Logic : MonoBehaviour
{
    // Player の Transform
    private Transform myTransform;

    // Player の Rigidbody
    private Rigidbody rig = null;

    //重力減となる惑星
    private GameObject Planet;

    // Planet タグがついているオブジェクトを格納する配列
    private GameObject[] Planets;

    // 重力の強さ
    public float Gravity;

    // 惑星に対する Player の向き
    private Vector3 Direction;

    // Ray が接触した惑星のポリゴンの法線
    private Vector3 Normal_vec = new Vector3(0, 0, 0);

    // 現在の惑星
    private GameObject currentPlanet;

    void Start()
    {
        // Player の Rigidbody を取得して rig に代入する
        rig = this.GetComponent<Rigidbody>();

        // すでに設定済みだが念のためにこちらでも設定する
        // Player の Rigidbody の回転を固定する
        rig.constraints = RigidbodyConstraints.FreezeRotation;
        // Player の Rigidbody の重力を無効にする
        rig.useGravity = false;

        // このオブジェクトの Transform を取得して myTransform に代入する
        myTransform = transform;

        // 最初の惑星を決定
        currentPlanet = Choose_Planet();
    }

    void Update()
    {        
        // 重力と Player の回転の制御を行う
        Attract();
        // Attract 関数内で使用する Normal_vec に Ray が接触したポリゴンの法線を格納する役割
        RayTest();
    }

    // 重力と Player の回転の制御の処理
    public void Attract()
    {
        // 重力の逆ベクトルを gravityUp として定義する
        Vector3 gravityUp = Normal_vec;

        // Player の上方向を bodyUp として定義する
        Vector3 bodyUp = myTransform.up;

        // gravityUp に Gravity をかけて Player の Rigidbody を参照し力を加える
        // これにより、Player は惑星の表面に引き寄せられる
        // Gravity は負の数で指定する必要がある
        myTransform.GetComponent<Rigidbody>().AddForce(gravityUp * Gravity);

        // 自身がどれだけ回転しているかの情報を myTransform.rotation で表し加算する
        // 現在の姿勢からどれだけ回転させるかを計算し targetRotation に格納している
        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * myTransform.rotation;

        // Quaternion.Lerp を使用して第一引数と第二引数の間の角度を第三引数でしていた秒数で線形補完しながら回転させる
        // 線形補間は直線を意識した回転をする 球面補完は球面に沿うような回転をする
        // 120 は回転の速さを表す数値で、これを大きくすると回転が速くなり、小さくすると回転が遅くなる
        myTransform.rotation = Quaternion.Lerp(myTransform.rotation, targetRotation, 120 * Time.deltaTime);        
    }

    // Planet タグがついているオブジェクトの中で最も近いオブジェクトを返す関数
    GameObject Choose_Planet()
    {
        // Planet タグがついているオブジェクトをすべて取得して
        Planets = GameObject.FindGameObjectsWithTag("Planet");

        // Planet タグがついているオブジェクトの数だけの要素数を持つ Planet_distance 配列を定義する
        double[] Planet_distance = new double[Planets.Length];

        // Player と Planet タグがついているオブジェクトの距離を計算して Planet_distance 配列に格納する
        for (int i = 0; i < Planets.Length; i++)
        {
            // Vector3.Distance 関数は第一引数と第二引数の距離を返す関数
            Planet_distance[i] = Vector3.Distance(this.transform.position, Planets[i].transform.position);
        }

        // Planet_distance 配列の中で最も小さい値を見つけて、そのインデックスを min_index に格納する
        int min_index = 0;
        double min_distance = Mathf.Infinity;

        // Planet_distance 配列の中で最も小さい値を見つけて、そのインデックスを min_index に格納する
        for (int j = 0; j < Planets.Length; j++)
        {
            if (Planet_distance[j] < min_distance)
            {
                // Planet_distance[j] が min_distance より小さい場合、min_distance を Planet_distance[j] に更新し、min_index を j に更新する
                min_distance = Planet_distance[j];
                min_index = j;
            }
        }

        // 最も近い Planet タグがついているオブジェクトを返す
        return Planets[min_index];
    }

    // Planet タグがついているオブジェクトの中で最も近いオブジェクトと二番目に近いオブジェクトを返す関数
    GameObject Choose_Second_Planet()
    {
        // Planet タグがついているオブジェクトをすべて取得
        Planets = GameObject.FindGameObjectsWithTag("Planet");

        // 最も近いオブジェクトと二番目に近いオブジェクトを格納する変数を定義
        GameObject nearest = null;
        GameObject second = null;

        // 最も近いオブジェクトと二番目に近いオブジェクトを見つけるための距離を格納する変数を定義
        float minDist = Mathf.Infinity;
        float secondDist = Mathf.Infinity;

        // Planet タグがついているオブジェクトの中で最も近いオブジェクトと二番目に近いオブジェクトを見つける
        foreach (GameObject p in Planets)
        {
            // Vector3.Distance 関数は第一引数と第二引数の距離を返す関数
            // これにより、Player と Planet タグがついているオブジェクトの距離を計算することができる
            float dist = Vector3.Distance(transform.position, p.transform.position);

            if (dist < minDist)
            {
                // dist が minDist より小さい場合、secondDist を minDist に更新し、second を nearest に更新する
                secondDist = minDist;
                second = nearest;

                // minDist を dist に更新し、nearest を p に更新する
                minDist = dist;
                nearest = p;
            }
            // dist が minDist より小さい場合、secondDist を minDist に更新し、second を nearest に更新する
            else if (dist < secondDist)
            {
                secondDist = dist;
                second = p;
            }
        }

        // 最も近いオブジェクトと二番目に近いオブジェクトを返す
        return second;
    }

    // Ray が接触したポリゴンの法線を格納する役割
    void RayTest()
    {
        if (currentPlanet == null) return;

        // Direction に Player から見た惑星中心のベクトルを代入しています
        Direction = currentPlanet.transform.position - transform.position;

        // Ray の発射地点は Player の座標で Ray を飛ばす方向は Direction の方向
        Ray ray = new Ray(transform.position, Direction);
        // Rayが当たったオブジェクトの情報を入れる
        RaycastHit hit;

        // もしRayにオブジェクトが衝突した場合
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            // Ray が当たったオブジェクトの tag が Planet だったら
            if (hit.collider.CompareTag("Planet"))
            {
                // Ray が接触した惑星のポリゴンの法線を Normal_vec に代入する
                Normal_vec = hit.normal;
            }
        }
    }

    // 重力を反転させる関数
    public void ReverseGravity()
    {
        // 現在の惑星から二番目に近い惑星を選ぶ
        GameObject nextPlanet = Choose_Second_Planet();

        // 二番目に近い惑星が存在する場合、currentPlanet を nextPlanet に更新する
        if (nextPlanet != null)
        {
            currentPlanet = nextPlanet;

            // 軽くジャンプを加える
            Vector3 jumpDir = (currentPlanet.transform.position - transform.position).normalized;
            // jumpDir に 5f の力をかけて Player をジャンプさせる
            rig.AddForce(jumpDir * 5f, ForceMode.Impulse);
        }
    }
}
