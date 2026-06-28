using System.Collections.Generic;
using UnityEngine;

public static class RuntimeMaterialLibrary
{
    private static readonly Dictionary<ColorId, Material> Cache = new Dictionary<ColorId, Material>();
    private static Material highlightMaterial;

    public static Material Get(ColorId colorId)
    {
        if (Cache.TryGetValue(colorId, out Material material))
        {
            return material;
        }

        Material newMaterial = CreateJellyMaterial(colorId);
        Cache[colorId] = newMaterial;
        return newMaterial;
    }

    public static Material GetHighlight()
    {
        if (highlightMaterial != null)
        {
            return highlightMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        highlightMaterial = new Material(shader);
        highlightMaterial.name = "Runtime_Jelly_Highlight";

        Color highlightColor = new Color(1f, 1f, 1f, 0.9f);

        if (highlightMaterial.HasProperty("_BaseColor"))
        {
            highlightMaterial.SetColor("_BaseColor", highlightColor);
        }
        else
        {
            highlightMaterial.color = highlightColor;
        }

        SetMaterialFloatIfExists(highlightMaterial, "_Smoothness", 0.95f);
        SetMaterialFloatIfExists(highlightMaterial, "_Metallic", 0f);

        return highlightMaterial;
    }

    private static Material CreateJellyMaterial(ColorId colorId)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = $"Runtime_{colorId}_JellyMaterial";

        Color color = colorId switch
        {
            ColorId.Yellow => new Color(1f, 0.82f, 0.05f),
            ColorId.Green => new Color(0.08f, 0.85f, 0.25f),
            ColorId.Red => new Color(0.95f, 0.12f, 0.1f),
            ColorId.Purple => new Color(0.58f, 0.12f, 1f),
            ColorId.Pink => new Color(1f, 0.25f, 0.78f),
            ColorId.Blue => new Color(0.08f, 0.75f, 1f),
            _ => new Color(0.25f, 0.25f, 0.28f)
        };

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else
        {
            material.color = color;
        }

        SetMaterialFloatIfExists(material, "_Smoothness", 0.85f);
        SetMaterialFloatIfExists(material, "_Metallic", 0f);

        return material;
    }

    private static void SetMaterialFloatIfExists(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }
}