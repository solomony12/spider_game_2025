using UnityEngine;
using Yarn;
using static Unity.Collections.Unicode;

public class LevelManager : MonoBehaviour
{
    [Header("Spider Manager Reference")]
    public SpiderManager spiderManager;

    [Header("Spawn Settings")]
    public Vector3 startPosition = Vector3.zero;

    public Yarn.Unity.DialogueRunner dialogueRunner;

    private int day = 0;

    void Awake()
    {
        // Subscribe to the event
        dialogueRunner.onDialogueComplete.AddListener(OnDialogueFinished);
    }

    private void Start()
    {
        dialogueRunner.StartDialogue("MainStory");
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
    }

    void OnDialogueFinished()
    {
        // TODO: Progress to the next day (restart everything like make new clones but keep the squished ones)
        // Clones, daily reset, next yarn?
        Debug.Log("The end");
    }

    public int GetCurrentDay()
    {
        return day;
    }
}
