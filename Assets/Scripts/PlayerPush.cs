using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    public float pushForce = 0.5f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb != null && !rb.isKinematic)
        {
            Vector3 pushDir = hit.moveDirection;
            pushDir.y = 0f;

            rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
        }
    }
}
