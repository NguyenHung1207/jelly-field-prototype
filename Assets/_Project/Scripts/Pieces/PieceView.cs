using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceView : MonoBehaviour
{
    private readonly List<GameObject> visualBlocks = new List<GameObject>();

    private GameObject miniBlockPrefab;

    public PieceData Data { get; private set; }

    public void Build(PieceData data, GameObject miniBlockPrefab = null)
    {
        Data = data;
        this.miniBlockPrefab = miniBlockPrefab;

        Refresh();
        EnsureRootCollider();
    }

    public void Refresh()
    {
        ClearVisualBlocks();

        if (Data == null)
            return;

        List<VisualGroup> groups = BuildVisualGroups();

        foreach (VisualGroup group in groups)
        {
            CreateGroupVisual(group);
        }
    }

    public void PlayMatchDisappearEffect(MiniSlot slot, ColorId color)
    {
        if (color == ColorId.None)
            return;

        Vector3 localPosition = PieceLayout.GetSlotLocalPosition(slot);
        Vector3 targetSize = new Vector3(
            PieceLayout.MiniWidth,
            PieceLayout.MiniHeight,
            PieceLayout.MiniDepth
        );

        GameObject ghost = CreateVisualBlock(
            $"MatchDisappear_{color}_{slot}",
            localPosition,
            targetSize,
            color
        );

        ghost.transform.SetParent(null, true);

        MatchDisappearEffect effect = ghost.AddComponent<MatchDisappearEffect>();
        effect.Play();
    }

    public void PlayFillAnimations(List<FillMove> fillMoves)
    {
        if (fillMoves == null || fillMoves.Count == 0)
            return;

        foreach (FillMove move in fillMoves)
        {
            StartCoroutine(PlayFillAnimationRoutine(move));
        }
    }

    private IEnumerator PlayFillAnimationRoutine(FillMove move)
    {
        Vector3 fromPosition = PieceLayout.GetSlotLocalPosition(move.From);
        Vector3 toPosition = PieceLayout.GetSlotLocalPosition(move.To);

        bool horizontal = Mathf.Abs(fromPosition.x - toPosition.x) >
                          Mathf.Abs(fromPosition.z - toPosition.z);

        Vector3 startPosition = (fromPosition + toPosition) * 0.5f;
        startPosition.y += 0.025f;

        Vector3 endPosition = toPosition;
        endPosition.y += 0.025f;

        Vector3 startSize;

        if (horizontal)
        {
            float distance = Mathf.Abs(fromPosition.x - toPosition.x);
            startSize = new Vector3(
                distance + PieceLayout.MiniWidth,
                PieceLayout.MiniHeight,
                PieceLayout.MiniDepth
            );
        }
        else
        {
            float distance = Mathf.Abs(fromPosition.z - toPosition.z);
            startSize = new Vector3(
                PieceLayout.MiniWidth,
                PieceLayout.MiniHeight,
                distance + PieceLayout.MiniDepth
            );
        }

        Vector3 endSize = new Vector3(
            PieceLayout.MiniWidth,
            PieceLayout.MiniHeight,
            PieceLayout.MiniDepth
        );

        GameObject fillGhost = CreateVisualBlock(
            $"FillGhost_{move.Color}_{move.From}_to_{move.To}",
            startPosition,
            startSize,
            move.Color
        );

        Vector3 startScale = fillGhost.transform.localScale;
        Vector3 endScale = new Vector3(
            startScale.x * endSize.x / startSize.x,
            startScale.y * endSize.y / startSize.y,
            startScale.z * endSize.z / startSize.z
        );

        float duration = 0.22f;
        float elapsed = 0f;

        while (elapsed < duration && fillGhost != null)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            fillGhost.transform.localPosition = Vector3.LerpUnclamped(
                startPosition,
                endPosition,
                easedT
            );

            fillGhost.transform.localScale = Vector3.LerpUnclamped(
                startScale,
                endScale,
                easedT
            );

            yield return null;
        }

        if (fillGhost != null)
    {
        Destroy(fillGhost);
    }
    }

    private List<VisualGroup> BuildVisualGroups()
    {
        List<VisualGroup> groups = new List<VisualGroup>();
        HashSet<MiniSlot> visited = new HashSet<MiniSlot>();

        MiniSlot[] allSlots =
        {
            MiniSlot.TopLeft,
            MiniSlot.TopRight,
            MiniSlot.BottomLeft,
            MiniSlot.BottomRight
        };

        foreach (MiniSlot startSlot in allSlots)
        {
            if (visited.Contains(startSlot))
                continue;

            ColorId color = Data.Get(startSlot);

            if (color == ColorId.None)
            {
                visited.Add(startSlot);
                continue;
            }

            VisualGroup group = new VisualGroup(color);

            Queue<MiniSlot> queue = new Queue<MiniSlot>();
            queue.Enqueue(startSlot);
            visited.Add(startSlot);

            while (queue.Count > 0)
            {
                MiniSlot current = queue.Dequeue();
                group.Slots.Add(current);

                foreach (MiniSlot neighbor in GetNeighborSlots(current))
                {
                    if (visited.Contains(neighbor))
                        continue;

                    if (Data.Get(neighbor) != color)
                        continue;

                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            groups.Add(group);
        }

        return groups;
    }

    private IEnumerable<MiniSlot> GetNeighborSlots(MiniSlot slot)
    {
        switch (slot)
        {
            case MiniSlot.TopLeft:
                yield return MiniSlot.TopRight;
                yield return MiniSlot.BottomLeft;
                break;

            case MiniSlot.TopRight:
                yield return MiniSlot.TopLeft;
                yield return MiniSlot.BottomRight;
                break;

            case MiniSlot.BottomLeft:
                yield return MiniSlot.TopLeft;
                yield return MiniSlot.BottomRight;
                break;

            case MiniSlot.BottomRight:
                yield return MiniSlot.TopRight;
                yield return MiniSlot.BottomLeft;
                break;
        }
    }

    private void CreateGroupVisual(VisualGroup group)
    {
        Vector3 center = GetGroupCenter(group.Slots);
        Vector3 size = GetGroupSize(group.Slots);

        GameObject visual = CreateVisualBlock(
            $"Group_{group.Color}_{group.Slots.Count}",
            center,
            size,
            group.Color
        );

        visualBlocks.Add(visual);
    }

    private Vector3 GetGroupCenter(List<MiniSlot> slots)
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        foreach (MiniSlot slot in slots)
        {
            Vector3 position = PieceLayout.GetSlotLocalPosition(slot);

            minX = Mathf.Min(minX, position.x);
            maxX = Mathf.Max(maxX, position.x);
            minZ = Mathf.Min(minZ, position.z);
            maxZ = Mathf.Max(maxZ, position.z);
        }

        return new Vector3(
            (minX + maxX) * 0.5f,
            0f,
            (minZ + maxZ) * 0.5f
        );
    }

    private Vector3 GetGroupSize(List<MiniSlot> slots)
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        foreach (MiniSlot slot in slots)
        {
            Vector3 position = PieceLayout.GetSlotLocalPosition(slot);

            minX = Mathf.Min(minX, position.x);
            maxX = Mathf.Max(maxX, position.x);
            minZ = Mathf.Min(minZ, position.z);
            maxZ = Mathf.Max(maxZ, position.z);
        }

        float width = PieceLayout.MiniWidth + Mathf.Abs(maxX - minX);
        float depth = PieceLayout.MiniDepth + Mathf.Abs(maxZ - minZ);

        return new Vector3(
            width,
            PieceLayout.MiniHeight,
            depth
        );
    }

    private GameObject CreateVisualBlock(string name, Vector3 localPosition, Vector3 targetSize, ColorId color)
    {
        GameObject blockRoot = new GameObject(name);
        blockRoot.transform.SetParent(transform, false);
        blockRoot.transform.localPosition = localPosition;
        blockRoot.transform.localRotation = Quaternion.identity;
        blockRoot.transform.localScale = Vector3.one;

        GameObject visualObject;

        if (miniBlockPrefab != null)
        {
            visualObject = Instantiate(miniBlockPrefab, blockRoot.transform);
            visualObject.name = "Visual";
        }
        else
        {
            visualObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualObject.name = "Visual";
            visualObject.transform.SetParent(blockRoot.transform, false);
        }

        visualObject.transform.localPosition = Vector3.zero;
        visualObject.transform.localRotation = Quaternion.identity;
        visualObject.transform.localScale = Vector3.one;

        Collider[] colliders = visualObject.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            Destroy(collider);
        }

        Renderer[] renderers = visualObject.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogError($"Visual block has no renderer: {name}");
            return blockRoot;
        }

        ApplyMaterial(renderers, color);
        NormalizeVisualToTarget(blockRoot.transform, visualObject.transform, renderers, targetSize);

        return blockRoot;
    }

    private void ApplyMaterial(Renderer[] renderers, ColorId color)
    {
        Material material = RuntimeMaterialLibrary.Get(color);

        foreach (Renderer renderer in renderers)
        {
            renderer.sharedMaterial = material;
        }
    }

    private void NormalizeVisualToTarget(
        Transform blockRoot,
        Transform visualRoot,
        Renderer[] renderers,
        Vector3 targetSize)
    {
        Bounds worldBounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            worldBounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 localSize = new Vector3(
            worldBounds.size.x / Mathf.Abs(blockRoot.lossyScale.x),
            worldBounds.size.y / Mathf.Abs(blockRoot.lossyScale.y),
            worldBounds.size.z / Mathf.Abs(blockRoot.lossyScale.z)
        );

        Vector3 scaleMultiplier = new Vector3(
            targetSize.x / Mathf.Max(localSize.x, 0.0001f),
            targetSize.y / Mathf.Max(localSize.y, 0.0001f),
            targetSize.z / Mathf.Max(localSize.z, 0.0001f)
        );

        visualRoot.localScale = Vector3.Scale(visualRoot.localScale, scaleMultiplier);

        worldBounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            worldBounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 localCenter = blockRoot.InverseTransformPoint(worldBounds.center);
        visualRoot.localPosition -= localCenter;
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

    private void ClearVisualBlocks()
    {
        for (int i = visualBlocks.Count - 1; i >= 0; i--)
        {
            if (visualBlocks[i] != null)
            {
                Destroy(visualBlocks[i]);
            }
        }

        visualBlocks.Clear();
    }

    private class VisualGroup
    {
        public readonly ColorId Color;
        public readonly List<MiniSlot> Slots = new List<MiniSlot>();

        public VisualGroup(ColorId color)
        {
            Color = color;
        }
    }
}