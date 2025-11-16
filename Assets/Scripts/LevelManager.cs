using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Yarn;
using Yarn.Unity;
using static System.Net.Mime.MediaTypeNames;

public class LevelManager : MonoBehaviour
{
    [Header("Spider Manager Reference")]
    public SpiderManager spiderManager;
    public GameObject stillSpider;

    [Header("Spawn Settings")]
    public Vector3 startPosition = Vector3.zero;

    public enum ActionType
    {
        Toilet,
        Food,
        SpiderKill,
        TalkGuard,
        Bed
    }

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
    private string waitText = "Free time.";
    public GameObject dayText;
    private string currentTutorialText;
    public GameObject headerText;
    public GameObject blackScreen;

    private bool wPressed = false;
    private bool dPressed = false;
    private bool dayOneTutorialFinished = false;

    private int spacePressedTimes = 0;
    private bool dayTwoTutorialFinished = false;

    private bool canUseToilet = false;
    private bool canUseBed = false;
    private bool canUseFood = false;
    private bool canTalkToGuard = false;
    private bool canKillSpidersTask = false;
    private int spidersToKill = 1;
    private int spidersKilledToday = 0;
    private bool waiting = false;
    bool hasDialogueDay = false;

    private int day;
    int[] specialDaysList = { 4, 5, 8, 10, 12, 14, 17, 19, 21, 23, 25 };

    private float delayTimeStartMin = 3f; // start off at 3f
    private float delayTimeStartMax = 5f; // start off at 5f
    private float delayTimeEndMin = 5f; // end at 5f
    private float delayTimeEndMax = 8f; // end at 8f
    private float delayTimeMin;
    private float delayTimeMax;

    public GameObject spork;

    public DynamicLightingController lightingController;
    public ShrinkingRoom roomShrinker;

    public AudioManager audioManager;
    public AudioClip trayScrapeSound;
    public AudioClip doorSlideSound;
    public AudioClip toiletFlushSound;
    public AudioClip dayBoomSound;
    public AudioClip eatSound;

    public PlayerMovement playerMovement;

    public GameObject[] webs;
    private int webActivateIndex = 0;

    public GameObject notDoTaskText;
    private int bathroomSkips = 0;
    private int foodSkips = 0;
    private int spiderKillSkips = 0;
    public Animator brownWaterAnimator;
    private bool isSpiderEnding = false;
    private bool isEscapeEnding = false;
    public GameObject fakeDoor;

    public GameObject playAgainButton;
    public GameObject mainMenuButton;
    
    public VisibilityChecker visibilityChecker;
    public GameObject dialogueHead;

    void Awake()
    {
        dialogueRunner.onDialogueComplete.AddListener(OnDialogueFinished);
        characterObject = GameObject.FindWithTag("VN_Char");
        delayTimeMin = delayTimeStartMin;
        delayTimeMax = delayTimeStartMax;
    }

    private void Start()
    {
        tutorialText = tutorialTextObject.GetComponent<TMP_Text>();
        ResetGame();
        characterObject.SetActive(false);
        
        if (FadeController.Instance == null)
        {
            Debug.Log("ERROR: FadeController not found!!!");
            return;
        }

        LevelManage();
    }

    public void PlayAgain()
    {
        ResetGame();
        LevelManage();
    }

    public void ReturnToMain()
    {
        FadeController.Instance.ResetEndingBool();
        SceneManager.LoadScene("TitleScene");
    }

