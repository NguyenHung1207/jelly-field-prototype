using System.Collections.Generic;
using UnityEngine;

public class PieceView : MonoBehaviour
{
    private readonly Dictionary<MiniSlot, Renderer> miniCubeRenderers = new Dictionary<MiniSlot, Renderer>();
    private readonly Dictionary<MiniSlot, Renderer> highlightRenderers = new Dictionary<MiniSlot, Renderer>();

    public PieceData Data { get; private set; }

    public void Build(PieceData data)
    {
        Data = data;

        ClearChildren();

        CreateMiniJellyBlock(MiniSlot.TopLeft, new Vector3(-0.25f, 0f, 0.25f));
        CreateMiniJellyBlock(MiniSlot.TopRight, new Vector3(0.25f, 0f, 0.25f));
        CreateMiniJellyBlock(MiniSlot.BottomLeft, new Vector3(-0.25f, 0f, -0.25f));
        CreateMiniJellyBlock(MiniSlot.BottomRight, new Vector3(0.25f, 0f, -0.25f));

        EnsureRootCollider();
        Refresh();
    }

    public void Refresh()
    {
        foreach (KeyValuePair<MiniSlot, Renderer> pair in miniCubeRenderers)
        {
            MiniSlot slot = pair.Key;
            ColorId colorId = Data.Get(slot);

            GameObject miniCubeObject = pair.Value.gameObject;
            bool hasColor = colorId != ColorId.None;

            miniCubeObject.SetActive(hasColor);

            if (highlightRenderers.TryGetValue(slot, out Renderer highlightRenderer))
            {
                highlightRenderer.gameObject.SetActive(hasColor);
            }

            if (hasColor)
            {
                pair.Value.sharedMaterial = RuntimeMaterialLibrary.Get(colorId);

                if (highlightRenderers.TryGetValue(slot, out Renderer highlight))
                {
                    highlight.sharedMaterial = RuntimeMaterialLibrary.GetHighlight();
                }
            }
        }
    }

    private void CreateMiniJellyBlock(MiniSlot slot, Vector3 localPosition)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = slot.ToString();

        cube.transform.SetParent(transform, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localScale = new Vector3(0.48f, 0.45f, 0.48f);

        Collider childCollider = cube.GetComponent<Collider>();
        if (childCollider != null)
        {
            Destroy(childCollider);
        }

        Renderer renderer = cube.GetComponent<Renderer>();
        miniCubeRenderers[slot] = renderer;

        CreateHighlight(slot, localPosition);
    }

    private void CreateHighlight(MiniSlot slot, Vector3 blockLocalPosition)
    {
        GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        highlight.name = $"{slot}_Highlight";

        highlight.transform.SetParent(transform, false);

        Vector3 highlightOffset = new Vector3(-0.12f, 0.24f, -0.12f);
        highlight.transform.localPosition = blockLocalPosition + highlightOffset;

        highlight.transform.localScale = new Vector3(0.08f, 0.012f, 0.035f);
        highlight.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);

        Collider collider = highlight.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = highlight.GetComponent<Renderer>();
        renderer.sharedMaterial = RuntimeMaterialLibrary.GetHighlight();

        highlightRenderers[slot] = renderer;
    }

    private void EnsureRootCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.center = Vector3.zero;
        boxCollider.size = new Vector3(1f, 0.65f, 1f);
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        miniCubeRenderers.Clear();
        highlightRenderers.Clear();
    }
}