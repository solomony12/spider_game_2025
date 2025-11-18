using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    private bool canPlay = true;

    private void OnEnable()
    {
        canPlay = true;
    }

    public void PlayGame()
    {
        if (canPlay)
        {
            FadeController.Instance.ResetTriggers();
            canPlay = false;
            FadeController.Instance.FadeToScene("SampleScene");
            Debug.Log("PlayGame triggered.");
        }
        else
        {
            Debug.Log("PlayGame already triggered. Ignoring.");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
