using System;
using UnityEngine;

public class BouncyBallSpawner : MonoBehaviour
{
    [Header("Ball Settings")]
    public GameObject ballPrefab;
    public GameObject spider;
    public float throwForce = 10f;
    public float spawnDistance = 2f;

    private GameObject spawnedBall;

    public LevelManager levelManager;

    public static event Action OnBallEndingReached;
    private static int ballThrown = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && LevelManager.getCurrentDay() > 1 && !levelManager.getGameReachedEndingBool())
        {
            if (spawnedBall == null)
            {
                ballThrown++;
                SpawnBall();
                CheckBallThrownCount();
            }
            else
            {
                DestroyBall();
            }
        }
    }

    public GameObject SpawnBall(bool noSpider = false)
    {
        // Determine spawn position in front of camera
        Camera cam = Camera.main;

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * spawnDistance;

        // Instantiate ball (5% chance of spider)
        int spiderChance = UnityEngine.Random.Range(1, 100);
        if (noSpider || spiderChance > 5)
        {
            spawnedBall = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            spawnedBall = Instantiate(spider, spawnPos, Quaternion.identity);
        }


        // Ensure it has a Rigidbody
        Rigidbody rb = spawnedBall.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = spawnedBall.AddComponent<Rigidbody>();
        }

        // Make it bouncy
        Collider col = spawnedBall.GetComponent<Collider>();
        if (col == null)
        {
            col = spawnedBall.AddComponent<SphereCollider>();
        }

        PhysicsMaterial bounceMat = new PhysicsMaterial();
        bounceMat.bounciness = 0.8f;
        bounceMat.frictionCombine = PhysicsMaterialCombine.Multiply;
        bounceMat.bounceCombine = PhysicsMaterialCombine.Maximum;
        col.material = bounceMat;

        // Add initial velocity away from camera
        rb.linearVelocity = cam.transform.forward * throwForce;

        return spawnedBall;
    }

    public void DestroyBall()
    {
        if (spawnedBall != null)
        {
            Destroy(spawnedBall);
            spawnedBall = null;
        }
    }

    public void ResetBallThrownCount()
    {
        ballThrown = 0;
    }

    public void CheckBallThrownCount()
    {
        if (ballThrown >= 50 && LevelManager.getCurrentDay() > 4)
        {
            OnBallEndingReached?.Invoke();
        }
    }
}
