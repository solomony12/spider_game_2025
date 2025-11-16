using UnityEngine;

public class BouncyBallSpawner : MonoBehaviour
{
    [Header("Ball Settings")]
    public GameObject ballPrefab;
    public float throwForce = 10f;
    public float spawnDistance = 2f;

    private GameObject spawnedBall;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (spawnedBall == null)
            {
                SpawnBall();
            }
            else
            {
                DestroyBall();
            }
        }
    }

    void SpawnBall()
    {
        // Determine spawn position in front of camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No Main Camera found!");
            return;
        }

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * spawnDistance;

        // Instantiate ball
        spawnedBall = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

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
    }

    void DestroyBall()
    {
        if (spawnedBall != null)
        {
            Destroy(spawnedBall);
            spawnedBall = null;
        }
    }
}
