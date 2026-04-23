using UnityEngine;

// プレイヤーが惑星の表面に引き寄せられるような重力と、惑星の表面に沿って回転するような処理を行うクラス
public class GravityLogic : MonoBehaviour
{
    // Player の Transform
    private Transform _myTransform;

    // Player の Rigidbody
    private Rigidbody _rb = null;

    //重力減となる惑星
    private GameObject _planet;

    // Planet タグがついているオブジェクトを格納する配列
    private GameObject[] _planets;

    // 重力の強さ
    public float _gravity;

    // 惑星に対する Player の向き
    private Vector3 _direction;

    // Ray が接触した惑星のポリゴンの法線
    private Vector3 _normalVec = new Vector3(0, 0, 0);

    // 現在の惑星
    private GameObject _currentPlanet;

    // 惑星をジャンプする力
    public float _planetJumpForce = 5f;

    // Player の球体回転の速さ
    private int _lerpRotationSpeed = 120;

    void Start()
    {
        // Player の Rigidbody を取得して rb に代入する
        _rb = this.GetComponent<Rigidbody>();

        // すでに設定済みだが念のためにこちらでも設定する
        // Player の Rigidbody の回転を固定する
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        // Player の Rigidbody の重力を無効にする
        _rb.useGravity = false;

        // このオブジェクトの Transform を取得して myTransform に代入する
        _myTransform = transform;

        // 最初の惑星を決定
        _currentPlanet = Choose_Planet();
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
        Vector3 _gravityUp = _normalVec;

        // Player の上方向を bodyUp として定義する
        Vector3 _bodyUp = _myTransform.up;

        // gravityUp に Gravity をかけて Player の Rigidbody を参照し力を加える
        // これにより、Player は惑星の表面に引き寄せられる
        // Gravity は負の数で指定する必要がある
        _myTransform.GetComponent<Rigidbody>().AddForce(_gravityUp * _gravity);

        // 自身がどれだけ回転しているかの情報を myTransform.rotation で表し加算する
        // 現在の姿勢からどれだけ回転させるかを計算し targetRotation に格納している
        Quaternion _targetRotation = Quaternion.FromToRotation(_bodyUp, _gravityUp) * _myTransform.rotation;

        // Quaternion.Lerp を使用して第一引数と第二引数の間の角度を第三引数でしていた秒数で線形補完しながら回転させる
        // 線形補間は直線を意識した回転をする 球面補完は球面に沿うような回転をする
        // lerpRotationSpeed は回転の速さを表す数値で、これを大きくすると回転が速くなり、小さくすると回転が遅くなる
        _myTransform.rotation = Quaternion.Lerp(_myTransform.rotation, _targetRotation, _lerpRotationSpeed * Time.deltaTime);        
    }

    // Planet タグがついているオブジェクトの中で最も近いオブジェクトを返す関数
    GameObject Choose_Planet()
    {
        // Planet タグがついているオブジェクトをすべて取得して
        _planets = GameObject.FindGameObjectsWithTag("_planet");

        // Planet タグがついているオブジェクトの数だけの要素数を持つ Planet_distance 配列を定義する
        double[] _planetDistance = new double[_planets.Length];

        // Player と Planet タグがついているオブジェクトの距離を計算して Planet_distance 配列に格納する
        for (int i = 0; i < _planets.Length; i++)
        {
            // Vector3.Distance 関数は第一引数と第二引数の距離を返す関数
            _planetDistance[i] = Vector3.Distance(this.transform.position, _planets[i].transform.position);
        }

        // Planet_distance 配列の中で最も小さい値を見つけて、そのインデックスを min_index に格納する
        int _minIndex = 0;
        double _minDistance = Mathf.Infinity;

        // Planet_distance 配列の中で最も小さい値を見つけて、そのインデックスを min_index に格納する
        for (int j = 0; j < _planets.Length; j++)
        {
            if (_planetDistance[j] < _minDistance)
            {
                // Planet_distance[j] が min_distance より小さい場合、min_distance を Planet_distance[j] に更新し、min_index を j に更新する
                _minDistance = _planetDistance[j];
                _minIndex = j;
            }
        }

        // 最も近い Planet タグがついているオブジェクトを返す
        return _planets[_minIndex];
    }

    // Planet タグがついているオブジェクトの中で最も近いオブジェクトと二番目に近いオブジェクトを返す関数
    GameObject Choose_Second_Planet()
    {
        // Planet タグがついているオブジェクトをすべて取得
        _planets = GameObject.FindGameObjectsWithTag("_planet");

        // 最も近いオブジェクトと二番目に近いオブジェクトを格納する変数を定義
        GameObject _nearest = null;
        GameObject _second = null;

        // 最も近いオブジェクトと二番目に近いオブジェクトを見つけるための距離を格納する変数を定義
        float _minDist = Mathf.Infinity;
        float _secondDist = Mathf.Infinity;

        // Planet タグがついているオブジェクトの中で最も近いオブジェクトと二番目に近いオブジェクトを見つける
        foreach (GameObject p in _planets)
        {
            // Vector3.Distance 関数は第一引数と第二引数の距離を返す関数
            // これにより、Player と Planet タグがついているオブジェクトの距離を計算することができる
            float _dist = Vector3.Distance(transform.position, p.transform.position);

            if (_dist < _minDist)
            {
                // dist が minDist より小さい場合、secondDist を minDist に更新し、second を nearest に更新する
                _secondDist = _minDist;
                _second = _nearest;

                // minDist を dist に更新し、nearest を p に更新する
                _minDist = _dist;
                _nearest = p;
            }
            // dist が minDist より小さい場合、secondDist を minDist に更新し、second を nearest に更新する
            else if (_dist < _secondDist)
            {
                _secondDist = _dist;
                _second = p;
            }
        }

        // 最も近いオブジェクトと二番目に近いオブジェクトを返す
        return _second;
    }

    // Ray が接触したポリゴンの法線を格納する役割
    void RayTest()
    {
        if (_currentPlanet == null) return;

        // Direction に Player から見た惑星中心のベクトルを代入しています
        _direction = _currentPlanet.transform.position - transform.position;

        // Ray の発射地点は Player の座標で Ray を飛ばす方向は Direction の方向
        Ray _ray = new Ray(transform.position, _direction);
        // Rayが当たったオブジェクトの情報を入れる
        RaycastHit _hit;

        // もしRayにオブジェクトが衝突した場合
        if (Physics.Raycast(_ray, out _hit, Mathf.Infinity))
        {
            // Ray が当たったオブジェクトの tag が Planet だったら
            if (_hit.collider.CompareTag("_planet"))
            {
                // Ray が接触した惑星のポリゴンの法線を Normal_vec に代入する
                _normalVec = _hit.normal;
            }
        }
    }

    // 重力を反転させる関数
    public void ReverseGravity()
    {
        Debug.Log("_gravity Reversed!");

        // 現在の惑星から二番目に近い惑星を選ぶ
        GameObject _nextPlanet = Choose_Second_Planet();

        // 二番目に近い惑星が存在する場合、currentPlanet を nextPlanet に更新する
        if (_nextPlanet != null)
        {
            _currentPlanet = _nextPlanet;

            // 軽くジャンプを加える
            Vector3 _jumpDir = (_currentPlanet.transform.position - transform.position).normalized;
            // jumpDir に planetJumpForce 分の力をかけて Player をジャンプさせる
            _rb.AddForce(_jumpDir * _planetJumpForce, ForceMode.Impulse);
        }
    }
}
