using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Yarn;
using Yarn.Unity;
using static Unity.Collections.Unicode;

public class LevelManager : MonoBehaviour
{
    [Header("Spider Manager Reference")]
    public SpiderManager spiderManager;
    public GameObject stillSpider;

    [Header("Spawn Settings")]
    public Vector3 startPosition = Vector3.zero;

    public Yarn.Unity.DialogueRunner dialogueRunner;
    public GameObject characterObject;
    public string characterArtPath = "Art/Characters/";

    public GameObject doorSlider;
    public GameObject bed;
    public GameObject food;
    public GameObject toilet;
    public GameObject slop;
    private float maxClickDistance = 3f;

    public Animator dsAnimator;
    public Animator foodAnimator;

    public Camera mainCamera;
    public GameObject playerParent;

    public GameObject smackSpiderText;
    public GameObject tutorialTextObject;
    private TMP_Text tutorialText;
    private string toiletText = "Use the toilet.";
    private string foodText = "Eat the meal.";
    private string talkText = "Talk to the guard.";
    private string bedText = "Go to bed.";
    public GameObject dayText;
    private string currentTutorialText;

    private bool wPressed = false;
    private bool aPressed = false;
    private bool sPressed = false;
    private bool dPressed = false;
    private bool dayOneTutorialFinished = false;

    private bool canUseToilet = false;
    private bool canUseBed = false;
    private bool canUseFood = false;
    private bool canTalkToGuard = false;

    private int day;

    void Awake()
    {
        dialogueRunner.onDialogueComplete.AddListener(OnDialogueFinished);
    }

    private void Start()
    {
        characterObject.SetActive(false);
        day = 1;
        stillSpider.SetActive(true);
        smackSpiderText.SetActive(false);
        tutorialTextObject.SetActive(true); // ALWAYS TRUE
        tutorialText = tutorialTextObject.GetComponent<TMP_Text>();
        tutorialText.text = "WASD to move. | Mouse to look around.";
        currentTutorialText = tutorialText.text;
        LevelManage();
    }

    private void StartDialogue()
    {
        dialogueRunner.StartDialogue("MainStory");
        characterObject.SetActive(true);
    }

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool hovering = false;

        // Hovering over for tutorial
        if (Physics.Raycast(ray, out hit))
        {
            float dist;

            switch (hit.collider.tag)
            {
                case "DoorSlider":
                    dist = Vector3.Distance(playerParent.transform.position, doorSlider.transform.position);
                    //Debug.Log("Door Slider: dist: " + dist + ", max " + (maxClickDistance).ToString());
                    if (dist <= maxClickDistance && canTalkToGuard)
                    {
                        tutorialText.text = "Press E to talk.";
                        hovering = true;
                    }
                    break;

                case "Bed":
                    dist = Vector3.Distance(playerParent.transform.position, bed.transform.position);
                    //Debug.Log("Bed: dist: " + dist + ", max " + (maxClickDistance).ToString());
                    if (dist <= maxClickDistance && canUseBed)
                    {
                        tutorialText.text = "Press E to sleep.";
                        hovering = true;
                    }
                    break;

                case "Food":
                    dist = Vector3.Distance(playerParent.transform.position, food.transform.position);
                    //Debug.Log("Food: dist: " + dist + ", max: " + (maxClickDistance).ToString());
                    if (dist <= maxClickDistance && canUseFood)
                    {
                        tutorialText.text = "Press E to eat.";
                        hovering = true;
                    }
                    break;

                case "Toilet":
                    dist = Vector3.Distance(playerParent.transform.position, toilet.transform.position);
                    //Debug.Log("Toilet: dist: " + dist + ", max " + (maxClickDistance).ToString());
                    if (dist <= maxClickDistance && canUseToilet)
                    {
                        tutorialText.text = "Press E to use the toilet.";
                        hovering = true;
                    }
                    break;
            }
        }
        if (!hovering)
        {
            tutorialText.text = currentTutorialText;
        }

