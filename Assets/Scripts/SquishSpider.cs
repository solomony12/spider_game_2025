using UnityEngine;

public class SquishSpider : MonoBehaviour
{
    [Header("Spider Manager Reference")]
    public SpiderManager spiderManager;

    [Header("Camera Reference")]
    public Camera mainCamera;
    public GameObject playerParent;

    public Animator sporkAnimator;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            sporkAnimator.SetTrigger("HitSpider");

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                GameObject clickedObject = hit.collider.gameObject;

                // Check if object is a spider clone
                foreach (var baseSpider in spiderManager.baseSpiders)
                {
                    var clones = spiderManager.GetClones(baseSpider);
                    if (clones == null) continue;

                    foreach (var kvp in clones)
                    {
                        if (kvp.Value == clickedObject)
                        {
                            // Check distance to player
                            float distance = Vector3.Distance(playerParent.transform.position, clickedObject.transform.position);
                            float maxClickDistance = 3f;

                            if (distance <= maxClickDistance)
                            {
                                Destroy(clickedObject);
                                clones.Remove(kvp.Key);
                                Debug.Log($"Squished spider clone ID {kvp.Key} of {baseSpider.name}");
                            }
                            else
                            {
                                Debug.Log("Too far to squish this spider.");
                            }

                            return;
                        }
                    }
                }
            }
        }

    }
}
