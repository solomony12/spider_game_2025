using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    private bool canPlay = true;

    public GameObject creditsPage;
    public Button hintsButton;

    public static MenuScript Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ResetStuff()
    {
        hintsButton.interactable = false;
        HideCredits();
    }

    void OnEnable()
    {
        canPlay = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void PlayGame()
    {
        if (canPlay)
        {
            HideCredits();
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
        if (creditsPage != null)
        {
            creditsPage.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // Check if HintsScene is loaded
        bool hintsLoaded = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == "HintsScene")
            {
                hintsLoaded = true;
                break;
            }
        }

        // Only run if TitleScene is active and HintsScene is NOT loaded
        if (sceneName == "TitleScene" && !hintsLoaded)
        {
            Button playButton = GameObject.Find("Play").GetComponent<Button>();
            playButton.onClick.AddListener(PlayGame);
            canPlay = true;

            creditsPage = GameObject.Find("CreditsPage");
            creditsPage.SetActive(true);

            Button closeButton = GameObject.Find("Close").GetComponent<Button>();
            closeButton.onClick.AddListener(HideCredits);

            hintsButton = GameObject.Find("Hints").GetComponent<Button>();
            hintsButton.onClick.AddListener(ShowHints);

            Button creditsButton = GameObject.Find("Credits").GetComponent<Button>();
            creditsButton.onClick.AddListener(ShowCredits);

            Button quitButton = GameObject.Find("Quit").GetComponent<Button>();
            quitButton.onClick.AddListener(QuitGame);

            ResetStuff(); // must go before EnableHints
            EnableHints();
        }
    }


    private void EnableHints()
    {
        if (LevelManager.checkIfHintsCanBeShown())
        {
            hintsButton.interactable = true;
        }
    }

    public void ShowHints()
    {
        SceneManager.LoadSceneAsync("HintsScene", LoadSceneMode.Additive);
    }
}
