using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivitySliderController : MonoBehaviour
{
    public Slider sensitivitySlider;
    public MouseSensitivityData mouseSettings;

    void Start()
    {
        if (mouseSettings == null || sensitivitySlider == null)
            return;

        float normalizedValue = (mouseSettings.CurrentSensitivity - mouseSettings.minSensitivity) /
                                (mouseSettings.maxSensitivity - mouseSettings.minSensitivity);
        sensitivitySlider.value = normalizedValue;

        sensitivitySlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float value)
    {
        if (mouseSettings != null)
        {
            mouseSettings.SetNormalizedSensitivity(value);
        }
    }
}
