using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public void PlayGame()
    {
        FadeController.Instance.FadeToScene("SampleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
