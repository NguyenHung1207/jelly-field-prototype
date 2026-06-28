using UnityEngine;

public class PieceDragController : MonoBehaviour
{
    private BoardManager boardManager;
    private PieceSpawner pieceSpawner;
    private PieceView pieceView;
    private JellyAnimator jellyAnimator;

    private Vector3 startPosition;
    private bool isPlaced;

    public void Init(BoardManager boardManager, PieceSpawner pieceSpawner)
    {
        this.boardManager = boardManager;
        this.pieceSpawner = pieceSpawner;

        pieceView = GetComponent<PieceView>();
        jellyAnimator = GetComponent<JellyAnimator>();

        startPosition = transform.position;
    }

    private void OnMouseDown()
    {
        if (isPlaced)
            return;

        startPosition = transform.position;

        if (jellyAnimator != null)
        {
            jellyAnimator.SetDraggingVisual(true);
        }
    }

    private void OnMouseDrag()
    {
        if (isPlaced)
            return;

        MoveToPointerPosition();

        if (boardManager != null)
        {
            boardManager.ShowPlacementPreview(transform.position);
        }
    }

    private void OnMouseUp()
    {
        if (isPlaced)
            return;

        if (boardManager != null)
        {
            boardManager.ClearPlacementPreview();
        }

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
            if (jellyAnimator != null)
            {
                jellyAnimator.SetDraggingVisual(false);
                jellyAnimator.MoveTo(startPosition, 0.18f);
            }
            else
            {
                transform.position = startPosition;
            }
        }
    }

    public void SetPlaced()
    {
        isPlaced = true;
        enabled = false;

        if (boardManager != null)
        {
            boardManager.ClearPlacementPreview();
        }

        if (jellyAnimator != null)
        {
            jellyAnimator.PlayPlaceAnimation();
        }
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