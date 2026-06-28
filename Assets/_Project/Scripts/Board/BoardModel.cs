public class BoardModel
{
    private readonly PieceView[,] pieces;

    public int Width { get; }
    public int Height { get; }

    public BoardModel(int width, int height)
    {
        Width = width;
        Height = height;
        pieces = new PieceView[width, height];
    }

    public bool IsInside(int x, int z)
    {
        return x >= 0 && x < Width && z >= 0 && z < Height;
    }

    public bool CanPlace(int x, int z)
    {
        return IsInside(x, z) && pieces[x, z] == null;
    }

    public void PlacePiece(int x, int z, PieceView pieceView)
    {
        pieces[x, z] = pieceView;
    }

    public PieceView GetPiece(int x, int z)
    {
        if (!IsInside(x, z))
            return null;

        return pieces[x, z];
    }

    public bool IsFull()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                if (pieces[x, z] == null)
                    return false;
            }
        }

        return true;
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
}