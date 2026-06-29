using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [SerializeField] private GameObject miniBlockPrefab;

    private BoardManager boardManager;
    private int spawnIndex;
    private int activeSlotCount = 1;
    private bool initialized;

    private readonly Dictionary<PieceView, int> slotByPiece = new Dictionary<PieceView, int>();

    public void Initialize(BoardManager boardManager)
    {
        this.boardManager = boardManager;
        initialized = true;

        LevelConfig config = LevelManager.Instance != null
            ? LevelManager.Instance.CurrentConfig
            : null;

        activeSlotCount = config != null
            ? Mathf.Clamp(config.SpawnSlotCount, 1, 2)
            : 1;

        SpawnInitialPieces();
    }

    private void SpawnInitialPieces()
    {
        slotByPiece.Clear();

        for (int i = 0; i < activeSlotCount; i++)
        {
            SpawnPieceIntoSlot(i);
        }
    }

    public void SpawnReplacementFor(PieceView placedPiece)
    {
        if (!initialized)
            return;

        if (LevelManager.Instance != null && LevelManager.Instance.IsLevelEnded)
            return;

        if (placedPiece == null)
            return;

        if (!slotByPiece.TryGetValue(placedPiece, out int slotIndex))
        {
            Debug.LogWarning("Placed piece was not registered in any spawn slot.");
            return;
        }

        slotByPiece.Remove(placedPiece);
        SpawnPieceIntoSlot(slotIndex);
    }

    private void SpawnPieceIntoSlot(int slotIndex)
    {
        if (boardManager == null)
        {
            Debug.LogError("BoardManager is missing.");
            return;
        }

        GameObject pieceObject = new GameObject($"Piece_{spawnIndex}_Slot_{slotIndex}");
        pieceObject.transform.position = boardManager.GetSpawnWorldPosition(slotIndex, activeSlotCount);

        PieceView pieceView = pieceObject.AddComponent<PieceView>();
        pieceView.Build(CreatePieceData(spawnIndex), miniBlockPrefab);

        JellyAnimator jellyAnimator = pieceObject.AddComponent<JellyAnimator>();
        jellyAnimator.PlaySpawnAnimation();

        PieceDragController dragController = pieceObject.AddComponent<PieceDragController>();
        dragController.Init(boardManager, this);

        slotByPiece[pieceView] = slotIndex;
        spawnIndex++;
    }

    private PieceData CreatePieceData(int index)
    {
        LevelConfig config = LevelManager.Instance != null
            ? LevelManager.Instance.CurrentConfig
            : null;

        if (config != null && config.PieceSequence != null && config.PieceSequence.Length > 0)
        {
            PiecePattern pattern = config.PieceSequence[index % config.PieceSequence.Length];

            if (pattern != null)
            {
                return pattern.ToPieceData();
            }
        }

        return CreateFallbackPieceData(index);
    }

    private PieceData CreateFallbackPieceData(int index)
    {
        int patternIndex = index % 6;

        switch (patternIndex)
        {
            case 0:
                return new PieceData(ColorId.Yellow, ColorId.Green, ColorId.Red, ColorId.Red);

            case 1:
                return new PieceData(ColorId.Yellow, ColorId.Yellow, ColorId.Red, ColorId.Green);

            case 2:
                return new PieceData(ColorId.Purple, ColorId.Green, ColorId.Yellow, ColorId.Purple);

            case 3:
                return new PieceData(ColorId.Green, ColorId.Red, ColorId.Blue, ColorId.Blue);

            case 4:
                return new PieceData(ColorId.Pink, ColorId.Yellow, ColorId.Pink, ColorId.Green);

            default:
                return new PieceData(ColorId.Blue, ColorId.Purple, ColorId.Red, ColorId.Yellow);
        }
    }
}