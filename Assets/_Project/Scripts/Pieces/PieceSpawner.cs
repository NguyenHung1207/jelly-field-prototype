using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    private BoardManager boardManager;
    private int spawnIndex;

    private void Start()
    {
        boardManager = BoardManager.Instance;
        SpawnNextPiece();
    }

    public void SpawnNextPiece()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.IsLevelEnded)
        {
            return;
        }

        if (boardManager == null)
        {
            Debug.LogError("BoardManager is missing.");
            return;
        }

        GameObject pieceObject = new GameObject($"Piece_{spawnIndex}");
        pieceObject.transform.position = boardManager.GetSpawnWorldPosition();

        PieceView pieceView = pieceObject.AddComponent<PieceView>();
        pieceView.Build(CreatePieceData(spawnIndex));

        PieceDragController dragController = pieceObject.AddComponent<PieceDragController>();
        dragController.Init(boardManager, this);

        spawnIndex++;
    }

    private PieceData CreatePieceData(int index)
    {
        int patternIndex = index % 6;

        switch (patternIndex)
        {
            case 0:
                return new PieceData(
                    ColorId.Yellow,
                    ColorId.Green,
                    ColorId.Red,
                    ColorId.Red
                );

            case 1:
                return new PieceData(
                    ColorId.Yellow,
                    ColorId.Yellow,
                    ColorId.Red,
                    ColorId.Green
                );

            case 2:
                return new PieceData(
                    ColorId.Purple,
                    ColorId.Green,
                    ColorId.Yellow,
                    ColorId.Purple
                );

            case 3:
                return new PieceData(
                    ColorId.Green,
                    ColorId.Red,
                    ColorId.Blue,
                    ColorId.Blue
                );

            case 4:
                return new PieceData(
                    ColorId.Pink,
                    ColorId.Yellow,
                    ColorId.Pink,
                    ColorId.Green
                );

            default:
                return new PieceData(
                    ColorId.Blue,
                    ColorId.Purple,
                    ColorId.Red,
                    ColorId.Yellow
                );
        }
    }
}