using System.Collections;
using UnityEngine;

public class MoveToCamera : MonoBehaviour
{
    private Coroutine moveRoutine;
    public Camera camera;
    public GameObject self;

    /// <summary>
    /// Move & rotate the object toward the camera over 'duration' seconds.
    /// </summary>
    public void MoveTowardCamera(float duration)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveAndRotate(duration));
    }

    private IEnumerator MoveAndRotate(float duration)
    {
        Transform cam = camera.transform;

        Vector3 startPos = self.transform.position;
        Vector3 endPos = cam.position;
        Debug.Log("End Pos: " + endPos);

        Quaternion startRot = self.transform.rotation;
        Quaternion endRot = Quaternion.LookRotation(-(cam.position - self.transform.position));

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Move toward camera
            Debug.Log($"Before Lerp: {self.transform.position}");
            self.transform.position = Vector3.Lerp(startPos, endPos, t);
            Debug.Log($"After Lerp: {self.transform.position}");


            // Rotate to face camera
            self.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        // Snap to final state
        self.transform.position = endPos;
        self.transform.rotation = endRot;

        moveRoutine = null;
    }

    public void ResetPosition()
    {
        self.transform.position = new Vector3(10.45f, 1.73f, 10.72f);
        self.transform.rotation = Quaternion.identity;
    }
}
