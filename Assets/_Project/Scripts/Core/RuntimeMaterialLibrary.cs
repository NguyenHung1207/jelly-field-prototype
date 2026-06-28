using System.Collections.Generic;
using UnityEngine;

public static class RuntimeMaterialLibrary
{
    private static readonly Dictionary<ColorId, Material> Cache = new Dictionary<ColorId, Material>();

    public static Material Get(ColorId colorId)
    {
        if (Cache.TryGetValue(colorId, out Material material))
        {
            return material;
        }

        Material newMaterial = CreateMaterial(colorId);
        Cache[colorId] = newMaterial;
        return newMaterial;
    }

    private static Material CreateMaterial(ColorId colorId)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = $"Runtime_{colorId}_Material";

        Color color = colorId switch
        {
            ColorId.Yellow => new Color(1f, 0.85f, 0.05f),
            ColorId.Green => new Color(0.1f, 0.8f, 0.25f),
            ColorId.Red => new Color(0.95f, 0.15f, 0.1f),
            ColorId.Purple => new Color(0.55f, 0.15f, 0.95f),
            ColorId.Pink => new Color(1f, 0.25f, 0.8f),
            ColorId.Blue => new Color(0.1f, 0.45f, 1f),
            _ => new Color(0.25f, 0.25f, 0.25f)
        };

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else
        {
            material.color = color;
        }

        return material;
    }
}