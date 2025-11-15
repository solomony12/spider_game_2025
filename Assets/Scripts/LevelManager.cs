using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Yarn;
using Yarn.Unity;

public class LevelManager : MonoBehaviour
{
    [Header("Spider Manager Reference")]
    public SpiderManager spiderManager;
    public GameObject stillSpider;

    [Header("Spawn Settings")]
    public Vector3 startPosition = Vector3.zero;

    public Yarn.Unity.DialogueRunner dialogueRunner;
    public static GameObject characterObject;
    public static string characterArtPath = "Art/Characters/";

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
    private string talkText = "Talk to the guard (observation slot).";
    private string bedText = "Go to bed.";
    public GameObject dayText;
    private string currentTutorialText;

    private bool wPressed = false;
    private bool aPressed = false;
    private bool dPressed = false;
    private bool dayOneTutorialFinished = false;

    private bool canUseToilet = false;
    private bool canUseBed = false;
    private bool canUseFood = false;
    private bool canTalkToGuard = false;

    private int day;

    public GameObject spork;

    void Awake()
    {
        dialogueRunner.onDialogueComplete.AddListener(OnDialogueFinished);
        characterObject = GameObject.FindWithTag("VN_Char");
    }

    private void Start()
    {
        characterObject.SetActive(false);
        day = 1;
        dayOneTutorialFinished = false;
        stillSpider.SetActive(true);
        smackSpiderText.SetActive(false);
        tutorialTextObject.SetActive(true); // ALWAYS TRUE
        tutorialText = tutorialTextObject.GetComponent<TMP_Text>();
        tutorialText.text = "WASD to move. | Mouse to look around.";
        currentTutorialText = tutorialText.text;
        spork.GetComponent<MeshRenderer>().enabled = false;
        LevelManage();
    }

    void OnEnable()
    {
        SquishSpider.OnSpiderSquished += HandleSpiderSquished;
    }

    void OnDisable()
    {
        SquishSpider.OnSpiderSquished -= HandleSpiderSquished;
    }

    // Day 3 Tutorial
    void HandleSpiderSquished()
    {
        if (day == 3 && smackSpiderText.activeSelf)
        {
            smackSpiderText.SetActive(false);

            tutorialText.text = toiletText;
            currentTutorialText = tutorialText.text;

            canUseToilet = true;
            dayOneTutorialFinished = true;
        }
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

        // E pressed // Press E
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

                            StartCoroutine(PlayNextScene());
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

                            // Progress to the next day (restart everything like make new clones but keep the squished ones)
                            // Clones, daily reset, next yarn?
                            day++;
                            Debug.Log($"Day {day} begins!");
                            LevelManage();
                        }
                        break;

                    case "Food":
                        dist = Vector3.Distance(playerParent.transform.position, food.transform.position);
                        if (dist <= maxClickDistance && canUseFood)
                        {
                            Debug.Log("food clicked");
                            EatFood();
                        }
                        break;

                    case "Toilet":
                        dist = Vector3.Distance(playerParent.transform.position, toilet.transform.position);
                        if (dist <= maxClickDistance && canUseToilet)
                        {
                            Debug.Log("toilet clicked");
                            UseToiletStart();
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
            if (Input.GetKeyDown(KeyCode.D)) dPressed = true;

            // Check if all have been pressed at least once
            if (wPressed && aPressed && dPressed)
            {
                tutorialText.text = toiletText;
                currentTutorialText = tutorialText.text;

                canUseToilet = true;
                dayOneTutorialFinished = true;
            }
        }
    }

    [YarnCommand("character")]
    public static void ChangeCharacterImage(string poseName)
    {
        string path = $"{characterArtPath}{poseName}";
        Sprite newSprite = Resources.Load<Sprite>(path);

        if (newSprite != null)
        {
            characterObject.GetComponent<UnityEngine.UI.Image>().sprite = newSprite;
        }
        else
        {
            Debug.LogError($"'{poseName}' sprite not found.\nTried Path: {path}");
        }
    }
    private IEnumerator PlayNextScene()
    {
        dsAnimator.SetBool("isOpen", true);

        yield return new WaitForSeconds(1.5f);

        tutorialText.text = "Click anywhere to continue.";
        currentTutorialText = tutorialText.text;

        characterObject.SetActive(true);

        // TODO: Select dialogue from story (just the next yarn node)
        dialogueRunner.StartDialogue("Start");
    }

    void OnDialogueFinished()
    {
        Debug.Log("End of Scene");
        characterObject.SetActive(false);
        canTalkToGuard = false;

        StartCoroutine(WrapUpTalking());
    }

    private IEnumerator WrapUpTalking()
    {
        dsAnimator.SetBool("isOpen", false);

        yield return new WaitForSeconds(1.5f);

        tutorialText.text = bedText;
        currentTutorialText = tutorialText.text;
        canUseBed = true;
    }

    // Gives food to player via animation
    private IEnumerator giveFood()
    {
        food.GetComponent<MeshRenderer>().enabled = true;
        slop.GetComponent<MeshRenderer>().enabled = true;

        foodAnimator.SetBool("isMealTime", true);
        yield return new WaitForSeconds(1f);
        canUseFood = true;

    }

    private void EatFood()
    {
        // TODO: eat
        slop.GetComponent<MeshRenderer>().enabled = false;

        StartCoroutine(foodFinish());
    }

    // Takes away food from player via animation
    private IEnumerator foodFinish()
    {
        canUseFood = false;
        foodAnimator.SetBool("isMealTime", false);

        yield return new WaitForSeconds(1f);

        canTalkToGuard = true;
        food.GetComponent<MeshRenderer>().enabled = false;

        tutorialText.text = talkText;
        currentTutorialText = tutorialText.text;
    }

    private void LevelManage()
    {
        if (day != 1 && day != 3)
        {
            tutorialText.text = toiletText;
            currentTutorialText = tutorialText.text;
        }

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
                spork.GetComponent<MeshRenderer>().enabled = true;
                tutorialText.text = "Kill a spider.";
                currentTutorialText = tutorialText.text;
                canUseToilet = false;
                break;
            default:
                CloneSpiders(1);
                break;
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

    private void UseToiletStart()
    {
        // TODO: use toilet

        UseToiletFinish();
    }

    private void UseToiletFinish()
    {
        canUseToilet = false;

        tutorialText.text = foodText;
        currentTutorialText = tutorialText.text;

        StartCoroutine(giveFood());
    }
}
