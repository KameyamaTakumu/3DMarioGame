using UnityEngine;

public class Stage1MANAGER : MonoBehaviour
{
    private void Start()
    {
        SoundManager.Instance.PlayBGM(BGM.Stage1);
    }
}

