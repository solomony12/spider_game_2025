using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{

    private bool notStarting = true;

    private void Start()
    {
        notStarting = true;
    }
    public void PlayGame()
    {
        if (notStarting)
        {
            notStarting = false;
            FadeController.Instance.FadeToScene("SampleScene");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
