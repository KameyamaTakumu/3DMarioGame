using UnityEngine;
using System;
using Unity.VisualScripting;

public class Boomerang : MonoBehaviour
{
    [Header("移動")]
    public float forwardSpeed = 20f;
    public float returnSpeed = 25f;

    [Header("滞空")]
    public float maxDistance = 10f;

    [Header("回転")]
    public float rotateSpeed = 1000f;

    [Header("カーブ")]
    public float curveAmount = 2f;

    public Transform owner;

    public Action onReturn;

    private Vector3 startPos;
    private Vector3 forwardDir;

    private bool returning;
    private bool stuck;

    void Start()
    {
        startPos = transform.position;

        // 投げた瞬間の方向を固定
        forwardDir = transform.forward;
    }

    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);

        if (stuck) return;

        if (!returning)
        {
            ForwardMove();

            float dist =
                Vector3.Distance(startPos, transform.position);

            if (dist >= maxDistance)
            {
                returning = true;
            }
        }
        else
        {
            ReturnMove();
        }
    }

    void ForwardMove()
    {
        // 前進
        Vector3 move = forwardDir * forwardSpeed;

        // 横カーブ
        move += transform.right *
                Mathf.Sin(Time.time * 15f) *
                curveAmount;

        transform.position += move * Time.deltaTime;
    }

    void ReturnMove()
    {
        if (owner == null) return;

        Vector3 target =
            owner.position + Vector3.up * 1.5f;

        Vector3 dir =
            (target - transform.position).normalized;

        transform.position +=
            dir * returnSpeed * Time.deltaTime;

        transform.rotation =
            Quaternion.LookRotation(dir);

        // プレイヤーへ戻る
        if (Vector3.Distance(transform.position, target) < 1f)
        {
            onReturn?.Invoke();

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (stuck) return;

        // 敵に当たった
        //if (other.CompareTag("Enemy"))
        //{
        //    stuck = true;

        //    transform.SetParent(other.transform);

        //    // キャプチャーマネージャ取得
        //    CaptureManager capture =
        //        FindObjectOfType<CaptureManager>();

        //    GoombaController gc = other.GetComponent<GoombaController>();

        //    if (capture != null)
        //    {
        //        capture.Capture(other.transform);
        //    }

        //    if (gc != null)
        //    {
        //        // キャプチャを実行する
        //        gc.OnCaptured();

        //        // Player にキャプチャを報告する
        //        Test_PlayerController.instance.captureTrigger = true;

        //        // ブーメランを削除
        //        Destroy(gameObject);

        //        Debug.Log("キャプチャしました");
        //    }

        //    Debug.Log("キャプチャー成功！");
        //}
        if (other.CompareTag("Enemy"))
        {
            stuck = true;

            // 敵に刺さる
            transform.SetParent(other.transform);

            Debug.Log("敵ヒット！");

            // キャプチャーマネージャ取得
            CaptureManager capture =
                FindObjectOfType<CaptureManager>();

            if (capture != null)
            {
                //capture.Capture(other.transform);
                capture.Capture(other.gameObject);
            }

            // 敵にControllerが付いているかを確認する
            if (other.GetComponent<GoombaController>() != null)
            {
                // Controllerコンポーネントを取得する
                GoombaController gc = other.GetComponent<GoombaController>();

                Debug.Log("コントローラー取得");

                if (gc != null)
                {
                    // キャプチャを実行する
                    gc.OnCaptured();

                    // Player にキャプチャを報告する
                    PlayerController.instance.captureTrigger = true;

                    // ブーメランを削除
                    Destroy(gameObject);

                    Debug.Log("キャプチャしました");
                }
            }
            else
            {
                Debug.Log("コントローラーが見つかりませんでした");
            }

            // ブーメランを戻す(再度投げられるようにする)
            returning = true;
            onReturn?.Invoke();
        }
    }
}