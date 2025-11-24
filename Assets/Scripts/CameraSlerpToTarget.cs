using System;
using UnityEngine;

public class CameraSlerpToTarget : MonoBehaviour
{
    public PlayerMovement player;
    public Transform target;
    public float rotateSpeed = 5f;
    public bool rotate = false;

    private Quaternion targetRotation;
    private Quaternion targetCameraRotation;

    void Update()
    {
        if (!rotate || target == null)
            return;

        // Disable player looking around
        player.enabled = false;

        // Player body rotation
        Vector3 direction = (target.position - player.transform.position).normalized;
        direction.y = 0f; // Keep horizontal only for body
        targetRotation = Quaternion.LookRotation(direction);
        player.transform.rotation = Quaternion.Slerp(
            player.transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );

        // amera pitch rotation
        Vector3 camDir = (target.position - player.playerCamera.position).normalized;
        targetCameraRotation = Quaternion.LookRotation(camDir);

        player.playerCamera.rotation = Quaternion.Slerp(
            player.playerCamera.rotation,
            targetCameraRotation,
            rotateSpeed * Time.deltaTime
        );
    }

    public void StartRotate(Transform t)
    {
        target = t;
        rotate = true;
    }

    public void StopRotate()
    {
        rotate = false;
        player.enabled = true;
    }
}
