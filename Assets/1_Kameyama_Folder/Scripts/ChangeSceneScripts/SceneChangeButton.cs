using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeButton : MonoBehaviour
{
    [SerializeField]
    private SceneObject sceneObject;

    public void OnClick()
    {
        SceneManager.LoadScene(sceneObject);
    }
}
