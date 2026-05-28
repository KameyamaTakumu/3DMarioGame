using UnityEngine;

public class TitleMANAGER : MonoBehaviour
{
    void Start()
    {
        SoundManager.Instance.PlayBGM(BGM.Title);
    }
}
