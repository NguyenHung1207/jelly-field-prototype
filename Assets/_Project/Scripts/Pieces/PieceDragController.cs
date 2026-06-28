using UnityEngine;

public class PieceDragController : MonoBehaviour
{
    private BoardManager boardManager;
    private PieceSpawner pieceSpawner;
    private PieceView pieceView;

    private Vector3 startPosition;
    private bool isPlaced;

    public void Init(BoardManager boardManager, PieceSpawner pieceSpawner)
    {
        this.boardManager = boardManager;
        this.pieceSpawner = pieceSpawner;
        pieceView = GetComponent<PieceView>();

        startPosition = transform.position;
    }

    private void OnMouseDown()
    {
        if (isPlaced)
            return;

        startPosition = transform.position;
    }

    private void OnMouseDrag()
    {
        if (isPlaced)
            return;

        MoveToPointerPosition();
    }

    private void OnMouseUp()
    {
        if (isPlaced)
            return;

        bool placedSuccessfully = boardManager.TryPlacePiece(pieceView);

        if (placedSuccessfully)
        {
            isPlaced = true;

            if (LevelManager.Instance == null || !LevelManager.Instance.IsLevelEnded)
            {
                pieceSpawner.SpawnNextPiece();
            }
        }
        else
        {
            transform.position = startPosition;
        }
    }

    public void SetPlaced()
    {
        isPlaced = true;
        enabled = false;
    }

    private void MoveToPointerPosition()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane dragPlane = new Plane(Vector3.up, new Vector3(0f, boardManager.DragPlaneY, 0f));

        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPosition = ray.GetPoint(enter);
            transform.position = worldPosition;
        }
    }
}