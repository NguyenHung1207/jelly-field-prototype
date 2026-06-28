using System.Collections.Generic;
using UnityEngine;

public class PieceView : MonoBehaviour
{
    private readonly Dictionary<MiniSlot, Renderer> miniCubeRenderers = new Dictionary<MiniSlot, Renderer>();

    public PieceData Data { get; private set; }

    public void Build(PieceData data)
    {
        Data = data;

        ClearChildren();

        CreateMiniCube(MiniSlot.TopLeft, new Vector3(-0.25f, 0f, 0.25f));
        CreateMiniCube(MiniSlot.TopRight, new Vector3(0.25f, 0f, 0.25f));
        CreateMiniCube(MiniSlot.BottomLeft, new Vector3(-0.25f, 0f, -0.25f));
        CreateMiniCube(MiniSlot.BottomRight, new Vector3(0.25f, 0f, -0.25f));

        EnsureRootCollider();
        Refresh();
    }

    public void Refresh()
    {
        foreach (KeyValuePair<MiniSlot, Renderer> pair in miniCubeRenderers)
        {
            ColorId colorId = Data.Get(pair.Key);

            GameObject miniCubeObject = pair.Value.gameObject;
            miniCubeObject.SetActive(colorId != ColorId.None);

            if (colorId != ColorId.None)
            {
                pair.Value.sharedMaterial = RuntimeMaterialLibrary.Get(colorId);
            }
        }
    }

    private void CreateMiniCube(MiniSlot slot, Vector3 localPosition)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = slot.ToString();

        cube.transform.SetParent(transform, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localScale = new Vector3(0.48f, 0.25f, 0.48f);

        Collider childCollider = cube.GetComponent<Collider>();
        if (childCollider != null)
        {
            Destroy(childCollider);
        }

        Renderer renderer = cube.GetComponent<Renderer>();
        miniCubeRenderers[slot] = renderer;
    }

    private void EnsureRootCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.center = Vector3.zero;
        boxCollider.size = new Vector3(1f, 0.5f, 1f);
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        miniCubeRenderers.Clear();
    }
}