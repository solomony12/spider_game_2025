using UnityEngine;

public class CameraRumble : MonoBehaviour
{
    [Header("Rumble Settings")]
    public float intensity = 0.1f;
    public float intensityGrowth = 0.1f;
    public float duration = 1f;

    private float shakeTimer = 0f;
    private float elapsed = 0f;
    private bool shaking = false;

    void Update()
    {
        if (shaking)
        {
            shakeTimer -= Time.deltaTime;
            elapsed += Time.deltaTime;

            if (shakeTimer > 0)
            {
                // Intensity increases over time
                float currentIntensity = intensity + (elapsed * intensityGrowth);

                float x = Random.Range(-currentIntensity, currentIntensity);
                float y = Random.Range(-currentIntensity, currentIntensity);
                float z = Random.Range(-currentIntensity, currentIntensity);

                Quaternion offset = Quaternion.Euler(x, y, z);
                transform.localRotation = transform.localRotation * offset;
            }
            else
            {
                shaking = false;
            }
        }
    }

    public void StartRumble(float customDuration = -1)
    {
        if (customDuration > 0)
            shakeTimer = customDuration;
        else
            shakeTimer = duration;

        elapsed = 0f;
        shaking = true;
    }
}
