using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HintsLooper : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text phraseText;
    public Button leftButton;
    public Button rightButton;
    public Button closeButton;

    private int index = 0;

    private readonly string[] phrases = new string[]
    {
        "Where did you go?",
        "Slack off just a little bit.",
        "Do as you're told.",
        "You really like the ball.",
        "The toilet is gross.",
        "You're safe under the sheets.",
        "The food is disgusting.",
        "You really hate spiders.",
        "Archnophobia compels you."
    };

    void Start()
    {
        UpdatePhrase();

        leftButton.onClick.AddListener(PreviousPhrase);
        rightButton.onClick.AddListener(NextPhrase);
        closeButton.onClick.AddListener(CloseScene);
    }

    private void UpdatePhrase()
    {
        phraseText.text = phrases[index];
    }

    public void NextPhrase()
    {
        index = (index + 1) % phrases.Length;
        UpdatePhrase();
    }

    public void PreviousPhrase()
    {
        index = (index - 1 + phrases.Length) % phrases.Length;
        UpdatePhrase();
    }

    private void CloseScene()
    {
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}
