using UnityEngine;

public class VisibilityChecker : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject target; // Head
    public Vector2 offset = new Vector2(0.30f, 0.30f); // Offset as fraction of screen (30% margin)
    public Animator animator;
    private bool showingHead = false;

    public LevelManager levelManager;
    public AudioManager audioManager;
    public AudioClip scary1;

    private void Start()
    {
        target.SetActive(false);
    }

    public bool IsOnScreen()
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(target.transform.position);

        // Viewport coordinates: (0,0) bottom-left, (1,1) top-right
        bool onScreenX = viewportPos.x > 0f + offset.x && viewportPos.x < 1f - offset.x;
        bool onScreenY = viewportPos.y > 0f + offset.y && viewportPos.y < 1f - offset.y;
        bool inFrontOfCamera = viewportPos.z > 0f; // z > 0 means in front of camera

        return onScreenX && onScreenY && inFrontOfCamera;
    }

    void Update()
    {
        if (showingHead && IsOnScreen())
        {
            animator.SetTrigger("SpottedHead");
            showingHead = false;
            audioManager.PlaySFX(scary1);
            levelManager.HideHead();
        }
    }

    public void ResetHead()
    {
        showingHead = false;
        target.SetActive(false);
        animator.ResetTrigger("SpottedHead");
        animator.Play("HeadOut");
    }

    public void ShowHead()
    {
        showingHead = true;
        target.SetActive(true);
    }
}
