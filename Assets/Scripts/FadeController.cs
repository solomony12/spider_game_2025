using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeController : MonoBehaviour
{
    public Animator animator;
    private string nextScene;

    public static FadeController Instance;

    private bool isEnding = false;

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

    public void FadeToScene(string sceneName)
    {
        Debug.Log("hello there");
        nextScene = sceneName;
        animator.SetTrigger("FadeIn");
    }

    public void OnFadeComplete()
    {
        if (isEnding)
        {
            return;
        }

        SceneManager.LoadScene(nextScene);
        animator.SetTrigger("FadeOut");
    }

    public void FadeToBlack()
    {
        isEnding = true;
        animator.SetTrigger("FadeIn");
    }

    public void ResetEndingBool()
    {
        isEnding = false;
    }

    public void ResetTriggers()
    {
        animator.Play("Clear");
        animator.ResetTrigger("FadeIn");
        animator.SetTrigger("FadeOut");
    }
}
