using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpiderManager : MonoBehaviour
{
    [Header("Base Spider Prefabs")]
    public GameObject[] baseSpiders;
    public GameObject[] threeBaseSpiders;

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

            // Different size spiders (10%)
            float minScale = 0.90f;
            float maxScale = 1.10f;
            float randomScale = Random.Range(minScale, maxScale);
            clone.transform.localScale = baseSpider.transform.localScale * randomScale;

            int id = nextCloneID[baseSpider]++;
            spiderClones[baseSpider].Add(id, clone);

            //Debug.Log($"Spawned clone ID {id} of spider {baseSpider.name}");
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

    // Hide all base spiders
    public void HideBaseSpiders()
    {
        foreach (GameObject spider in baseSpiders)
        {
            if (spider != null)
            {
                spider.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    // Show all base spiders
    public void ShowBaseSpiders()
    {
        foreach (GameObject spider in baseSpiders)
        {
            if (spider != null)
            {
                spider.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    public void ShowThreeBaseSpiders()
    {
        foreach (GameObject spider in threeBaseSpiders)
        {
            if (spider != null)
            {
                spider.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    public int numberOfLiveSpiders()
    {
        int total = 0;

        foreach (var entry in spiderClones.Values)
        {
            total += entry.Count;
        }

        return total;
    }

    public void RemoveSpiderCloneFromList(GameObject clone)
    {
        // Find which outer key contains this clone
        GameObject spiderKeyToRemove = null;
        int cloneIdToRemove = -1;

        foreach (var kvp in spiderClones)
        {
            foreach (var inner in kvp.Value)
            {
                if (inner.Value == clone)
                {
                    spiderKeyToRemove = kvp.Key;
                    cloneIdToRemove = inner.Key;
                    break;
                }
            }
            if (spiderKeyToRemove != null)
                break;
        }

        // If not found, nothing to remove
        if (spiderKeyToRemove == null)
            return;

        // Remove the clone from the inner dictionary
        spiderClones[spiderKeyToRemove].Remove(cloneIdToRemove);

        // Remove the spider entry entirely
        if (spiderClones[spiderKeyToRemove].Count == 0)
        {
            spiderClones.Remove(spiderKeyToRemove);
        }
    }

}
