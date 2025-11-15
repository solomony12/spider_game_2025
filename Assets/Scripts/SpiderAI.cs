using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class SpiderAI : MonoBehaviour
{
    public enum SpiderPlane { Floor, Wall, Ceiling }

    [Header("Plane Settings")]
    public SpiderPlane plane = SpiderPlane.Floor;
    private Vector3 planeNormal;

    [Header("Surface Bounds")]
    public Collider surfaceBounds;  // The plane the spider crawls on
    public float edgeMargin = 0.65f; // Distance from edges to avoid

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float wanderRadius = 3f;

    [Header("Player Interaction")]
    public Transform player;
    public float fleeDistance = 0.5f;
    public float attackDistance = 1.5f;
    [Range(0f, 1f)] public float attackChance = 0.2f;

    [Header("Timing")]
    public float stuckCheckDelay = 2f;  // Delay before picking new target if stuck
    public float stuckThreshold = 0.05f; // Distance threshold to consider "stuck"

    private Rigidbody rb;
    private Vector3 targetPosition;
    private bool isMoving = false;

    private Vector3 lastPosition;
    private float stuckTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Keep upright

        // Set plane normal
        switch (plane)
        {
            case SpiderPlane.Floor: planeNormal = Vector3.up; break;
            case SpiderPlane.Ceiling: planeNormal = Vector3.down; break;
            case SpiderPlane.Wall: planeNormal = Vector3.right; break;
        }
    }

    void Start()
    {
        lastPosition = rb.position;
        StartCoroutine(AIBehavior());
    }

    void FixedUpdate()
    {
        Move();
        CheckIfStuck();
    }

    void Move()
    {
        if (!isMoving) return;

        Vector3 direction = (targetPosition - transform.position);
        direction = Vector3.ProjectOnPlane(direction, planeNormal).normalized;

        float distance = Vector3.Distance(rb.position, targetPosition);
        float speed = distance > attackDistance ? walkSpeed : runSpeed;

        Vector3 movement = direction * speed * Time.fixedDeltaTime;

        // Move with Rigidbody to respect collisions
        rb.MovePosition(rb.position + movement);

        // Smooth rotation along plane
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, planeNormal);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f));
        }

        // Stop when close enough
        if (distance < 0.1f)
            isMoving = false;
    }

    // Checks if spider is stuck and picks a new target if necessary
    void CheckIfStuck()
    {
        if (!isMoving) return;

        float distanceMoved = Vector3.Distance(rb.position, lastPosition);
        if (distanceMoved < stuckThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckCheckDelay)
            {
                // Pick a new random target
                targetPosition = GetSurfaceBoundedPosition(rb.position + Random.insideUnitSphere * wanderRadius);
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f; // Reset timer if moving
        }

        lastPosition = rb.position;
    }

    IEnumerator AIBehavior()
    {
        while (true)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Flee if player is too close
            if (distanceToPlayer < fleeDistance)
            {
                Vector3 fleeDir = (transform.position - player.position).normalized;
                fleeDir = Vector3.ProjectOnPlane(fleeDir, planeNormal);
                targetPosition = GetSurfaceBoundedPosition(rb.position + fleeDir * wanderRadius);
                isMoving = true;
                yield return new WaitForSeconds(Random.Range(2f, 10f));
            }
            // Chance to charge player
            else if (distanceToPlayer < attackDistance && Random.value < attackChance)
            {
                targetPosition = GetSurfaceBoundedPosition(player.position);
                isMoving = true;
                yield return new WaitForSeconds(Random.Range(2f, 10f));
            }
            else
            {
                // Random wandering
                if (!isMoving)
                {
                    Vector3 randomDir = Random.insideUnitSphere;
                    randomDir = Vector3.ProjectOnPlane(randomDir, planeNormal);
                    targetPosition = GetSurfaceBoundedPosition(rb.position + randomDir * wanderRadius);
                    isMoving = true;
                }
                yield return new WaitForSeconds(Random.Range(2f, 10f));
            }

            yield return null;
        }
    }

    // Clamps a position to be within the surface bounds with a margin
    Vector3 GetSurfaceBoundedPosition(Vector3 pos)
    {
        if (surfaceBounds == null)
            return pos;

        Vector3 min = surfaceBounds.bounds.min + Vector3.one * edgeMargin;
        Vector3 max = surfaceBounds.bounds.max - Vector3.one * edgeMargin;

        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.y = Mathf.Clamp(pos.y, min.y, max.y);
        pos.z = Mathf.Clamp(pos.z, min.z, max.z);

        // Project onto plane to ensure it stays on floor/wall/ceiling
        pos = Vector3.ProjectOnPlane(pos - rb.position, planeNormal) + rb.position;

        return pos;
    }

    // Show behavior in Editor
    void OnDrawGizmosSelected()
    {
        // Wander radius (green)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        // Flee distance (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);

        // Attack distance (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        // Show edge margin on surface
        if (surfaceBounds != null)
        {
            Gizmos.color = Color.cyan; // Edge margin color

            Vector3 min = surfaceBounds.bounds.min + Vector3.one * edgeMargin;
            Vector3 max = surfaceBounds.bounds.max - Vector3.one * edgeMargin;
            Vector3 center = (min + max) / 2f;
            Vector3 size = max - min;

            Gizmos.DrawWireCube(center, size);
        }
    }
}