    public void ResetGame()
    {
        // Day / counters
        day = 0;
        delayTimeMin = delayTimeStartMin;
        delayTimeMax = delayTimeStartMax;

        // Tutorial flags
        dayOneTutorialFinished = false;
        dayTwoTutorialFinished = false;
        wPressed = false;
        dPressed = false;
        spacePressedTimes = 0;

        // Actions
        canUseToilet = false;
        canUseBed = false;
        canUseFood = false;
        canTalkToGuard = false;
        canKillSpidersTask = false;

        // Spider counters
        spidersToKill = 1;
        spidersKilledToday = 0;
        bathroomSkips = 0;
        foodSkips = 0;
        spiderKillSkips = 0;
        webActivateIndex = 0;

        // UI
        tutorialText.text = "WASD to move. | Mouse to look around.";
        currentTutorialText = tutorialText.text;
        tutorialTextObject.SetActive(true);
        notDoTaskText.SetActive(false);
        smackSpiderText.SetActive(false);
        headerText.SetActive(false);
        blackScreen.SetActive(false);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        mainMenuButton.SetActive(false);
        playAgainButton.SetActive(false);
        FadeController.Instance.ResetEndingBool();
        dialogueHead.SetActive(false);
        isSpiderEnding = false;
        isEscapeEnding = false;
        fakeDoor.SetActive(false);

        // Player / Camera
        ResetPlayerAndCamera();
        spork.GetComponent<MeshRenderer>().enabled = false;

        // Spider manager
        spiderManager.DestroyAllClones();
        spiderManager.HideBaseSpiders();
        stillSpider.SetActive(true);

        // Animators
        dsAnimator.SetBool("isOpen", false);
        foodAnimator.SetBool("isMealTime", false);
        brownWaterAnimator.ResetTrigger("FlowBrownWater");
        brownWaterAnimator.Play("BrownWaterLow", 0, 0f);
        visibilityChecker.ResetHead();

        // Environment
        RenderSettings.fog = false;
        lightingController.ApplyPhaseSettings(DynamicLightingController.TimePhase.Morning);

        // Deactivate webs
        foreach (GameObject web in webs)
            web.SetActive(false);
    }

    void OnEnable()
    {
        SquishSpider.OnSpiderSquished += HandleSpiderSquished;
    }

    void OnDisable()
    {
        SquishSpider.OnSpiderSquished -= HandleSpiderSquished;
    }

    public void SetAction(ActionType action)
    {
        // Reset all
        canUseToilet = false;
        canUseFood = false;
        canKillSpidersTask = false;
        canTalkToGuard = false;
        canUseBed = false;

        // Set the selected one
        switch (action)
        {
            case ActionType.Toilet:
                canUseToilet = true;
                break;
            case ActionType.Food:
                canUseFood = true;
                break;
            case ActionType.SpiderKill:
                canKillSpidersTask = true;
                break;
            case ActionType.TalkGuard:
                canTalkToGuard = true;
                break;
            case ActionType.Bed:
                canUseBed = true;
                break;
        }
    }

    // Day 3 Tutorial
    void HandleSpiderSquished()
    {
        if (day == 3 && smackSpiderText.activeSelf)
        {
            canKillSpidersTask = false;
            smackSpiderText.SetActive(false);

            tutorialText.text = toiletText;
            currentTutorialText = tutorialText.text;

            SetAction(ActionType.Toilet);
        }
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            roomShrinker.ShrinkRoom();
        }*/

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool hovering = false;

        // Hovering over for tutorial
        if (Physics.Raycast(ray, out hit) && !waiting)
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

