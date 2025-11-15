using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class SpiderAI : MonoBehaviour
{
    public enum SpiderPlane { Floor, Wall, Ceiling }
    public enum AIState { Idle, Wander, Attack, Flee }
    public AIState currentState = AIState.Idle;
    private Coroutine currentStateCoroutine = null;
    private bool isIdle = true;
    private bool isWandering = false;

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
    public float idleMin = 1f;
    public float idleMax = 4f;
    private float stateTimer = 0f; // Counts down the remaining time for idle/wander

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

        stateTimer = Random.Range(idleMin, idleMax);
        currentState = AIState.Idle;
        isIdle = true;
    }

    void Start()
    {

        lastPosition = rb.position;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Flee
        if (distanceToPlayer < fleeDistance)
        {
            if (currentState != AIState.Flee)
                SetState(AIState.Flee);
            return;
        }

        // Attack if close but not fleeing
        if (distanceToPlayer < attackDistance && Random.value < attackChance)
        {
            if (currentState != AIState.Attack && currentState != AIState.Flee)
                SetState(AIState.Attack);
            return;
        }

        // Otherwise idle
        if (currentState != AIState.Idle && currentState != AIState.Wander)
            SetState(AIState.Idle);
    }

    void FixedUpdate()
    {
        HandleMovement();
        CheckIfStuck();
    }


    void SetState(AIState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        // Stop previous state coroutine
        if (currentStateCoroutine != null)
            StopCoroutine(currentStateCoroutine);

        switch (newState)
        {
            case AIState.Idle:
                currentStateCoroutine = StartCoroutine(IdleState());
                break;
            case AIState.Wander:
                currentStateCoroutine = StartCoroutine(WanderState());
                break;
            case AIState.Flee:
                currentStateCoroutine = StartCoroutine(FleeState());
                break;
            case AIState.Attack:
                currentStateCoroutine = StartCoroutine(AttackState());
                break;
        }
    }

    IEnumerator IdleState()
    {
        isMoving = false;
        stateTimer = Random.Range(idleMin, idleMax);

        while (stateTimer > 0f)
        {
            // Decrease timer each frame
            stateTimer -= Time.deltaTime;

            // Check for flee priority
            if (Vector3.Distance(transform.position, player.position) < fleeDistance)
            {
                SetState(AIState.Flee);
                yield break; // Stop idle coroutine
            }

            yield return null; // Wait for next frame
        }

        // Idle finished, switch to wander
        SetState(AIState.Wander);
    }

    IEnumerator WanderState()
    {
        isMoving = true;
        PickRandomLocation();
        stateTimer = Random.Range(idleMin, idleMax);

        while (stateTimer > 0f)
        {
            stateTimer -= Time.deltaTime;

            // Check for flee
            if (Vector3.Distance(transform.position, player.position) < fleeDistance)
            {
                SetState(AIState.Flee);
                yield break;
            }

            yield return null;
        }

        // Wander finished, switch to idle
        SetState(AIState.Idle);
    }

    void PickRandomLocation()
    {
        Vector3 randomDir = Random.insideUnitSphere;
        randomDir = Vector3.ProjectOnPlane(randomDir, planeNormal);
        targetPosition = GetSurfaceBoundedPosition(rb.position + randomDir * wanderRadius);
    }

    IEnumerator FleeState()
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, player.position) < fleeDistance)
        {
            Vector3 fleeDir = (transform.position - player.position).normalized;
            fleeDir = Vector3.ProjectOnPlane(fleeDir, planeNormal).normalized;

            rb.MovePosition(rb.position + fleeDir * runSpeed * Time.deltaTime);

            if (fleeDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(fleeDir, planeNormal);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 5f));
            }

            yield return null;
        }

        // Player is far enough, go back to idle
        SetState(AIState.Idle);
    }

    IEnumerator AttackState()
    {
        isMoving = true;
        targetPosition = GetSurfaceBoundedPosition(player.position);

        float attackDuration = 0.5f;
        float timer = 0f;

        while (timer < attackDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        SetState(AIState.Idle);
    }

    void HandleMovement()
    {
        if (!isMoving) return;

        Vector3 direction = (targetPosition - transform.position);
        direction = Vector3.ProjectOnPlane(direction, planeNormal).normalized;

        float distance = Vector3.Distance(rb.position, targetPosition);
        float speed = (currentState == AIState.Flee || distance < attackDistance) ? runSpeed : walkSpeed;

        Vector3 movement = direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, planeNormal);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f));
        }

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
            // Reset timer if moving
            stuckTimer = 0f;
        }

        lastPosition = rb.position;
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
