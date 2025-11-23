using System;
using UnityEngine;

public class PaperAirplaneSpawner : MonoBehaviour
{
    [Header("Airplane Settings")]
    public GameObject airplanePrefab;
    public GameObject spiderPrefab;
    public float throwForce = 12f;
    public float spawnDistance = 2f;

    private GameObject spawnedPlane;

    public LevelManager levelManager;

    public static event Action OnAirplaneEndingReached;
    private static int planesThrown = 0;

    void Update()
    {
        // Use Z for paper plane (starting Day 6)
        if (Input.GetKeyDown(KeyCode.Z) && LevelManager.getCurrentDay() > 5 && !levelManager.getGameReachedEndingBool())
        {
            if (spawnedPlane == null)
            {
                planesThrown++;
                SpawnAirplane();
                CheckPlanesThrownCount();
            }
            else
            {
                DestroyAirplane();
            }
        }
    }

    public GameObject SpawnAirplane(bool noSpider = false)
    {
        Camera cam = Camera.main;

        Vector3 spawnPos = cam.transform.position
                         + cam.transform.forward * spawnDistance;

        // 5% chance to spawn spider instead
        int spiderChance = UnityEngine.Random.Range(1, 100);
        if (!noSpider && spiderChance <= 5)
        {
            spawnedPlane = Instantiate(spiderPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            spawnedPlane = Instantiate(airplanePrefab, spawnPos, Quaternion.identity);
        }

        Rigidbody rb = spawnedPlane.GetComponent<Rigidbody>();
        if (rb == null) rb = spawnedPlane.AddComponent<Rigidbody>();

        rb.useGravity = true;
        rb.mass = 0.05f;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.3f;

        // Add gliding behavior
        if (spawnedPlane.GetComponent<PaperAirplaneGlide>() == null)
            spawnedPlane.AddComponent<PaperAirplaneGlide>();

        // Throw forward
        rb.linearVelocity = cam.transform.forward * throwForce;

        return spawnedPlane;
    }

    public void DestroyAirplane()
    {
        if (spawnedPlane != null)
        {
            Destroy(spawnedPlane);
            spawnedPlane = null;
        }
    }

    public void ResetPlanesThrownCount()
    {
        planesThrown = 0;
    }

    public void CheckPlanesThrownCount()
    {
        if (planesThrown >= 75 && LevelManager.getCurrentDay() > 5)
        {
            OnAirplaneEndingReached?.Invoke();
        }
    }
}