        // Spider/escape ending hovering
        if ((isSpiderEnding || isEscapeEnding) && Physics.Raycast(ray, out hit))
        {
            float dist;

            switch (hit.collider.tag)
            {
                case "DoorSlider":
                    dist = Vector3.Distance(playerParent.transform.position, doorSlider.transform.position);
                    //Debug.Log("Door Slider: dist: " + dist + ", max " + (maxClickDistance).ToString());
                    if (dist <= maxClickDistance)
                    {
                        tutorialText.text = "Press E to leave.";
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
        if (Input.GetKeyDown(KeyCode.E) && !waiting)
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

                            // Check for possible ending
                            bool nextDay = true;
                            try
                            {
                                nextDay = CheckThreshold();
                            }
                            catch (Exception e)
                            {
                                Debug.Log("ERROR: " + e);
                            }
                            

                            // Progress to the next day (restart everything like make new clones but keep the squished ones)
                            // Clones, daily reset, next yarn?
                            if (nextDay)
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

        // Spider/escape ending leave room 
        if ((isSpiderEnding || isEscapeEnding) && Input.GetKeyDown(KeyCode.E) && Physics.Raycast(ray, out hit))
        {
            float dist;

            switch (hit.collider.tag)
            {
                case "DoorSlider":
                    dist = Vector3.Distance(playerParent.transform.position, doorSlider.transform.position);
                    //Debug.Log("Door Slider: dist: " + dist + ", max " + (maxClickDistance).ToString());
                    if (dist <= maxClickDistance)
                    {
                        Debug.Log("Leave Room");

                        if (isSpiderEnding)
                        {
                            LeaveRoomSpider();
                        }
                        else if (isEscapeEnding)
                        {
                            LeaveRoomEscape();
                        }
                    }
                    break;
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
            if (Input.GetKeyDown(KeyCode.D)) dPressed = true;

            // Check if all have been pressed at least once
            if (wPressed && dPressed)
            {
                tutorialText.text = toiletText;
                currentTutorialText = tutorialText.text;

                SetAction(ActionType.Toilet);
                dayOneTutorialFinished = true;
            }
        }

        // Update tutorial for Day 2 (ball)
        if (day == 2 && !dayTwoTutorialFinished)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                spacePressedTimes++;
            }

            // Check if all have been pressed at least once
            if (spacePressedTimes == 2)
            {
                tutorialText.text = toiletText;
                currentTutorialText = tutorialText.text;

                SetAction(ActionType.Toilet);
                dayTwoTutorialFinished = true;
            }
        }

        // Press P to skip task
        if (Input.GetKeyDown(KeyCode.P) && notDoTaskText.activeSelf)
        {
            if (canUseToilet)
            {
                bathroomSkips++;
                StartCoroutine(UseToiletFinish());
            }
            else if (canUseFood)
            {
                foodSkips++;
                StartCoroutine(foodFinish());
            }
            else if (canKillSpidersTask)
            {
                spiderKillSkips++;
                canKillSpidersTask = false;
                StartCoroutine(FinishKillingSpidersTask());
            }
            else
            {
                Debug.Log("ERROR: They skipped a task that is not skippable or the canUse/Kill bool is put in the wrong place.");
            }

            // Make sure text is disabled
            if (notDoTaskText.activeSelf)
            {
                notDoTaskText.SetActive(false);
            }
        }

        // TEMP DELETE TODO
        if (Input.GetKeyDown(KeyCode.M))
        {
            LevelManage();
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
        canTalkToGuard = false;

        // TODO: [VN] use if needed in the future
        //characterObject.SetActive(true);

        // TODO: Select dialogue from story (just the next yarn node)
        int dialogueDay = -1;
        hasDialogueDay = false;

        for (int i = 0; i < specialDaysList.Length; i++)
        {
            if (specialDaysList[i] == day)
            {
                dialogueDay = day;
                hasDialogueDay = true;
                break;
            }
        }

        if (hasDialogueDay)
        {
            dialogueHead.SetActive(true);
        }
        else
        {
            dialogueHead.SetActive(false);
        }

        dsAnimator.SetBool("isOpen", true);
        audioManager.PlaySFX(doorSlideSound);

        yield return new WaitForSeconds(1.5f);

        tutorialText.text = "Click anywhere to continue.";
        currentTutorialText = tutorialText.text;

        if (hasDialogueDay)
        {
            string dialogueNode = "day_" + day.ToString();
            dialogueRunner.StartDialogue(dialogueNode);
        }
        else
        {
            dialogueRunner.StartDialogue("BaseDialogue");
        }
    }

    void OnDialogueFinished()
    {
        Debug.Log("End of Scene");
        // TODO: [VN] use if needed in the future
        //characterObject.SetActive(false);
        canTalkToGuard = false;

        StartCoroutine(WrapUpTalking());
    }

    private IEnumerator WrapUpTalking()
    {
        // delay a bit
        if (!hasDialogueDay)
        {
            waiting = true;
            tutorialText.text = waitText;
            currentTutorialText = tutorialText.text;
            yield return new WaitForSeconds(UnityEngine.Random.Range(delayTimeMin, delayTimeMax));
            waiting = false;
        }

        lightingController.ApplyPhaseSettings(DynamicLightingController.TimePhase.Night);

        dsAnimator.SetBool("isOpen", false);
        audioManager.PlaySFX(doorSlideSound);

        yield return new WaitForSeconds(1.5f);

        if (dialogueHead.activeSelf)
            dialogueHead.SetActive(false);

        tutorialText.text = bedText;
        currentTutorialText = tutorialText.text;
        SetAction(ActionType.Bed);
    }

    // Gives food to player via animation
    private IEnumerator giveFood()
    {
        food.GetComponent<MeshRenderer>().enabled = true;
        slop.GetComponent<MeshRenderer>().enabled = true;

        foodAnimator.SetBool("isMealTime", true);
        audioManager.PlaySFX(trayScrapeSound);
        yield return new WaitForSeconds(1f);

        SetAction(ActionType.Food);
        if (day > 4)
        {
            notDoTaskText.SetActive(true);
        }

    }

    private void EatFood()
    {
        // TODO: eat
        audioManager.PlaySFX(eatSound);
        slop.GetComponent<MeshRenderer>().enabled = false;
        notDoTaskText.SetActive(false);

        StartCoroutine(foodFinish());
    }

    // Takes away food from player via animation
    private IEnumerator foodFinish()
    {
        canUseFood = false;

        // delay a bit
        waiting = true;
        tutorialText.text = waitText;
        currentTutorialText = tutorialText.text;
        yield return new WaitForSeconds(UnityEngine.Random.Range(delayTimeMin, delayTimeMax));
        waiting = false;

        foodAnimator.SetBool("isMealTime", false);
        audioManager.PlaySFX(trayScrapeSound);

        yield return new WaitForSeconds(1f);

        food.GetComponent<MeshRenderer>().enabled = false;

        KillSpidersTask();
    }

    private void KillSpidersTask()
    {
        // Not this task until day 4
        if (day < 4)
        {
            StartCoroutine(FinishKillingSpidersTask());
            return;
        }

        spidersKilledToday = 0;
        spidersToKill = Math.Min(10, 1 + (day - 1) / 3); // no more than 10 a day;

        tutorialText.text = "Kill " + spidersToKill + " spiders.";
        currentTutorialText = tutorialText.text;

        SetAction(ActionType.SpiderKill);
        if (day > 4)
        {
            notDoTaskText.SetActive(true);
        }
    }

    public void SpiderTaskCounter()
    {
        if (canKillSpidersTask && !waiting)
        {
            spidersKilledToday++;
            if (spidersKilledToday >= spidersToKill)
            {
                canKillSpidersTask = false;
                notDoTaskText.SetActive(false);
                StartCoroutine(FinishKillingSpidersTask());
            }
        }
    }

    public bool getIsSpiderTaskOn()
    {
        return canKillSpidersTask;
    }

    private IEnumerator FinishKillingSpidersTask()
    {
        // start on day 4
        if (day >= 4)
        {
            // delay a bit
            waiting = true;
            tutorialText.text = waitText;
            currentTutorialText = tutorialText.text;
            yield return new WaitForSeconds(UnityEngine.Random.Range(delayTimeMin, delayTimeMax));
            waiting = false;
        }


        SetAction(ActionType.TalkGuard);

        tutorialText.text = talkText;
        currentTutorialText = tutorialText.text;

        lightingController.ApplyPhaseSettings(DynamicLightingController.TimePhase.Evening);
    }

    private void LevelManage()
    {
        canUseFood = false;
        canKillSpidersTask = false;
        canTalkToGuard = false;
        canUseBed = false;

        day++;
        Debug.Log($"Day {day} begins!");

        if (day >= 30)
        {
            if (spiderKillSkips == 0 && foodSkips == 0 && bathroomSkips == 0)
            {
                isEscapeEnding = true;
                EscapeEndingPart1();
            }
            else
            {
                StartCoroutine(ResidentPrisonerEnding());
            }
            return;
        }

        ResetPlayerAndCamera();

        // Increase time slightly
        delayTimeMin = Math.Min(delayTimeMin + 0.1f, delayTimeEndMin);
        delayTimeMax = Math.Min(delayTimeMax + 0.1f, delayTimeEndMax);

        lightingController.ApplyPhaseSettings(DynamicLightingController.TimePhase.Morning);
        audioManager.PlaySFX(dayBoomSound, 4f);

        // End of tutorial
        if (day > 3)
        {
            tutorialText.text = toiletText;
            currentTutorialText = tutorialText.text;
        }

        canUseBed = false;

        SetAction(ActionType.Toilet);
        if (day > 4)
        {
            notDoTaskText.SetActive(true);
        }

        ShowDay(day);

        switch (day)
        {
            case 1:
                // No clone spiders since new game
                canUseToilet = false;
                notDoTaskText.SetActive(false);
                spiderManager.DestroyAllClones();
                spiderManager.HideBaseSpiders();
                stillSpider.SetActive(true);
                break;

            case 2:
                // Show only 3 base spiders
                stillSpider.SetActive(false);
                notDoTaskText.SetActive(false);
                spiderManager.HideBaseSpiders();
                spiderManager.ShowThreeBaseSpiders();
                tutorialText.text = "[Space] to throw ball. | [Space] to retrieve it.";
                currentTutorialText = tutorialText.text;
                canUseToilet = false;
                break;

            case 3:
                // Show one clone each of all 6 base spiders
                CloneSpiders(1); // 6 total
                smackSpiderText.SetActive(true);
                spork.GetComponent<MeshRenderer>().enabled = true;
                tutorialText.text = "Kill a spider.";
                currentTutorialText = tutorialText.text;
                canUseToilet = false;
                notDoTaskText.SetActive(false);
                SetAction(ActionType.SpiderKill);
                break;
            case 4:
                webs[webActivateIndex++].SetActive(true); // web 0 show
                DailySetup();
                break;
            case 7:
                webs[webActivateIndex++].SetActive(true); // web 1 show
                DailySetup();
                break;
            case 9:
                CloneSpiders(1);

                tutorialText.text = "...";
                currentTutorialText = tutorialText.text;

                visibilityChecker.ShowHead();
                canUseToilet = false;
                notDoTaskText.SetActive(false);

                break;
            case 10:
                webs[webActivateIndex++].SetActive(true); // web 2 show
                DailySetup();
                break;
            case 13:
                webs[webActivateIndex++].SetActive(true); // web 3 show
                DailySetup();
                break;
            default:
                DailySetup();
                break;
        }
        
    }

    private void ResetPlayerAndCamera()
    {
        // Reset Player
        // Disable the CharacterController temporarily
        CharacterController cc = playerParent.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        // Teleport player
        playerParent.transform.position = new Vector3(-1.63f, 3.84f, 0.07f);
        playerParent.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);

        // Re-enable CharacterController
        if (cc != null)
            cc.enabled = true;

        // Reset camera
        playerMovement.ResetCameraRotation();
    }

    private void DailySetup()
    {
        int totalLiveSpiders = spiderManager.numberOfLiveSpiders();
        int totalNeeded = (day - 2) * 6; // -2 since first two days are no-spawn days
        int amountToSpawn = totalNeeded - totalLiveSpiders;
        int setsOfSix = amountToSpawn / 6;
        CloneSpiders(setsOfSix);
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

    private void ShowDay(int number)
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

    private void ShowText(string text)
    {
        headerText.SetActive(true);
        headerText.GetComponent<TMP_Text>().text = text;
        blackScreen.SetActive(true);
    }

    private void UseToiletStart()
    {
        // TODO: use toilet
        audioManager.PlaySFX(toiletFlushSound, 0.8f);
        StartCoroutine(UseToiletFinish());
    }

    private IEnumerator UseToiletFinish()
    {
        notDoTaskText.SetActive(false);
        canUseToilet = false;

        // delay a bit
        waiting = true;
        tutorialText.text = waitText;
        currentTutorialText = tutorialText.text;
        yield return new WaitForSeconds(UnityEngine.Random.Range(delayTimeMin, delayTimeMax));
        waiting = false;

        lightingController.ApplyPhaseSettings(DynamicLightingController.TimePhase.Noon);

        tutorialText.text = foodText;
        currentTutorialText = tutorialText.text;

        StartCoroutine(giveFood());
    }

    public void HideHead()
    {
        tutorialText.text = toiletText;
        currentTutorialText = tutorialText.text;

        SetAction(ActionType.Toilet);
    }

    public int getCurrentDay()
    {
        return day;
    }

    private bool CheckThreshold()
    {
        if (bathroomSkips >= 5) // 5
        {
            StartCoroutine(BathroomEnding());
            return false;
        }

        else if (foodSkips >= 8) // 8
        {
            StartCoroutine(StarvationEnding());
            return false;
        }

        else if (spiderKillSkips >= 10) // 10
        {
            isSpiderEnding = true;
            SpidersEndingPart1();
            return false;
        }
        else
            return true;
    }

    private IEnumerator BathroomEnding()
    {
        // Game over (seem like it's the next day)
        ResetPlayerAndCamera();
        audioManager.PlaySFX(dayBoomSound, 4f);
        spork.SetActive(false);

        tutorialText.text = "You shouldn't have held it in.";
        currentTutorialText = tutorialText.text;

        // but you can't do anything (except ball)
        yield return new WaitForSeconds(3f);

        // brown water fills up the room slowly (and spiders get pushed away)
        brownWaterAnimator.SetTrigger("FlowBrownWater");

        yield return new WaitForSeconds(6.333f);

        // Introduce fog as murky
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color32(0x3D, 0x22, 0x0E, 0xFF);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.4f;

        yield return new WaitForSeconds(4.666f);

        StartCoroutine(EndingHelper("Ending 1/5: Constipation"));
        Debug.Log("Constipation Ending");
    }

    private IEnumerator StarvationEnding()
    {
        // Game over (seem like it's the next day)
        ResetPlayerAndCamera();
        audioManager.PlaySFX(dayBoomSound, 4f);
        spork.SetActive(false);

        tutorialText.text = "Your stomach yearns for food.";
        currentTutorialText = tutorialText.text;

        // Introduce fog as tired
        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.gray;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.25f;

        // but you can't do anything (except ball)
        yield return new WaitForSeconds(3f);

        // TODO:

        StartCoroutine(EndingHelper("Ending 2/5: Starvation"));
        Debug.Log("Starvation Ending");
    }

    private void SpidersEndingPart1()
    {
        // Game over (seem like it's the next day)
        ResetPlayerAndCamera();
        audioManager.PlaySFX(dayBoomSound, 4f);
        spork.SetActive(false);

        tutorialText.text = "The door is unlocked?";
        currentTutorialText = tutorialText.text;
    }

    private void LeaveRoomSpider()
    {
        // Disable the CharacterController temporarily
        CharacterController cc = playerParent.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        // Teleport player
        playerParent.transform.position = new Vector3(7.07f, 3.84f, -1.4f);

        // Re-enable CharacterController
        if (cc != null)
            cc.enabled = true;

        fakeDoor.SetActive(true);

        tutorialText.text = "...";
        currentTutorialText = tutorialText.text;

        StartCoroutine(SpidersEndingPart2());
    }

    private IEnumerator SpidersEndingPart2()
    {

        // but you can't do anything (except ball)
        yield return new WaitForSeconds(3f);

        // TODO: SPIDERS ATTACK FROM BOTH SIDES

        // SUDDEN BLACKNESS
        // show ending text
        ShowText("Ending 3/5: Spiders");
        audioManager.PlaySFX(dayBoomSound, 4f);

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        mainMenuButton.SetActive(true);
        playAgainButton.SetActive(true);

        Debug.Log("Spiders Ending");
    }

    private IEnumerator ResidentPrisonerEnding()
    {
        spork.SetActive(false);

        ResetPlayerAndCamera();
        audioManager.PlaySFX(dayBoomSound, 4f);
        ShowDay(day);

        tutorialText.text = "What was the outside world like?";
        currentTutorialText = tutorialText.text;

        yield return new WaitForSeconds(3f);

        StartCoroutine(EndingHelper("Ending 4/5: Resident Prisoner"));
        Debug.Log("Resident Prisoner Ending");
    }

    private void EscapeEndingPart1()
    {
        // Game over (seem like it's the next day)
        ResetPlayerAndCamera();
        audioManager.PlaySFX(dayBoomSound, 4f);
        spork.SetActive(false);

        tutorialText.text = "The door is unlocked?";
        currentTutorialText = tutorialText.text;
    }

    private void LeaveRoomEscape()
    {
        // Disable the CharacterController temporarily
        CharacterController cc = playerParent.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        // Teleport player
        playerParent.transform.position = new Vector3(7.07f, 3.84f, -1.4f);

        // Re-enable CharacterController
        if (cc != null)
            cc.enabled = true;

        fakeDoor.SetActive(true);

        tutorialText.text = "...";
        currentTutorialText = tutorialText.text;

        StartCoroutine(EscapeEndingPart2());
    }

    private IEnumerator EscapeEndingPart2()
    {

        // but you can't do anything (except ball)
        yield return new WaitForSeconds(5f);

        StartCoroutine(EndingHelper("Ending 5/5: Good Behavior"));

        Debug.Log("Good Behavior Ending");
    }

    private IEnumerator EndingHelper(string text)
    {
        // fade to black
        FadeController.Instance.FadeToBlack();

        yield return new WaitForSeconds(2f);
        // show ending text
        ShowText(text);
        audioManager.PlaySFX(dayBoomSound, 4f);

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        mainMenuButton.SetActive(true);
        playAgainButton.SetActive(true);

    }
}
