using UnityEngine;

public class ShrinkingRoom : MonoBehaviour
{
    [Header("Room Walls")]
    public Transform wall1; // Wall
    public Transform wall2; // Window
    public Transform wall3; // Wall 1
    public Transform wall4; // Door

    [Header("Furniture")]
    public Transform toilet; // attached to wall1
    public Transform bed; // corner between wall1 and wall2
    public Transform table; // attached to wall3

    [Header("Shrink Settings")]
    public float shrinkAmount = 0.01f;    // fraction toward center per shrink
    public float shrinkDuration = 3f;     // seconds for smooth transition

    [Header("Room Size Limits")]
    public float minRoomWidth = 6f;         // min distance between wall1-wall3
    public float minRoomLength = 9f;        // min distance between wall2-wall4

    private bool isShrinking = false;
    private float t = 0f;

    private Vector3[] wallsStart = new Vector3[4];
    private Vector3[] wallsTarget = new Vector3[4];

    private Vector3 toiletStart, bedStart, tableStart;
    private Vector3 toiletTarget, bedTarget, tableTarget;

    public void ShrinkRoom()
    {
        if (isShrinking) return;

        // Calculate current room dimensions
        float width = Vector3.Distance(wall1.position, wall3.position);
        float length = Vector3.Distance(wall2.position, wall4.position);

        if (width <= minRoomWidth || length <= minRoomLength)
        {
            Debug.Log("Room is already at minimum size!");
            return;
        }

        // Save starting positions
        wallsStart[0] = wall1.position;
        wallsStart[1] = wall2.position;
        wallsStart[2] = wall3.position;
        wallsStart[3] = wall4.position;

        toiletStart = toilet.position;
        bedStart = bed.position;
        tableStart = table.position;

        // Compute room center
        Vector3 center = (wall1.position + wall2.position + wall3.position + wall4.position) / 4f;

        // Compute target positions for walls
        for (int i = 0; i < 4; i++)
            wallsTarget[i] = wallsStart[i] + (center - wallsStart[i]) * shrinkAmount;

        // Anchor furniture to their walls/corners
        // Toilet follows wall1 along its normal
        Vector3 wall1Dir = (wallsTarget[0] - wallsStart[0]);
        toiletTarget = toilet.position + wall1Dir;

        // Bed follows corner between wall1 and wall2
        Vector3 cornerMove = ((wallsTarget[0] - wallsStart[0]) + (wallsTarget[1] - wallsStart[1])) / 2f;
        bedTarget = bed.position + cornerMove;

        // Table follows wall3
        Vector3 wall3Dir = (wallsTarget[2] - wallsStart[2]);
        tableTarget = table.position + wall3Dir;

        // Start shrinking
        t = 0f;
        isShrinking = true;
    }

    void Update()
    {
        if (!isShrinking) return;

        t += Time.deltaTime / shrinkDuration;

        // Move walls
        wall1.position = Vector3.Lerp(wallsStart[0], wallsTarget[0], t);
        wall2.position = Vector3.Lerp(wallsStart[1], wallsTarget[1], t);
        wall3.position = Vector3.Lerp(wallsStart[2], wallsTarget[2], t);
        wall4.position = Vector3.Lerp(wallsStart[3], wallsTarget[3], t);

        // Move anchored furniture
        toilet.position = Vector3.Lerp(toiletStart, toiletTarget, t);
        bed.position = Vector3.Lerp(bedStart, bedTarget, t);
        table.position = Vector3.Lerp(tableStart, tableTarget, t);

        if (t >= 1f)
            isShrinking = false;
    }
}
