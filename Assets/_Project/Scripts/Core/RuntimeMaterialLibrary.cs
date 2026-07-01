using System.Collections.Generic;
using UnityEngine;

public static class RuntimeMaterialLibrary
{
    private static readonly Dictionary<ColorId, Material> materials = new Dictionary<ColorId, Material>();

    public static Material Get(ColorId colorId)
    {
        if (materials.ContainsKey(colorId))
        {
            return materials[colorId];
        }

        Color color = GetColor(colorId);
        Material material = MobileSafeMaterial.Create($"Runtime_{colorId}", color);

        materials[colorId] = material;
        return material;
    }

    private static Color GetColor(ColorId colorId)
    {
        return colorId switch
        {
            ColorId.Yellow => new Color(1f, 0.82f, 0.05f, 1f),
            ColorId.Green => new Color(0.08f, 0.85f, 0.25f, 1f),
            ColorId.Red => new Color(0.95f, 0.12f, 0.1f, 1f),
            ColorId.Purple => new Color(0.58f, 0.12f, 1f, 1f),
            ColorId.Pink => new Color(1f, 0.25f, 0.78f, 1f),
            ColorId.Blue => new Color(0.08f, 0.75f, 1f, 1f),
            _ => new Color(0.8f, 0.8f, 0.8f, 1f)
        };
    }

    public static void Clear()
    {
        materials.Clear();
    }
}