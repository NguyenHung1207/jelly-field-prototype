using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceView : MonoBehaviour
{
    private readonly Dictionary<MiniSlot, Renderer> miniCubeRenderers = new Dictionary<MiniSlot, Renderer>();
    private readonly Dictionary<MiniSlot, Transform> miniCubeTransforms = new Dictionary<MiniSlot, Transform>();
    private readonly Dictionary<MiniSlot, Renderer> highlightRenderers = new Dictionary<MiniSlot, Renderer>();

    public PieceData Data { get; private set; }

    public void Build(PieceData data)
    {
        Data = data;

        ClearChildren();

        CreateMiniJellyBlock(MiniSlot.TopLeft);
        CreateMiniJellyBlock(MiniSlot.TopRight);
        CreateMiniJellyBlock(MiniSlot.BottomLeft);
        CreateMiniJellyBlock(MiniSlot.BottomRight);

        EnsureRootCollider();
        Refresh();
    }

    public void Refresh()
    {
        foreach (MiniSlot slot in miniCubeRenderers.Keys)
        {
            ColorId colorId = Data.Get(slot);

            Transform blockTransform = miniCubeTransforms[slot];
            Renderer blockRenderer = miniCubeRenderers[slot];

            bool hasColor = colorId != ColorId.None;

            blockTransform.gameObject.SetActive(hasColor);

            if (highlightRenderers.TryGetValue(slot, out Renderer highlightRenderer))
            {
                highlightRenderer.gameObject.SetActive(hasColor);
            }

            if (!hasColor)
                continue;

            blockRenderer.sharedMaterial = RuntimeMaterialLibrary.Get(colorId);

            ApplyMergedLook(slot, colorId);

            if (highlightRenderers.TryGetValue(slot, out Renderer highlight))
            {
                highlight.sharedMaterial = RuntimeMaterialLibrary.GetHighlight();
                UpdateHighlightTransform(slot);
            }
        }
    }

    public void PlayMatchDisappearEffect(MiniSlot slot, ColorId color)
    {
        if (color == ColorId.None)
            return;

        if (!miniCubeTransforms.TryGetValue(slot, out Transform sourceTransform))
            return;

        GameObject ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ghost.name = $"MatchDisappear_{color}_{slot}";

        ghost.transform.position = sourceTransform.position;
        ghost.transform.rotation = sourceTransform.rotation;
        ghost.transform.localScale = sourceTransform.lossyScale;

        Collider ghostCollider = ghost.GetComponent<Collider>();
        if (ghostCollider != null)
        {
            Destroy(ghostCollider);
        }

        Renderer ghostRenderer = ghost.GetComponent<Renderer>();
        ghostRenderer.sharedMaterial = RuntimeMaterialLibrary.Get(color);

        MatchDisappearEffect effect = ghost.AddComponent<MatchDisappearEffect>();
        effect.Play();
    }

    public void PlayFillAnimations(List<FillMove> fillMoves)
    {
        if (fillMoves == null || fillMoves.Count == 0)
            return;

        foreach (FillMove move in fillMoves)
        {
            if (!miniCubeTransforms.ContainsKey(move.To))
                continue;

            StartCoroutine(PlayFillAnimationRoutine(move));
        }
    }

    private IEnumerator PlayFillAnimationRoutine(FillMove move)
    {
        if (!miniCubeTransforms.TryGetValue(move.To, out Transform targetTransform))
            yield break;

        if (!miniCubeTransforms.TryGetValue(move.From, out Transform sourceTransform))
            yield break;

        Renderer targetRenderer = miniCubeRenderers[move.To];

        Renderer targetHighlight = null;
        if (highlightRenderers.TryGetValue(move.To, out Renderer foundHighlight))
        {
            targetHighlight = foundHighlight;
        }

        Vector3 finalLocalPosition = targetTransform.localPosition;
        Vector3 finalLocalScale = targetTransform.localScale;

        Vector3 sourceLocalPosition = sourceTransform.localPosition;

        targetRenderer.enabled = false;

        if (targetHighlight != null)
        {
            targetHighlight.enabled = false;
        }

        GameObject fillGhost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fillGhost.name = $"FillGhost_{move.Color}_{move.From}_to_{move.To}";
        fillGhost.transform.SetParent(transform, false);

        Collider collider = fillGhost.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer ghostRenderer = fillGhost.GetComponent<Renderer>();
        ghostRenderer.sharedMaterial = RuntimeMaterialLibrary.Get(move.Color);

        bool horizontal = Mathf.Abs(sourceLocalPosition.x - finalLocalPosition.x) >
                          Mathf.Abs(sourceLocalPosition.z - finalLocalPosition.z);

        Vector3 startLocalPosition = (sourceLocalPosition + finalLocalPosition) * 0.5f;
        Vector3 startLocalScale = finalLocalScale;

        float distance;

        if (horizontal)
        {
            distance = Mathf.Abs(sourceLocalPosition.x - finalLocalPosition.x);
            startLocalScale.x = distance + finalLocalScale.x;
            startLocalScale.z = finalLocalScale.z;
        }
        else
        {
            distance = Mathf.Abs(sourceLocalPosition.z - finalLocalPosition.z);
            startLocalScale.z = distance + finalLocalScale.z;
            startLocalScale.x = finalLocalScale.x;
        }

        startLocalScale.y = finalLocalScale.y * 0.92f;

        fillGhost.transform.localPosition = startLocalPosition;
        fillGhost.transform.localScale = startLocalScale;

        float duration = 0.22f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            float overshoot = Mathf.Sin(t * Mathf.PI) * 0.08f;

            fillGhost.transform.localPosition = Vector3.LerpUnclamped(
                startLocalPosition,
                finalLocalPosition,
                easedT
            );

            fillGhost.transform.localScale = Vector3.LerpUnclamped(
                startLocalScale,
                finalLocalScale + new Vector3(overshoot, overshoot * 0.5f, overshoot),
                easedT
            );

            yield return null;
        }

        Destroy(fillGhost);

        targetRenderer.enabled = true;

        if (targetHighlight != null)
        {
            targetHighlight.enabled = true;
        }

        targetTransform.localPosition = finalLocalPosition;
        targetTransform.localScale = finalLocalScale;
    }

    private void CreateMiniJellyBlock(MiniSlot slot)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = slot.ToString();

        cube.transform.SetParent(transform, false);
        cube.transform.localPosition = PieceLayout.GetSlotLocalPosition(slot);
        cube.transform.localScale = new Vector3(PieceLayout.MiniWidth, PieceLayout.MiniHeight, PieceLayout.MiniDepth);

        Collider childCollider = cube.GetComponent<Collider>();
        if (childCollider != null)
        {
            Destroy(childCollider);
        }

        miniCubeTransforms[slot] = cube.transform;
        miniCubeRenderers[slot] = cube.GetComponent<Renderer>();

        CreateHighlight(slot);
    }

    private void CreateHighlight(MiniSlot slot)
    {
        GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        highlight.name = $"{slot}_Highlight";
        highlight.transform.SetParent(transform, false);

        Collider collider = highlight.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = highlight.GetComponent<Renderer>();
        renderer.sharedMaterial = RuntimeMaterialLibrary.GetHighlight();

        highlightRenderers[slot] = renderer;
    }

    private void ApplyMergedLook(MiniSlot slot, ColorId colorId)
    {
        Transform t = miniCubeTransforms[slot];

        float leftExtend = HasSameColor(GetLeftNeighbor(slot), colorId) ? PieceLayout.MergeOverlap : 0f;
        float rightExtend = HasSameColor(GetRightNeighbor(slot), colorId) ? PieceLayout.MergeOverlap : 0f;
        float upExtend = HasSameColor(GetTopNeighbor(slot), colorId) ? PieceLayout.MergeOverlap : 0f;
        float downExtend = HasSameColor(GetBottomNeighbor(slot), colorId) ? PieceLayout.MergeOverlap : 0f;

        Vector3 basePosition = PieceLayout.GetSlotLocalPosition(slot);

        float width = PieceLayout.MiniWidth + leftExtend + rightExtend;
        float depth = PieceLayout.MiniDepth + upExtend + downExtend;

        float shiftX = (rightExtend - leftExtend) * 0.5f;
        float shiftZ = (upExtend - downExtend) * 0.5f;

        t.localPosition = basePosition + new Vector3(shiftX, 0f, shiftZ);
        t.localScale = new Vector3(width, PieceLayout.MiniHeight, depth);
    }

    private void UpdateHighlightTransform(MiniSlot slot)
    {
        if (!miniCubeTransforms.TryGetValue(slot, out Transform blockTransform))
            return;

        if (!highlightRenderers.TryGetValue(slot, out Renderer highlightRenderer))
            return;

        Transform highlightTransform = highlightRenderer.transform;

        Vector3 blockPos = blockTransform.localPosition;
        Vector3 blockScale = blockTransform.localScale;

        highlightTransform.localPosition = blockPos + new Vector3(
            -blockScale.x * 0.18f,
            blockScale.y * 0.48f,
            -blockScale.z * 0.18f
        );

        highlightTransform.localScale = new Vector3(
            blockScale.x * 0.18f,
            blockScale.y * 0.05f,
            blockScale.z * 0.10f
        );

        highlightTransform.localRotation = Quaternion.Euler(0f, 0f, -25f);
    }

    private bool HasSameColor(MiniSlot? neighborSlot, ColorId colorId)
    {
        if (!neighborSlot.HasValue)
            return false;

        return Data.Get(neighborSlot.Value) == colorId;
    }

    private MiniSlot? GetLeftNeighbor(MiniSlot slot)
    {
        return slot switch
        {
            MiniSlot.TopRight => MiniSlot.TopLeft,
            MiniSlot.BottomRight => MiniSlot.BottomLeft,
            _ => null
        };
    }

    private MiniSlot? GetRightNeighbor(MiniSlot slot)
    {
        return slot switch
        {
            MiniSlot.TopLeft => MiniSlot.TopRight,
            MiniSlot.BottomLeft => MiniSlot.BottomRight,
            _ => null
        };
    }

    private MiniSlot? GetTopNeighbor(MiniSlot slot)
    {
        return slot switch
        {
            MiniSlot.BottomLeft => MiniSlot.TopLeft,
            MiniSlot.BottomRight => MiniSlot.TopRight,
            _ => null
        };
    }

    private MiniSlot? GetBottomNeighbor(MiniSlot slot)
    {
        return slot switch
        {
            MiniSlot.TopLeft => MiniSlot.BottomLeft,
            MiniSlot.TopRight => MiniSlot.BottomRight,
            _ => null
        };
    }

    private void EnsureRootCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.center = Vector3.zero;
        boxCollider.size = new Vector3(1f, 0.72f, 1f);
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        miniCubeRenderers.Clear();
        miniCubeTransforms.Clear();
        highlightRenderers.Clear();
    }
}