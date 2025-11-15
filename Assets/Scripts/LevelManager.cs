using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yarn;
using Yarn.Unity;
using static Unity.Collections.Unicode;

public class LevelManager : MonoBehaviour
{
    [Header("Spider Manager Reference")]
    public SpiderManager spiderManager;

    [Header("Spawn Settings")]
    public Vector3 startPosition = Vector3.zero;

    public Yarn.Unity.DialogueRunner dialogueRunner;
    public GameObject characterObject;
    public string characterArtPath = "Art/Characters/";

    public GameObject doorSlider;
    public GameObject bed;
    public GameObject food;
    public GameObject toilet;
    private float maxClickDistance = 3f;

    public Animator dsAnimator;
    public Animator foodAnimator;

    public Camera mainCamera;
    public GameObject playerParent;

    private int day = 0;

    void Awake()
    {
        dialogueRunner.onDialogueComplete.AddListener(OnDialogueFinished);
    }

    private void Start()
    {
        characterObject.SetActive(false);

        StartDialogue(); // TODO: temp
    }

    private void StartDialogue()
    {
        dialogueRunner.StartDialogue("MainStory");
        characterObject.SetActive(true);
    }

    void Update()
    {
        /*
        // If door slider is clicked on, start next dialogue (they also have needed to finish their daily tasks)
        if ()
        {
            // TODO: Select dialogue from story (just the next yarn node)
            dialogueRunner.StartDialogue("Start");
        }
        */
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                float dist;

                switch (hit.collider.tag)
                {
                    case "DoorSlider":
                        dist = Vector3.Distance(playerParent.transform.position, doorSlider.transform.position);
                        if (dist <= maxClickDistance)
                        {
                            Debug.Log("door slider clicked");
                        }
                        break;

                    case "Bed":
                        dist = Vector3.Distance(playerParent.transform.position, bed.transform.position);
                        if (dist <= maxClickDistance)
                        {
                            Debug.Log("bed clicked");
                        }
                        break;

                    case "Food":
                        dist = Vector3.Distance(playerParent.transform.position, food.transform.position);
                        if (dist <= maxClickDistance)
                        {
                            Debug.Log("food clicked");
                        }
                        break;

                    case "Toilet":
                        dist = Vector3.Distance(playerParent.transform.position, toilet.transform.position);
                        if (dist <= maxClickDistance)
                        {
                            Debug.Log("toilet clicked");
                        }
                        break;
                }
            }
        }

        // Click anywhere to continue dialogue
        if (dialogueRunner.IsDialogueRunning && Input.GetMouseButtonDown(0))
        {
            dialogueRunner.RequestNextLine();
        }

        // Advance day when P is pressed (temp)
        if (Input.GetKeyDown(KeyCode.P))
        {
            day++;
            Debug.Log($"Day {day} begins!");

            // Spawn one extra clone for each base spider
            foreach (GameObject baseSpider in spiderManager.baseSpiders)
            {
                // Determine how many clones exist already
                var existingClones = spiderManager.GetClones(baseSpider)?.Count ?? 0;

                Vector3 spawnPos = baseSpider.transform.position;

                spiderManager.SpawnClones(baseSpider, 1, spawnPos, Vector3.zero);
            }
        }

        /*
        // Test doors (temp (T Y))
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("open sesame");
            dsAnimator.SetBool("isOpen", true);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("close sesame");
            dsAnimator.SetBool("isOpen", false);
        }
        */
    }

    [YarnCommand("character")]
    public void ChangeCharacterImage(string poseName)
    {
        string path = $"{characterArtPath}{poseName}";
        Sprite newSprite = Resources.Load<Sprite>(path);

        if (newSprite != null)
        {
            characterObject.GetComponent<Image>().sprite = newSprite;
        }
        else
        {
            throw new Exception($"'{poseName}' sprite not found.\nPath tried: {path}");
        }
    }

    void OnDialogueFinished()
    {
        // TODO: Progress to the next day (restart everything like make new clones but keep the squished ones)
        // Clones, daily reset, next yarn?
        Debug.Log("The end");
        characterObject.SetActive(false);
    }

    // Gives food to player via animation
    private IEnumerator giveFood()
    {
        dsAnimator.SetBool("isOpen", true);

        yield return new WaitForSeconds(1.5f);

        foodAnimator.SetBool("isMealTime", true);

    }

    // Takes away food from player via animation
    private IEnumerator foodFinish()
    {
        foodAnimator.SetBool("isMealTime", false);

        yield return new WaitForSeconds(2.5f);

        dsAnimator.SetBool("isOpen", false);
    }

    public int GetCurrentDay()
    {
        return day;
    }
}
