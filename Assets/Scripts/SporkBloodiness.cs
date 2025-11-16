using UnityEngine;

public class SporkBloodiness : MonoBehaviour
{
    [Header("Assign Target GameObject")]
    public GameObject targetObject;

    [Header("Materials (Base)")]
    public Material[] baseMaterials = new Material[4];

    [Header("Base Maps (Albedo)")]
    public Texture[] baseMaps = new Texture[4];

    [Header("Extra Textures (e.g., Metallic/Detail)")]
    public Texture[] extraTextures = new Texture[4];

    // Runtime copies of materials
    private Material[] runtimeMaterials = new Material[4];

    void Start()
    {
        // Make safe runtime copies of the materials
        for (int i = 0; i < baseMaterials.Length; i++)
        {
            if (baseMaterials[i] != null)
                runtimeMaterials[i] = new Material(baseMaterials[i]);
        }
    }

    public void SelectMaterial(int index)
    {
        if (index < 0 || index >= runtimeMaterials.Length)
        {
            Debug.LogError("Index must be 0–" + (runtimeMaterials.Length - 1));
            return;
        }

        if (targetObject == null)
        {
            Debug.LogError("Target Object is not assigned!");
            return;
        }

        Renderer rend = targetObject.GetComponent<Renderer>();

        if (rend == null)
        {
            Debug.LogError("Target object has no Renderer!");
            return;
        }

        Material mat = runtimeMaterials[index];

        // Apply Base Map 
        if (mat != null && baseMaps[index] != null)
        {
            mat.SetTexture("_BaseMap", baseMaps[index]);
            mat.mainTexture = baseMaps[index];
        }

        // Apply Metallic (Roughness)
        if (mat != null && extraTextures[index] != null)
        {
            mat.SetTexture("_MetallicGlossMap", extraTextures[index]);
        }

        // Assign the material to the object
        rend.material = mat;

        Debug.Log("Applied material " + index + " with base map and extra texture");
    }
}
