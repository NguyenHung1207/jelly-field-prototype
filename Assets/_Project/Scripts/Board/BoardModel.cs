public class BoardModel
{
    private readonly PieceView[,] pieces;
    private readonly bool[,] validCells;

    public int Width { get; }
    public int Height { get; }

    public BoardModel(int width, int height, bool[,] validCells)
    {
        Width = width;
        Height = height;

        pieces = new PieceView[width, height];
        this.validCells = validCells;
    }

    public bool IsInsideBounds(int x, int z)
    {
        return x >= 0 && x < Width && z >= 0 && z < Height;
    }

    public bool IsValidCell(int x, int z)
    {
        if (!IsInsideBounds(x, z))
            return false;

        if (validCells == null)
            return true;

        return validCells[x, z];
    }

    public bool CanPlace(int x, int z)
    {
        return IsValidCell(x, z) && pieces[x, z] == null;
    }

    public void PlacePiece(int x, int z, PieceView pieceView)
    {
        if (!IsValidCell(x, z))
            return;

        pieces[x, z] = pieceView;
    }

    public PieceView GetPiece(int x, int z)
    {
        if (!IsValidCell(x, z))
            return null;

        return pieces[x, z];
    }

    public bool RemovePiece(PieceView pieceView)
    {
        if (pieceView == null)
            return false;

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                if (pieces[x, z] == pieceView)
                {
                    pieces[x, z] = null;
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsFull()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                if (!IsValidCell(x, z))
                    continue;

                if (pieces[x, z] == null)
                    return false;
            }
        }

        return true;
    }
}