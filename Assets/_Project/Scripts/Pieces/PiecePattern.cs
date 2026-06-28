using System;

[Serializable]
public class PiecePattern
{
    public ColorId TopLeft;
    public ColorId TopRight;
    public ColorId BottomLeft;
    public ColorId BottomRight;

    public PieceData ToPieceData()
    {
        return new PieceData(TopLeft, TopRight, BottomLeft, BottomRight);
    }
}