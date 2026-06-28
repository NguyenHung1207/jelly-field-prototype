using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 5;
    [SerializeField] private float cellSize = 1.1f;

    private PieceView testPiece;

    private void Start()
    {
        SetupCamera();
        CreateBoard();
        CreateTestPieceA();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestCaseA();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestCaseB();
        }
    }

    private void CreateBoard()
    {
        GameObject boardRoot = new GameObject("Generated Board");
        boardRoot.transform.SetParent(transform, false);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cell.name = $"Cell_{x}_{z}";

                cell.transform.SetParent(boardRoot.transform, false);
                cell.transform.position = new Vector3(x * cellSize, -0.08f, z * cellSize);
                cell.transform.localScale = new Vector3(0.95f, 0.08f, 0.95f);

                Renderer renderer = cell.GetComponent<Renderer>();
                renderer.sharedMaterial = RuntimeMaterialLibrary.Get(ColorId.None);
            }
        }
    }

    private void CreateTestPieceA()
    {
        PieceData data = new PieceData(
            ColorId.Yellow,
            ColorId.Green,
            ColorId.Red,
            ColorId.Red
        );

        CreateOrReplaceTestPiece(data);
    }

    private void CreateTestPieceB()
    {
        PieceData data = new PieceData(
            ColorId.Yellow,
            ColorId.Yellow,
            ColorId.Red,
            ColorId.Green
        );

        CreateOrReplaceTestPiece(data);
    }

    private void CreateOrReplaceTestPiece(PieceData data)
    {
        if (testPiece != null)
        {
            Destroy(testPiece.gameObject);
        }

        GameObject pieceObject = new GameObject("Test Piece");
        pieceObject.transform.position = new Vector3(
            (width - 1) * cellSize * 0.5f,
            0.25f,
            (height - 1) * cellSize * 0.5f
        );

        testPiece = pieceObject.AddComponent<PieceView>();
        testPiece.Build(data);
    }

    private void TestCaseA()
    {
        CreateTestPieceA();

        testPiece.Data.Clear(MiniSlot.TopLeft);
        testPiece.Data.ResolveEmptySlots();
        testPiece.Refresh();

        Debug.Log("Test A: [Yellow, Green, Red, Red] -> clear Yellow -> [Green, Green, Red, Red]");
    }

    private void TestCaseB()
    {
        CreateTestPieceB();

        testPiece.Data.Clear(MiniSlot.TopLeft, MiniSlot.TopRight);
        testPiece.Data.ResolveEmptySlots();
        testPiece.Refresh();

        Debug.Log("Test B: [Yellow, Yellow, Red, Green] -> clear Yellow row -> [Red, Green, Red, Green]");
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
        mainCamera.orthographicSize = Mathf.Max(width, height) * 0.85f;
        mainCamera.transform.position = boardCenter + new Vector3(0f, 6f, -6f);
        mainCamera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }
}