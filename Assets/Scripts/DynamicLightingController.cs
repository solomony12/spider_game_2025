using UnityEngine;

public class DynamicLightingController : MonoBehaviour
{
    public Light directionalLight;
    public Color morningColor = new Color(1f, 0.85f, 0.6f);
    public Color noonColor = Color.white;
    public Color eveningColor = new Color(1f, 0.45f, 0.3f);
    public Color nightColor = new Color(0.1f, 0.1f, 0.25f);

    public float transitionDuration = 4f;

    private Color startColor;
    private float startIntensity;
    private Color targetColor;
    private float targetIntensity;
    private float t = 0f;
    private bool isTransitioning = false;

    public float maxSunIntensity = 1.2f;
    public float skyboxMinExposure = 0.8f;
    public float skyboxMaxExposure = 1.3f;

    public Material morningSkybox;
    public Material noonSkybox;
    public Material eveningSkybox;
    public Material nightSkybox;

    public Light defaultLight;
    public Light darkLight;
    public Light exitLight;

    public enum TimePhase { Morning, Noon, Evening, Night }
    public TimePhase currentPhase = TimePhase.Morning;

    void Start()
    {
        ApplyPhaseSettings(currentPhase, true);
    }

    void Update()
    {
        if (isTransitioning)
        {
            t += Time.deltaTime / transitionDuration;
            t = Mathf.Clamp01(t);

            directionalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            directionalLight.color = Color.Lerp(startColor, targetColor, t);

            //UpdateSkyboxExposure();

            if (t >= 1f)
                isTransitioning = false;
        }
    }

    public void AdvancePhase()
    {
        currentPhase = (TimePhase)(((int)currentPhase + 1) % 4);
        ApplyPhaseSettings(currentPhase);
    }

    public void ApplyPhaseSettings(TimePhase phase, bool instant = false)
    {
        switch (phase)
        {
            case TimePhase.Morning:
                RenderSettings.skybox = morningSkybox;
                targetColor = morningColor;
                targetIntensity = 0.8f;
                break;

            case TimePhase.Noon:
                RenderSettings.skybox = noonSkybox;
                targetColor = noonColor;
                targetIntensity = 1.0f;
                break;

            case TimePhase.Evening:
                RenderSettings.skybox = eveningSkybox;
                targetColor = eveningColor;
                targetIntensity = 0.6f;
                break;

            case TimePhase.Night:
                RenderSettings.skybox = nightSkybox;
                targetColor = nightColor;
                targetIntensity = 0.25f;
                break;
        }

        if (instant)
        {
            directionalLight.color = targetColor;
            directionalLight.intensity = targetIntensity;

            t = 1f;
            isTransitioning = false;

            //UpdateSkyboxExposure();
        }
        else
        {
            startColor = directionalLight.color;
            startIntensity = directionalLight.intensity;
            t = 0f;
            isTransitioning = true;
        }

        // Re-sample skybox for ambient lighting
        //DynamicGI.UpdateEnvironment();
    }

    public void SetLightDefault()
    {
        defaultLight.enabled = true;
        darkLight.enabled = false;
        exitLight.enabled = false;
        RenderSettings.skybox = morningSkybox;

        DynamicGI.UpdateEnvironment();
    }

    public void SetLightDark()
    {
        defaultLight.enabled = false;
        darkLight.enabled = true;
        exitLight.enabled = true;
        RenderSettings.skybox = nightSkybox;

        DynamicGI.UpdateEnvironment();
    }

    private void UpdateSkyboxExposure()
    {
        float normalized = Mathf.Clamp01(directionalLight.intensity / maxSunIntensity);
        float exposure = Mathf.Lerp(skyboxMinExposure, skyboxMaxExposure, normalized);
        RenderSettings.skybox.SetFloat("_Exposure", exposure);
    }
}
