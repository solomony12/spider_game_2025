using System;
using System.Collections.Generic;
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

    public SporkBloodiness sporkBloodiness;
    private int spidersKilled;

    public LevelManager levelManager;

    private List<GameObject> crushedSpiders = new List<GameObject>();
    private List<GameObject> splatterList = new List<GameObject>();

    private void Start()
    {
        spidersKilled = 0;
    }

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
                        if (kvp.Value == clickedObject && levelManager.PlaySporkSounds())
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

                                crushedSpiders.Add(crushedSpider);
                                splatterList.Add(splatter);

                                // Remove the original spider clone
                                Destroy(clickedObject);
                                clones.Remove(kvp.Key);
                                spiderManager.RemoveSpiderCloneFromList(kvp.Value);

                                int index = UnityEngine.Random.Range(0, squishSounds.Length);
                                AudioClip clip = squishSounds[index];
                                audioManager.PlaySFX(clip, 0.4f);
                                spidersKilled++;
                                levelManager.SpiderTaskCounter();
                                CheckBloodLevels();

                                OnSpiderSquished?.Invoke();

                                //Debug.Log($"Squished spider clone ID {kvp.Key} of {baseSpider.name}");
                            }
                            else
                            {
                                audioManager.PlaySFX(missSound);
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

    private void CheckBloodLevels()
    {
        if (spidersKilled < 25)
        {
            sporkBloodiness.SelectMaterial(0);
        }
        else if (spidersKilled >= 25 && spidersKilled < 50)
        {
            sporkBloodiness.SelectMaterial(1);
        }
        else if (spidersKilled >= 50 && spidersKilled < 100)
        {
            sporkBloodiness.SelectMaterial(2);
        }
        else // over 100
        {
            sporkBloodiness.SelectMaterial(3);
        }
    }

    public void ResetSporkMaterial()
    {
        sporkBloodiness.SelectMaterial(0);
    }

    public int getSpidersKilled()
    {
        return spidersKilled;
    }

    public void ClearCrushedSpiders()
    {
        foreach (GameObject spider in crushedSpiders)
        {
            Destroy(spider);
        }
        crushedSpiders.Clear();

        foreach (GameObject splatter in splatterList)
        {
            Destroy(splatter);
        }
        splatterList.Clear();
    }
}
