using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Spider Manager Reference")]
    public SpiderManager spiderManager;

    [Header("Spawn Settings")]
    public Vector3 startPosition = Vector3.zero;

    private int day = 0;

    void Update()
    {
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

    public int GetCurrentDay()
    {
        return day;
    }
}
