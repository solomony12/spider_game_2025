using System.Collections.Generic;
using UnityEngine;

public class SpiderManager : MonoBehaviour
{
    [Header("Base Spider Prefabs")]
    public GameObject[] baseSpiders;

    // Nested dictionary: baseSpider -> cloneID -> clone
    private Dictionary<GameObject, Dictionary<int, GameObject>> spiderClones = new Dictionary<GameObject, Dictionary<int, GameObject>>();

    // Keep track of next clone ID per base spider
    private Dictionary<GameObject, int> nextCloneID = new Dictionary<GameObject, int>();

    void Start()
    {
        // Initialize dictionaries for each base spider
        foreach (GameObject spider in baseSpiders)
        {
            spiderClones[spider] = new Dictionary<int, GameObject>();
            nextCloneID[spider] = 0;
        }
    }

    // Spawn clones for a specific base spider
    public void SpawnClones(GameObject baseSpider, int amount, Vector3 startPosition, Vector3 offset)
    {
        if (!spiderClones.ContainsKey(baseSpider))
        {
            Debug.LogError("Base spider not registered!");
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            Vector3 position = startPosition + offset * i;
            GameObject clone = Instantiate(baseSpider, position, baseSpider.transform.rotation);

            // Different size spiders
            float minScale = 0.3f;
            float maxScale = 1.6f;
            float randomScale = Random.Range(minScale, maxScale);
            clone.transform.localScale = baseSpider.transform.localScale * randomScale;

            int id = nextCloneID[baseSpider]++;
            spiderClones[baseSpider].Add(id, clone);

            Debug.Log($"Spawned clone ID {id} of spider {baseSpider.name}");
        }
    }

    // Access clones of a specific spider
    public Dictionary<int, GameObject> GetClones(GameObject baseSpider)
    {
        if (spiderClones.TryGetValue(baseSpider, out var clones))
            return clones;

        return null;
    }

    // Destroy all clones of a specific spider
    public void DestroyClones(GameObject baseSpider)
    {
        if (!spiderClones.ContainsKey(baseSpider)) return;

        foreach (var clone in spiderClones[baseSpider].Values)
        {
            Destroy(clone);
        }

        spiderClones[baseSpider].Clear();
        nextCloneID[baseSpider] = 0;
    }

    // Destroy all clones of all spiders
    public void DestroyAllClones()
    {
        foreach (var spider in baseSpiders)
        {
            DestroyClones(spider);
        }
    }
}
