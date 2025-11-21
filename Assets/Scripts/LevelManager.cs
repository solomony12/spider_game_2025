using System;
using System.Collections;
using System.Collections.Generic;
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

    private const string totalEndings = "8"; // We don't include Ending 0

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
    private int totalSpidersKilled = 0;
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
    private bool sporkIsVisible = false;
    private static bool gameIsBeingReplayed = false; // DO NOT EVER SET FALSE ANYWHERE ELSE
    private bool toiletFirstUsed = false;

    public DynamicLightingController lightingController;
    public ShrinkingRoom roomShrinker;

    public AudioManager audioManager;
    public AudioClip trayScrapeSound;
    public AudioClip doorSlideSound;
    public AudioClip toiletFlushSound;
    public AudioClip dayBoomSound;
    public AudioClip eatSound;
    public AudioClip scarySound;
    public AudioClip hummingSound;
    public AudioClip hallwaySound;
    public AudioClip bubblingSound;
    public AudioClip scuttlingSound;
    public AudioClip echoScuttlingSound;

    public PlayerMovement playerMovement;

    public GameObject[] webs;
    private int webActivateIndex = 0;

    // Endings stuff
    public GameObject notDoTaskText;
    private int bathroomSkips = 0;
    private int foodSkips = 0;
    private int spiderKillSkips = 0;
    public Animator brownWaterAnimator;
    private bool isSpiderEnding = false;
    private bool isEscapeEnding = false;
    private bool isBallEnding = false;
    public GameObject fakeDoor;
    public GameObject headSpider;
    public MoveToCamera moveToCamera;
    private bool interactWithSlider = true;
    private bool stayedInBedConsecutively = false;
    private int stayedInBedCount = 0;
    public GameObject deadSpidersWall;
    private List<GameObject> ballClones = new List<GameObject>();

    public GameObject playAgainButton;
    public GameObject mainMenuButton;
    private bool notStarting = true;
    private bool gameReachedEnding = false;
    private bool isInDialogue = false;


    public VisibilityChecker visibilityChecker;
    public GameObject dialogueHead;

    public SquishSpider squishSpiderScript;
    public BouncyBallSpawner bouncyBallSpawner;

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
        StartCoroutine(OccasionalSpiderScuttling());
        LevelManage(1);
    }

    public void PlayAgain()
    {
        if (notStarting)
        {
            notStarting = false;
            ResetGame();
            LevelManage(1);
        }
        notStarting = true;
    }

    // Main Menu
    public void ReturnToMain()
    {
        if (notStarting)
        {
            ResetGame();
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            FadeController.Instance.ResetEndingBool();
            FadeController.Instance.ResetTriggers();
            SceneManager.LoadScene("TitleScene");
        }
        notStarting = true;
    }

    public void ResetGame()
    {
        CancelInvoke();
        StopAllCoroutines();
        waiting = false;
        StartCoroutine(OccasionalSpiderScuttling());

        // Day / counters
        day = 0;
        delayTimeMin = delayTimeStartMin;
        delayTimeMax = delayTimeStartMax;
        bouncyBallSpawner.ResetBallThrownCount();

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
        interactWithSlider = true;
        toiletFirstUsed = false;

        // Spider counters
        spidersToKill = 1;
        spidersKilledToday = 0;
        totalSpidersKilled = 0;
        bathroomSkips = 0;
        foodSkips = 0;
        spiderKillSkips = 0;
        webActivateIndex = 0;

        // UI
        tutorialText.text = "WASD to move. | Mouse to look around.";
        currentTutorialText = tutorialText.text;
        tutorialTextObject.SetActive(true);
        notDoTaskText.SetActive(false);
        if (!gameIsBeingReplayed)
        {
            smackSpiderText.SetActive(false);
        }
        headerText.SetActive(false);
        blackScreen.GetComponent<UnityEngine.UI.Image>().color = new Color(0f, 0f, 0f, 1f);
        blackScreen.SetActive(false);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        mainMenuButton.SetActive(false);
        playAgainButton.SetActive(false);
        FadeController.Instance.ResetEndingBool();
        dialogueHead.SetActive(false);
        isSpiderEnding = false;
        isEscapeEnding = false;
        isBallEnding = false;
        fakeDoor.SetActive(false);
        headSpider.SetActive(false);
        gameReachedEnding = false;
        isInDialogue = false;
        food.GetComponent<MeshRenderer>().enabled = false;
        slop.GetComponent<MeshRenderer>().enabled = false;
        deadSpidersWall.SetActive(false);
        foreach (GameObject ball in ballClones)
        {
            Destroy(ball);
        }

        // Music
        audioManager.PlayMusic(hummingSound, 0.798f);

        // Player / Camera
        ResetPlayerAndCamera();
        spork.SetActive(true);
        spork.GetComponent<MeshRenderer>().enabled = false;
        squishSpiderScript.ResetSporkMaterial();
        sporkIsVisible = false;
        bouncyBallSpawner.DestroyBall();
        CharacterController cc = playerParent.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = true;
        playerParent.GetComponent<CapsuleCollider>().enabled = true;

        // Spider manager
        spiderManager.DestroyAllClones();
        spiderManager.HideBaseSpiders();
        stillSpider.SetActive(true);
        squishSpiderScript.ClearCrushedSpiders();

        // Animators
        dsAnimator.SetBool("isOpen", false);
        dsAnimator.Rebind();
        dsAnimator.Update(0f);
        foodAnimator.SetBool("isMealTime", false);
        foodAnimator.Rebind();
        foodAnimator.Update(0f);
        brownWaterAnimator.ResetTrigger("FlowBrownWater");
        brownWaterAnimator.Rebind();
        brownWaterAnimator.Update(0f);
        moveToCamera.ResetPosition();
        visibilityChecker.ResetHead();

        // Environment
        RenderSettings.fog = false;
        lightingController.ApplyPhaseSettings(DynamicLightingController.TimePhase.Morning);
        Time.timeScale = 1f;

        // Deactivate webs
        foreach (GameObject web in webs)
            web.SetActive(false);

        // Bedridden Ending
        stayedInBedConsecutively = false;
        stayedInBedCount = 0;
    }

    void OnEnable()
    {
        SquishSpider.OnSpiderSquished += HandleSpiderSquished;
        BouncyBallSpawner.OnBallEndingReached += HandleBallEnding;
    }

    void OnDisable()
    {
        SquishSpider.OnSpiderSquished -= HandleSpiderSquished;
        BouncyBallSpawner.OnBallEndingReached -= HandleBallEnding;
    }

    public void SetAction(ActionType action)
    {
        // Reset all
        canUseToilet = false;
        canUseFood = false;
        canKillSpidersTask = false;
        canTalkToGuard = false;
        if (day <= 4 || toiletFirstUsed)
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

    private void DisableAllActions()
    {
        waiting = true;
        canUseToilet = false;
        canUseFood = false;
        canKillSpidersTask = false;
        canTalkToGuard = false;
        canUseBed = false;
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
                    if (dist <= maxClickDistance && canUseBed && !toiletFirstUsed)
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
        if ((isSpiderEnding || isEscapeEnding) && Physics.Raycast(ray, out hit) && interactWithSlider)
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

                            isInDialogue = true;
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
                                stayedInBedConsecutively = true;
                                stayedInBedCount++;
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
        if ((isSpiderEnding || isEscapeEnding) && Input.GetKeyDown(KeyCode.E) && Physics.Raycast(ray, out hit) && interactWithSlider)
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
                            interactWithSlider = false;
                            LeaveRoomSpider();
                        }
                        else if (isEscapeEnding)
                        {
                            interactWithSlider = false;
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

        // Ending 0 - ??? (clipped out of map
        if (playerParent.transform.position.y < -20f)
        {
            StopAllCoroutines();
            CancelInvoke();

            // SUDDEN BLACKNESS
            // show ending text
            ShowText($"Ending 0/{totalEndings}: ???");
            audioManager.PlaySFX(dayBoomSound, 4f);

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            mainMenuButton.SetActive(true);
            playAgainButton.SetActive(true);

            Debug.Log("??? Ending");
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

        // Esc to show pause menu
        if (Input.GetKeyDown(KeyCode.Escape) && !gameReachedEnding && !isInDialogue)
        {
            ShowMenu();
        }

        // Can skip tutorial
        if (gameIsBeingReplayed && day == 1 && Input.GetKeyDown(KeyCode.V) && !toiletFirstUsed)
        {
            stillSpider.SetActive(false);
            spork.GetComponent<MeshRenderer>().enabled = true;
            sporkIsVisible = true;
            LevelManage(4);
        }


#if UNITY_EDITOR
        // TEMP DELETE TODO
        if (Input.GetKeyDown(KeyCode.M))
        {
            StopAllCoroutines();
            CancelInvoke();
            waiting = false;
            LevelManage();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            StopAllCoroutines();
            CancelInvoke();
            waiting = false;
            LevelManage(29);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            gameReachedEnding = true;
            StartCoroutine(ConstipationEnding());
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            gameReachedEnding = true;
            StartCoroutine(BedriddenEnding());
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            gameReachedEnding = true;
            StartCoroutine(StarvationEnding());
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            totalSpidersKilled = 175;
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            gameReachedEnding = true;
            isSpiderEnding = true;
            SpidersEndingPart1();
        }
#endif

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

        // Select dialogue from story (just the next yarn node)
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
            spork.SetActive(false);
            sporkIsVisible = false;

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

        if (!spork.activeSelf)
        {
            spork.SetActive(true);
            sporkIsVisible = true;
        }

        StartCoroutine(WrapUpTalking());
    }

    private IEnumerator WrapUpTalking()
    {
        lightingController.ApplyPhaseSettings(DynamicLightingController.TimePhase.Night);

        dsAnimator.SetBool("isOpen", false);
        audioManager.PlaySFX(doorSlideSound);

        yield return new WaitForSeconds(1.5f);

        isInDialogue = false;

        if (dialogueHead.activeSelf)
            dialogueHead.SetActive(false);

        tutorialText.text = bedText;
        currentTutorialText = tutorialText.text;

        toiletFirstUsed = false;
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

        // delay a bit (before spiders)
        if (day < 4)
        {
            waiting = true;
            tutorialText.text = waitText;
            currentTutorialText = tutorialText.text;
            yield return new WaitForSeconds(UnityEngine.Random.Range(delayTimeMin, delayTimeMax));
            waiting = false;
        }

        foodAnimator.SetBool("isMealTime", false);
        audioManager.PlaySFX(trayScrapeSound);

        yield return new WaitForSeconds(1f);

        food.GetComponent<MeshRenderer>().enabled = false;
        slop.GetComponent<MeshRenderer>().enabled = false;

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

        // Summon that many spiders if needed
        int totalLiveSpiders = spiderManager.numberOfLiveSpiders();
        Debug.Log($"NumOfLiveSpiders: {totalLiveSpiders} | SpidersToKill: {spidersToKill}");
        int amountToSpawn = 0;
        if (totalLiveSpiders < spidersToKill)
        {
            Debug.Log("NEEDING TO SUMMON SPIDERS");
            amountToSpawn = spidersToKill - totalLiveSpiders;
            float setsOfSix = (float)(amountToSpawn / 6.0);
            if (Math.Abs(setsOfSix - Math.Round(setsOfSix)) < 0.0001f)
                CloneSpiders((int)setsOfSix);
            else
                CloneSpiders(((int)setsOfSix)+1);

            Debug.Log($"SpidersToKill: {spidersToKill} | SetsOfSix: {setsOfSix}");
        }

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
            Debug.Log($"Current SpidersKilled Today: {spidersKilledToday}");
            if (spidersKilledToday >= spidersToKill)
            {
                totalSpidersKilled += spidersKilledToday;
                Debug.Log($"(if-part) Total Spiders Killed: {totalSpidersKilled}");
                canKillSpidersTask = false;
                notDoTaskText.SetActive(false);
                StartCoroutine(FinishKillingSpidersTask());
            }
        }
        else
        {
            totalSpidersKilled += 1;
            Debug.Log($"(else-part) Total Spiders Killed: {totalSpidersKilled}");
        }
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

    private void LevelManage(int dayUpdate = -1)
    {
        canUseFood = false;
        canKillSpidersTask = false;
        canTalkToGuard = false;
        if (day <= 4)
        {
            canUseBed = false;
        }
        else
        {
            canUseBed = true;
        }

        if (dayUpdate == -1)
        {
            day++;
        }
        else
        {
            day = dayUpdate;
        }
        Debug.Log($"Day {day} begins!");

        // Check for Circus Clown Ending
        bouncyBallSpawner.CheckBallThrownCount();

        if (day >= 30)
        {
            if (spiderKillSkips == 0 && foodSkips == 0 && bathroomSkips == 0)
            {
                DisableAllActions();
                gameReachedEnding = true;
                isEscapeEnding = true;
                EscapeEndingPart1();
            }
            else
            {
                DisableAllActions();
                gameReachedEnding = true;
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
                if (gameIsBeingReplayed)
                {
                    smackSpiderText.SetActive(true);
                    smackSpiderText.GetComponent<TMP_Text>().text = "Press V to skip tutorial (advance to Day 4)";
                }
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
                if (smackSpiderText.activeSelf)
                {
                    smackSpiderText.SetActive(false);
                }
                break;

            case 3:
                // Show one clone each of all 6 base spiders
                CloneSpiders(1); // 6 total
                smackSpiderText.SetActive(true);
                smackSpiderText.GetComponent<TMP_Text>().text = "Smack spiders (Left Click) to squish them.";
                spork.GetComponent<MeshRenderer>().enabled = true;
                sporkIsVisible = true;
                tutorialText.text = "Kill a spider.";
                currentTutorialText = tutorialText.text;
                canUseToilet = false;
                notDoTaskText.SetActive(false);
                SetAction(ActionType.SpiderKill);
                break;
            case 4:
                webs[webActivateIndex++].SetActive(true); // web 0 show
                if (smackSpiderText.activeSelf)
                {
                    smackSpiderText.SetActive(false);
                }
                DailySetup();
                break;
            case 5:
                canUseBed = true;
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
        stayedInBedConsecutively = false;
        stayedInBedCount = 0;

        // TODO: use toilet
        audioManager.PlaySFX(toiletFlushSound, 0.8f);
        if (!toiletFirstUsed)
        {
            toiletFirstUsed = true;
            smackSpiderText.SetActive(false);
        }
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

    public bool getGameReachedEndingBool()
    {
        return gameReachedEnding;
    }

    private void HandleBallEnding()
    {
        isBallEnding = true;
    }

    private bool CheckThreshold()
    {
        if (totalSpidersKilled >= 175) // normal amount killed should be 153
            return TriggerEnding(JuggernautEnding());

        if (stayedInBedConsecutively && stayedInBedCount >= 7)
            return TriggerEnding(BedriddenEnding());

        if (bathroomSkips >= 5) // 5
            return TriggerEnding(ConstipationEnding());

        if (foodSkips >= 8) // 8
            return TriggerEnding(StarvationEnding());

        if (spiderKillSkips >= 10)
        {
            DisableAllActions();
            gameReachedEnding = true;
            isSpiderEnding = true;
            SpidersEndingPart1();
            return false;
        }

        if (isBallEnding)
            return TriggerEnding(CircusClownEnding());

        // No ending triggered
        return true;
    }

    private bool TriggerEnding(IEnumerator coroutine)
    {
        DisableAllActions();
        if (notDoTaskText.activeSelf)
            notDoTaskText.SetActive(false);
        canUseBed = false;
        gameReachedEnding = true;
        StartCoroutine(coroutine);
        return false;
    }

    private IEnumerator JuggernautEnding()
    {
        // Game over
        ResetPlayerAndCamera();
        audioManager.PlaySFX(dayBoomSound, 4f);

        tutorialText.text = "Spiders fear your very existence.";
        currentTutorialText = tutorialText.text;

        spiderManager.DestroyAllClones();
        squishSpiderScript.ClearCrushedSpiders();
        // Show wall of spiders
        deadSpidersWall.SetActive(true);


        yield return new WaitForSeconds(5f);

        StartCoroutine(EndingHelper($"Ending 7/{totalEndings}: Juggernaut"));
        Debug.Log("Juggernaut Ending");
    }

    private IEnumerator ConstipationEnding()
    {
        // Game over (seem like it's the next day)
        ResetPlayerAndCamera();
        audioManager.PlaySFX(dayBoomSound, 4f);
        spork.SetActive(false);
        sporkIsVisible = false;

        tutorialText.text = "You shouldn't have held it in.";
        currentTutorialText = tutorialText.text;

        // but you can't do anything (except ball)
        yield return new WaitForSeconds(3f);

        audioManager.PlaySFX(bubblingSound);

        // brown water fills up the room slowly (and spiders get pushed away)
        brownWaterAnimator.SetTrigger("FlowBrownWater");

        yield return new WaitForSeconds(6.333f);

        // Introduce fog as murky
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color32(0x3D, 0x22, 0x0E, 0xFF);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.4f;

        yield return new WaitForSeconds(4.666f);

        StartCoroutine(EndingHelper($"Ending 4/{totalEndings}: Constipation"));
        Debug.Log("Constipation Ending");
    }

    private IEnumerator StarvationEnding()
    {
        // Game over (seem like it's the next day)
        ResetPlayerAndCamera();
        audioManager.PlaySFX(dayBoomSound, 4f);
        spork.SetActive(false);
        sporkIsVisible = false;

        tutorialText.text = "Your stomach yearns for food.";
        currentTutorialText = tutorialText.text;

        // Introduce fog as tired
        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.gray;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.25f;

        // but you can't do anything
        yield return new WaitForSeconds(3f);

        // TODO:

        StartCoroutine(EndingHelper($"Ending 6/{totalEndings}: Starvation"));
        Debug.Log("Starvation Ending");
    }

    private void SpidersEndingPart1()
    {
        // Game over (seem like it's the next day)
        ResetPlayerAndCamera();
        audioManager.PlaySFX(dayBoomSound, 4f);
        spork.SetActive(false);
        sporkIsVisible = false;

        tutorialText.text = "The door is unlocked?";
        currentTutorialText = tutorialText.text;
    }

    private void LeaveRoomSpider()
    {
        audioManager.PlayMusic(hallwaySound, 0.698f);
        headSpider.SetActive(true);

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

        // but you can't do anything
        yield return new WaitForSeconds(4.72f);

        // SPIDER ATTACK
        moveToCamera.MoveTowardCamera(0.25f);
        audioManager.PlaySFX(scarySound);
        audioManager.PlaySFX(echoScuttlingSound, 3f);

        yield return new WaitForSeconds(0.24f);
        //moveToCamera.ResetPosition();

        // SUDDEN BLACKNESS
        // show ending text
        ShowText($"True Ending {totalEndings}/{totalEndings}: Spiders");
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
        sporkIsVisible = false;

        ResetPlayerAndCamera();
        audioManager.PlaySFX(dayBoomSound, 4f);
        ShowDay(day);

        tutorialText.text = "What was the outside world like?";
        currentTutorialText = tutorialText.text;

        yield return new WaitForSeconds(3f);

        StartCoroutine(EndingHelper($"Ending 1/{totalEndings}: Resident Prisoner"));
        Debug.Log("Resident Prisoner Ending");
    }

    private void EscapeEndingPart1()
    {
        // Game over (seem like it's the next day)
        ResetPlayerAndCamera();
        audioManager.PlaySFX(dayBoomSound, 4f);
        spork.SetActive(false);
        sporkIsVisible = false;

        tutorialText.text = "The door is unlocked?";
        currentTutorialText = tutorialText.text;
    }

    private void LeaveRoomEscape()
    {
        audioManager.PlayMusic(hallwaySound, 0.698f);

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

        // but you can't do anything
        yield return new WaitForSeconds(5f);

        StartCoroutine(EndingHelper($"Ending 2/{totalEndings}: Good Behavior"));

        Debug.Log("Good Behavior Ending");
    }

    private IEnumerator BedriddenEnding()
    {
        // Game over
        canUseBed = false;

        CharacterController cc = playerParent.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;
        playerParent.GetComponent<CapsuleCollider>().enabled = false;
        // Set Player to bed position
        playerParent.transform.position = new Vector3(-3.71f, 1.23f, -3.42f);
        playerParent.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
        playerMovement.ResetCameraRotation();

        audioManager.PlaySFX(dayBoomSound, 4f);
        spork.SetActive(false);
        sporkIsVisible = false;

        tutorialText.text = "You don't want to get up.";
        currentTutorialText = tutorialText.text;

        // but you can't do anything (except look around)
        yield return new WaitForSeconds(5f);

        StartCoroutine(EndingHelper($"Ending 5/{totalEndings}: Bedridden"));
        Debug.Log("Bedridden Ending");
    }

    private IEnumerator CircusClownEnding()
    {
        // Game over (seem like it's the next day)
        ResetPlayerAndCamera();
        audioManager.PlaySFX(dayBoomSound, 4f);
        spork.SetActive(false);
        sporkIsVisible = false;

        tutorialText.text = "Juggling is quite fun.";
        currentTutorialText = tutorialText.text;

        // We don't want the player to move as it's possible to clip out and get Ending 0
        CharacterController cc = playerParent.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        StartCoroutine(SpawnBalls());

        // but you can't do anything
        yield return new WaitForSeconds(5f);

        StartCoroutine(EndingHelper($"Ending 3/{totalEndings}: Circus Clown"));
        Debug.Log("Circus Clown Ending");
    }

    IEnumerator SpawnBalls()
    {
        while (isBallEnding)
        {
            GameObject ballClone = bouncyBallSpawner.SpawnBall(true);
            ballClones.Add(ballClone);
            yield return new WaitForSeconds(0.3f);
        }
    }

    private IEnumerator EndingHelper(string text)
    {
        gameReachedEnding = true;

        if (notDoTaskText.activeSelf)
        {
            notDoTaskText.SetActive(false);
        }

        // fade to black
        FadeController.Instance.FadeToBlack();

        yield return new WaitForSeconds(2f);
        // show ending text
        ShowText(text);
        audioManager.PlaySFX(dayBoomSound, 4f);
        isBallEnding = false;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        gameIsBeingReplayed = true;
        mainMenuButton.SetActive(true);
        playAgainButton.SetActive(true);

    }

    public bool PlaySporkSounds()
    {
        return sporkIsVisible;
    }

    private void ShowMenu()
    {
        if (gameReachedEnding)
            return;

        blackScreen.SetActive(!blackScreen.activeSelf);
        playAgainButton.SetActive(!playAgainButton.activeSelf);
        mainMenuButton.SetActive(!mainMenuButton.activeSelf);

        if (playAgainButton.activeSelf)
        {
            blackScreen.GetComponent<UnityEngine.UI.Image>().color = new Color(0f, 0f, 0f, 0.5f);
            Time.timeScale = 0f;
            ShowText("Paused");
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
        else
        {
            blackScreen.GetComponent<UnityEngine.UI.Image>().color = new Color(0f, 0f, 0f, 1f);
            Time.timeScale = 1f;
            headerText.SetActive(false);
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
    }

    private IEnumerator OccasionalSpiderScuttling()
    {
        while (!gameReachedEnding)
        {
            float waitTime = UnityEngine.Random.Range(10f, 25f);
            yield return new WaitForSeconds(waitTime);
            audioManager.PlaySFX(scuttlingSound);
        }
    }
}
