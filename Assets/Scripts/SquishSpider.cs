using System;
using UnityEngine;
using UnityEngine.Audio;

public class SquishSpider : MonoBehaviour
{
    [Header("Spider Manager Reference")]
    public SpiderManager spiderManager;

    [Header("Camera Reference")]
    public Camera mainCamera;
    public GameObject playerParent;

    public Animator sporkAnimator;

    public GameObject crushedSpiderBase;
    public GameObject splatterBase;

    public static event Action OnSpiderSquished;

    public AudioManager audioManager;
    public AudioClip[] squishSounds;
    public AudioClip missSound;

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
                            float maxClickDistance = 5f;

                            if (distance <= maxClickDistance)
                            {
                                // Spawn crushed spider at the same position and rotation
                                GameObject crushedSpider = Instantiate(
                                    crushedSpiderBase,
                                    clickedObject.transform.position,
                                    clickedObject.transform.rotation
                                );
                                Quaternion splatterRotation = clickedObject.transform.rotation * Quaternion.Euler(90f, 0f, 0f);
                                GameObject splatter = Instantiate(
                                    splatterBase,
                                    clickedObject.transform.position,
                                    splatterRotation
                                );

                                // Remove the original spider clone
                                Destroy(clickedObject);
                                clones.Remove(kvp.Key);

                                int index = UnityEngine.Random.Range(0, squishSounds.Length);
                                AudioClip clip = squishSounds[index];
                                audioManager.PlaySFX(clip, 0.75f);

                                OnSpiderSquished?.Invoke();

                                Debug.Log($"Squished spider clone ID {kvp.Key} of {baseSpider.name}");
                            }


                            return;
                        }
                    }
                }
            }
            else
            {
                audioManager.PlaySFX(missSound);
            }
        }
    }
}
