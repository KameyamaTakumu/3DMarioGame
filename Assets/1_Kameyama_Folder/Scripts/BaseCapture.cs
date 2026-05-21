using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// キャプチャの基底クラス 
/// 接触したオブジェクトがこのBaseCaputureの派生クラスであればそのオブジェクトにキャプチャする
/// </summary>
public class BaseCapture : MonoBehaviour
{
    public virtual void GiveCaptureComponent(){}
}
