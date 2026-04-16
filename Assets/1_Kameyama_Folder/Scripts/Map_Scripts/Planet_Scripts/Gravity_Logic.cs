using UnityEngine;

public class Gravity_Logic : MonoBehaviour
{
    //PlayerのTransform
    private Transform myTransform;

    //PlayerのRigidbody
    private Rigidbody rig = null;

    //重力減となる惑星
    private GameObject Planet;

    //「Planet」タグがついているオブジェクトを格納する配列
    private GameObject[] Planets;

    //重力の強さ
    public float Gravity;

    //惑星に対するPlayerの向き
    private Vector3 Direction;

    //Rayが接触した惑星のポリゴンの法線
    private Vector3 Normal_vec = new Vector3(0, 0, 0);

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
                min_distance = Planet_distance[j];
                min_index = j;
            }
        }

        // 最も近い Planet タグがついているオブジェクトを返す
        return Planets[min_index];
    }

    // Ray が接触したポリゴンの法線を格納する役割
    void RayTest()
    {
        // Choose_Planet 関数の戻り値を Planet に代入している
        // Choose_Planet 関数は Planet タグがついているオブジェクトの中で最も近いオブジェクトを返す関数
        Planet = Choose_Planet();

        // Direction に Player から見た惑星中心のベクトルを代入しています
        Direction = Planet.transform.position - this.transform.position;

        // Ray の発射地点は Player の座標で Ray を飛ばす方向は Direction の方向
        Ray ray = new Ray(this.transform.position, Direction);

        //Rayが当たったオブジェクトの情報を入れる箱
        RaycastHit hit;

        //もしRayにオブジェクトが衝突したら
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            //Rayが当たったオブジェクトのtagがPlanetだったら
            if (hit.collider.tag == "Planet")
            {
                // Ray が接触した惑星のポリゴンの法線を Normal_vec に代入する
                // これにより Player は惑星の表面に沿って Player が回転するようになる
                Normal_vec = hit.normal;
            }
        }
    }
}
