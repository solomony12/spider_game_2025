using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    private bool canPlay = true;

    public GameObject creditsPage;

    private void Start()
    {
        creditsPage.SetActive(false);
    }

    private void OnEnable()
    {
        canPlay = true;
    }

    public void PlayGame()
    {
        if (canPlay)
        {
            creditsPage.SetActive(false);
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

    public void ShowCredits()
    {
        creditsPage.SetActive(true);
    }

    public void HideCredits()
    {
        creditsPage.SetActive(false);
    }
}