        // E pressed
        if (Input.GetKeyDown(KeyCode.E))
        {

            if (Physics.Raycast(ray, out hit))
            {
                float dist;

                switch (hit.collider.tag)
                {
                    case "DoorSlider":
                        dist = Vector3.Distance(playerParent.transform.position, doorSlider.transform.position);
                        if (dist <= maxClickDistance && canTalkToGuard)
                        {
                            Debug.Log("door slider clicked");

                            // If door slider is clicked on, start next dialogue (they also have needed to finish their daily tasks)
                            // TODO: Select dialogue from story (just the next yarn node)
                            dialogueRunner.StartDialogue("Start");
                        }
                        break;

                    case "Bed":
                        dist = Vector3.Distance(playerParent.transform.position, bed.transform.position);
                        if (dist <= maxClickDistance && canUseBed)
                        {
                            Debug.Log("bed clicked");

                            // Reset Player
                            playerParent.transform.position = new Vector3(-1.63f, 3.84f, 0.07f);
                            playerParent.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);

                            day++;
                            Debug.Log($"Day {day} begins!");
                            LevelManage(); // temp
                        }
                        break;

                    case "Food":
                        dist = Vector3.Distance(playerParent.transform.position, food.transform.position);
                        if (dist <= maxClickDistance && canUseFood)
                        {
                            Debug.Log("food clicked");
                        }
                        break;

                    case "Toilet":
                        dist = Vector3.Distance(playerParent.transform.position, toilet.transform.position);
                        if (dist <= maxClickDistance && canUseToilet)
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

        // Update tutorial for Day 1
        if (day == 1 && !dayOneTutorialFinished)
        {
            if (Input.GetKeyDown(KeyCode.W)) wPressed = true;
            if (Input.GetKeyDown(KeyCode.A)) aPressed = true;
            if (Input.GetKeyDown(KeyCode.S)) sPressed = true;
            if (Input.GetKeyDown(KeyCode.D)) dPressed = true;

            // Check if all have been pressed at least once
            if (wPressed && aPressed && sPressed && dPressed)
            {
                tutorialText.text = toiletText;
                currentTutorialText = tutorialText.text;

                canUseToilet = true;
                dayOneTutorialFinished = true;
            }
        }

        // enable bed // temp
        if (Input.GetKeyDown(KeyCode.P))
        {
            canUseBed = true;
        }

        // Give food // temp
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("open sesame");
            StartCoroutine(giveFood());
        }

        // take back food // temp
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("close sesame");
            StartCoroutine(foodFinish());
        }
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
        Debug.Log("End of Scene");
        characterObject.SetActive(false);
        canTalkToGuard = false;

        tutorialText.text = bedText;
        currentTutorialText = tutorialText.text;
    }

    // Gives food to player via animation
    private IEnumerator giveFood()
    {
        dsAnimator.SetBool("isOpen", true);

        yield return new WaitForSeconds(1f);

        foodAnimator.SetBool("isMealTime", true);
        canUseFood = true;

    }

    // Takes away food from player via animation
    private IEnumerator foodFinish()
    {
        canUseFood=false;
        foodAnimator.SetBool("isMealTime", false);

        yield return new WaitForSeconds(1f);

        dsAnimator.SetBool("isOpen", false);
        canTalkToGuard = true;

        tutorialText.text = talkText;
        currentTutorialText = tutorialText.text;
    }

    public int GetCurrentDay()
    {
        return day;
    }

    private void LevelManage()
    {
        // StartDialogue(); (temp use elsewhere)

        canUseBed = false;
        canUseToilet = true;

        ShowDay(day);

        switch (day)
        {
            case 1:
                // No clone spiders since new game
                canUseToilet = false;
                spiderManager.DestroyAllClones();
                spiderManager.HideBaseSpiders();
                stillSpider.SetActive(true);
                break;

            case 2:
                // Show only 3 base spiders
                stillSpider.SetActive(false);
                spiderManager.HideBaseSpiders();
                spiderManager.ShowThreeBaseSpiders();
                break;

            case 3:
                // Show one clone each of all 6 base spiders
                CloneSpiders(1); // 6 total
                smackSpiderText.SetActive(true);
                break;
            case 4:
                CloneSpiders(2); // 12 total
                smackSpiderText.SetActive(false);
                break;
            // TODO: We'll have some fallthrough cases to prevent too many spiders
        }
        
    }

    /// <summary>
    /// Clones spiders
    /// </summary>
    /// <param name="amount">Clone each spider by this amount</param>
    private void CloneSpiders(int amount = 1)
    {
        spiderManager.ShowBaseSpiders();

        // Spawn one extra clone for each base spider
        foreach (GameObject baseSpider in spiderManager.baseSpiders)
        {
            Vector3 spawnPos = baseSpider.transform.position;

            spiderManager.SpawnClones(baseSpider, amount, spawnPos, Vector3.zero);
        }

        spiderManager.HideBaseSpiders();
    }

    public void ShowDay(int number)
    {
        dayText.SetActive(true);
        dayText.GetComponent<TMP_Text>().text = "Day " + number.ToString();
        StartCoroutine(ShowDayCoroutine());
    }

    private IEnumerator ShowDayCoroutine()
    {
        yield return new WaitForSeconds(5f);

        dayText.SetActive(false);
    }
}
