using UnityEngine;
using UnityEngine.InputSystem;

public class CaptureManager : MonoBehaviour
{
    public CameraController cameraController;

    [Header("プレイヤー")]
    public GameObject player;

    private GameObject currentEnemy;

    private bool captured;

    void Update()
    {
        // Tで解除
        if (captured && Keyboard.current.tKey.wasPressedThisFrame)
        {
            ReleaseCapture();
        }
    }

    // キャプチャー開始
    public void Capture(GameObject enemy)
    {
        captured = true;

        currentEnemy = enemy;

        // Player非表示
        player.SetActive(false);

        // カメラ変更
        cameraController.SetTarget(enemy.transform);

        Debug.Log("キャプチャー開始");
    }

    // キャプチャー解除
    void ReleaseCapture()
    {
        captured = false;

        // プレイヤーを敵位置へ移動
        player.transform.position =
            currentEnemy.transform.position;

        // Player再表示
        player.SetActive(true);

        // カメラ戻す
        cameraController.SetTarget(player.transform);

        PlayerController.instance.captureTrigger = false;

        Debug.Log("キャプチャー解除");
    }
}