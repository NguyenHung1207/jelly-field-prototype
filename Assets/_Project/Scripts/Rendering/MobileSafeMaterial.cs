using UnityEngine;

public static class MobileSafeMaterial
{
    public static Material Create(string materialName, Color color)
    {
        Shader shader = Shader.Find("Unlit/Color");

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader == null)
        {
            shader = Shader.Find("UI/Default");
        }

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            Debug.LogError($"No compatible shader found for material {materialName}.");
            return null;
        }

        Material material = new Material(shader);
        material.name = materialName;

        ApplyColor(material, color);

        return material;
    }

    public static void ApplyColor(Material material, Color color)
    {
        if (material == null)
            return;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        material.color = color;
    }
}