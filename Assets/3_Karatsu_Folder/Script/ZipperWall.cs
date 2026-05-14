using UnityEngine;
using System.Collections;

public class ZipperWall : MonoBehaviour
{
    public enum ZipperWallType
    {
        StayAsFloor,    //そのまま床として存在する
        FadeOut         //フェードアウトして消える
    }

    [Header("設定")]
    public ZipperWallType zipperWallType = ZipperWallType.StayAsFloor;    //ジッパーウォールのタイプ
    public float openSpeed = 1.0f;    //開く速度

    [Header("参照")]
    public GameObject wallMesh;    //壁のメッシュオブジェクト

    private bool isOpened = false;    //開いているかどうか

    void Update()
    {
        // テスト用：スペースキーを押したらチャックが動いたことにする
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OpenZipperWall();
        }
    }


    //チャックが終点に来たときに呼び出す関数
    public void OpenZipperWall()
    {
        if(isOpened) return;    //すでに開いている場合は何もしない

        isOpened = true;    //開いた状態にする

        StartCoroutine(AnimateWallOpening());    //ジッパーウォールを開くコルーチンを開始する
    }

    //ジッパーウォールを開くコルーチン
    IEnumerator AnimateWallOpening()
    {
        // RotatingPivotを90度倒す
        Quaternion startRot = transform.rotation;
        Quaternion endRot = transform.rotation * Quaternion.Euler(-90, 0, 0);

        float time = 0;
        while (time < 1.0f)
        {
            time += Time.deltaTime * openSpeed;
            transform.rotation = Quaternion.Slerp(startRot, endRot, time);
            yield return null;
        }

        // 倒れ終わった後に、タイプ別の処理を行う
        HandlePostOpenProcess();
    }

    //ジッパーウォールが開いた後の処理
    void HandlePostOpenProcess()
    {
        switch (zipperWallType)
        {
            case ZipperWallType.StayAsFloor:
                // 何もしない（そのまま床として存在する）
                break;

            case ZipperWallType.FadeOut:
                StartCoroutine(FadeOutAndDestroy());
                break;
        }
    }

    //フェードアウトして消えるコルーチン
    IEnumerator FadeOutAndDestroy()
    {
        Renderer wallRenderer = wallMesh.GetComponent<Renderer>();
        Color startColor = wallRenderer.material.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0); // 透明な色

        float time = 0;
        while (time < 1.0f)
        {
            time += Time.deltaTime * openSpeed;
            wallRenderer.material.color = Color.Lerp(startColor, endColor, time);
            yield return null;
        }

        Destroy(gameObject); // ジッパーウォールオブジェクトを破壊する
    }

}
