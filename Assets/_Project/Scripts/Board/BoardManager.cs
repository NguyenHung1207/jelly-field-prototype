using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Fallback Board Settings")]
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 5;
    [SerializeField] private float cellSize = 1.1f;

    private BoardModel boardModel;
    private MatchResolver matchResolver;
    private Transform boardRoot;
    private bool[,] validCells;

    public float DragPlaneY => 0.35f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        matchResolver = new MatchResolver();
    }

    private void Start()
    {
        ApplyLevelConfig();
        validCells = CreateValidCellMask();
        boardModel = new BoardModel(width, height, validCells);

        SetupCamera();
        CreateBoard();

        PieceSpawner spawner = FindFirstObjectByType<PieceSpawner>();
        if (spawner != null)
        {
            spawner.Initialize(this);
        }
    }

    private void ApplyLevelConfig()
    {
        if (LevelManager.Instance == null || LevelManager.Instance.CurrentConfig == null)
        {
            Debug.LogWarning("BoardManager is using fallback board settings because LevelConfig is missing.");
            return;
        }

        LevelConfig config = LevelManager.Instance.CurrentConfig;
        width = config.Width;
        height = config.Height;
    }

    private bool[,] CreateValidCellMask()
    {
        bool[,] mask = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                mask[x, z] = true;
            }
        }

        if (LevelManager.Instance == null || LevelManager.Instance.CurrentConfig == null)
        {
            return mask;
        }

        Vector2Int[] blockedCells = LevelManager.Instance.CurrentConfig.BlockedCells;

        foreach (Vector2Int blockedCell in blockedCells)
        {
            if (blockedCell.x < 0 || blockedCell.x >= width)
                continue;

            if (blockedCell.y < 0 || blockedCell.y >= height)
                continue;

            mask[blockedCell.x, blockedCell.y] = false;
        }

        return mask;
    }

    private void CreateBoard()
    {
        GameObject boardRootObject = new GameObject("Generated Board");
        boardRootObject.transform.SetParent(transform, false);
        boardRoot = boardRootObject.transform;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (!validCells[x, z])
                    continue;

                GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cell.name = $"Cell_{x}_{z}";

                cell.transform.SetParent(boardRoot, false);
                cell.transform.position = GetCellWorldPosition(x, z);
                cell.transform.localScale = new Vector3(0.95f, 0.08f, 0.95f);

                Renderer renderer = cell.GetComponent<Renderer>();
                renderer.sharedMaterial = RuntimeMaterialLibrary.Get(ColorId.None);
            }
        }
    }

    public bool TryPlacePiece(PieceView pieceView)
    {
        if (pieceView == null)
            return false;

        if (!TryGetGridPositionFromWorld(pieceView.transform.position, out int x, out int z))
            return false;

        if (!boardModel.CanPlace(x, z))
        {
            Debug.LogWarning($"Cannot place at ({x}, {z}). Cell is invalid or occupied.");
            return false;
        }

        boardModel.PlacePiece(x, z, pieceView);

        Vector3 snapPosition = GetCellWorldPosition(x, z);
        snapPosition.y = 0.25f;

        pieceView.transform.position = snapPosition;
        pieceView.transform.SetParent(boardRoot, true);

        PieceDragController dragController = pieceView.GetComponent<PieceDragController>();
        if (dragController != null)
        {
            dragController.SetPlaced();
        }

        Collider collider = pieceView.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        Debug.Log($"Placed piece at ({x}, {z})");

        Dictionary<ColorId, int> scoreByColor = matchResolver.Resolve(boardModel);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddScores(scoreByColor);

            if (!LevelManager.Instance.IsLevelEnded && boardModel.IsFull())
            {
                LevelManager.Instance.LoseLevel();
            }
        }

        return true;
    }

    public bool TryGetGridPositionFromWorld(Vector3 worldPosition, out int x, out int z)
    {
        x = Mathf.RoundToInt(worldPosition.x / cellSize);
        z = Mathf.RoundToInt(worldPosition.z / cellSize);

        return boardModel.IsValidCell(x, z);
    }

    public Vector3 GetCellWorldPosition(int x, int z)
    {
        return new Vector3(x * cellSize, -0.08f, z * cellSize);
    }

    public Vector3 GetSpawnWorldPosition()
    {
        float centerX = (width - 1) * cellSize * 0.5f;
        float spawnZ = -1.4f * cellSize;

        return new Vector3(centerX, 0.25f, spawnZ);
    }

    private void SetupCamera()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 boardCenter = new Vector3(
            (width - 1) * cellSize * 0.5f,
            0f,
            (height - 1) * cellSize * 0.5f
        );

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = Mathf.Max(width, height + 2) * 0.75f;
        mainCamera.transform.position = boardCenter + new Vector3(0f, 6f, -6f);
        mainCamera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }
}