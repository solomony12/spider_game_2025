using UnityEngine;

[CreateAssetMenu(fileName = "MouseSensitivityData", menuName = "Settings/Mouse Sensitivity")]
public class MouseSensitivityData : ScriptableObject
{
    [Header("Mouse Sensitivity Settings")]
    [Tooltip("Minimum sensitivity value.")]
    public float minSensitivity = 100f;

    [Tooltip("Maximum sensitivity value.")]
    public float maxSensitivity = 300f;

    [Tooltip("Current sensitivity, clamped between min and max.")]
    [SerializeField] private float currentSensitivity = 200f;
    public float CurrentSensitivity
    {
        get => currentSensitivity;
        set => currentSensitivity = Mathf.Clamp(value, minSensitivity, maxSensitivity);
    }

    public void SetNormalizedSensitivity(float t)
    {
        CurrentSensitivity = Mathf.Lerp(minSensitivity, maxSensitivity, Mathf.Clamp01(t));
    }
}
