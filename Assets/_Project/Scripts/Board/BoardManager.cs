using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Fallback Board Settings")]
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 5;
    [SerializeField] private float cellSize = 1.0f;

    private BoardModel boardModel;
    private MatchResolver matchResolver;
    private Transform boardRoot;
    private bool[,] validCells;

    private PieceSpawner pieceSpawner;
    private bool isResolvingTurn;
    private readonly Dictionary<Vector2Int, Renderer> cellRenderers = new Dictionary<Vector2Int, Renderer>();

    private Material normalCellMaterial;
    private Material validPreviewMaterial;
    private Material invalidPreviewMaterial;

    private Vector2Int? currentPreviewCell;

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
        CreateRuntimeMaterials();
    }

    private void Start()
    {
        ApplyLevelConfig();
        validCells = CreateValidCellMask();
        boardModel = new BoardModel(width, height, validCells);

        SetupCamera();
        CreateBoard();

        pieceSpawner = FindFirstObjectByType<PieceSpawner>();

        if (pieceSpawner != null)
        {
            pieceSpawner.Initialize(this);
        }
    }

    private void CreateRuntimeMaterials()
    {
        normalCellMaterial = CreateMaterial("Cell_Normal", new Color(0.28f, 0.28f, 0.32f));
        validPreviewMaterial = CreateMaterial("Cell_Valid_Preview", new Color(0.25f, 0.75f, 0.35f));
        invalidPreviewMaterial = CreateMaterial("Cell_Invalid_Preview", new Color(0.85f, 0.25f, 0.25f));
    }

    private Material CreateMaterial(string materialName, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = materialName;

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

        cellRenderers.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (!validCells[x, z])
                    continue;

                GameObject cell = new GameObject($"Cell_{x}_{z}");

                cell.transform.SetParent(boardRoot, false);
                cell.transform.position = GetCellWorldPosition(x, z);

                MeshFilter meshFilter = cell.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = RoundedCellMeshFactory.CreateRoundedSquareMesh(
                    PieceLayout.BoardCellFootprint,
                    0.16f,
                    8
                );

                MeshRenderer meshRenderer = cell.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = normalCellMaterial;

                Vector2Int coordinate = new Vector2Int(x, z);
                cellRenderers[coordinate] = meshRenderer;
            }
        }
    }

    public bool TryPlacePiece(PieceView pieceView)
    {
        if (isResolvingTurn)
            return false;

        ClearPlacementPreview();

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
        snapPosition.y = PieceLayout.PlacedPieceY;

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

        StartCoroutine(FinishTurnSequence(pieceView));

        return true;
    }

    private IEnumerator FinishTurnSequence(PieceView placedPieceView)
    {
        isResolvingTurn = true;

        Dictionary<ColorId, int> scoreByColor = null;

        yield return StartCoroutine(matchResolver.ResolveSequential(
            boardModel,
            result => scoreByColor = result
        ));

        if (scoreByColor == null)
        {
            scoreByColor = new Dictionary<ColorId, int>();
        }

        if (scoreByColor.Count == 0)
        {
            yield return new WaitForSeconds(GameAnimationTiming.PlaceAnimationDuration);
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddScores(scoreByColor);

            if (!LevelManager.Instance.IsLevelEnded && boardModel.IsFull())
            {
                LevelManager.Instance.LoseLevel();
            }

            if (!LevelManager.Instance.IsLevelEnded && pieceSpawner != null)
            {
                pieceSpawner.SpawnReplacementFor(placedPieceView);
            }
        }
        else
        {
            if (pieceSpawner != null)
            {
                pieceSpawner.SpawnReplacementFor(placedPieceView);
            }
        }

        isResolvingTurn = false;
    }

    public void ShowPlacementPreview(Vector3 worldPosition)
    {
        ClearPlacementPreview();

        int x = Mathf.RoundToInt(worldPosition.x / cellSize);
        int z = Mathf.RoundToInt(worldPosition.z / cellSize);

        if (!boardModel.IsInsideBounds(x, z))
            return;

        Vector2Int coordinate = new Vector2Int(x, z);

        if (!cellRenderers.TryGetValue(coordinate, out Renderer cellRenderer))
            return;

        bool canPlace = boardModel.CanPlace(x, z);
        cellRenderer.sharedMaterial = canPlace ? validPreviewMaterial : invalidPreviewMaterial;

        currentPreviewCell = coordinate;
    }

    public void ClearPlacementPreview()
    {
        if (!currentPreviewCell.HasValue)
            return;

        if (cellRenderers.TryGetValue(currentPreviewCell.Value, out Renderer cellRenderer))
        {
            cellRenderer.sharedMaterial = normalCellMaterial;
        }

        currentPreviewCell = null;
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

    public Vector3 GetSpawnWorldPosition(int slotIndex, int slotCount)
    {
        float centerX = (width - 1) * cellSize * 0.5f;
        float spawnZ = -1.45f * cellSize;

        float slotSpacing = 0.90f;
        float offsetX = (slotIndex - (slotCount - 1) * 0.5f) * slotSpacing;

        return new Vector3(
            centerX + offsetX,
            PieceLayout.PlacedPieceY,
            spawnZ
        );
    }

    public Vector3 GetSpawnWorldPosition()
    {
        return GetSpawnWorldPosition(0, 1);
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