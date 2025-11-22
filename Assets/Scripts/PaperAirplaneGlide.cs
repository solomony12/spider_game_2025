using UnityEngine;

public class PaperAirplaneGlide : MonoBehaviour
{
    public float liftMultiplier = 0.15f;
    public float stability = 2f;
    public float gravityBoost = 0.2f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude > 0.1f)
        {
            float speed = velocity.magnitude;

            // Lift
            Vector3 lift = Vector3.up * (speed * liftMultiplier);
            rb.AddForce(lift, ForceMode.Force);

            // Dip down
            rb.AddForce(Vector3.down * gravityBoost, ForceMode.Force);

            // Orientation Stability
            Vector3 forwardDir = velocity.normalized;
            Vector3 targetForward = new Vector3(forwardDir.x, Mathf.Min(forwardDir.y, 0f), forwardDir.z);

            Quaternion targetRotation = Quaternion.LookRotation(targetForward);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * stability));
        }
    }
}
