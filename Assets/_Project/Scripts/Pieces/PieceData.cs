using System;
using System.Collections.Generic;

[Serializable]
public class PieceData
{
    public ColorId[] Colors = new ColorId[4];

    public PieceData(ColorId topLeft, ColorId topRight, ColorId bottomLeft, ColorId bottomRight)
    {
        Colors[(int)MiniSlot.TopLeft] = topLeft;
        Colors[(int)MiniSlot.TopRight] = topRight;
        Colors[(int)MiniSlot.BottomLeft] = bottomLeft;
        Colors[(int)MiniSlot.BottomRight] = bottomRight;
    }

    public ColorId Get(MiniSlot slot)
    {
        return Colors[(int)slot];
    }

    public void Set(MiniSlot slot, ColorId color)
    {
        Colors[(int)slot] = color;
    }

    public void Clear(params MiniSlot[] slots)
    {
        foreach (MiniSlot slot in slots)
        {
            Set(slot, ColorId.None);
        }
    }

    public List<FillMove> ResolveEmptySlots()
    {
        List<FillMove> fillMoves = new List<FillMove>();

        ResolveHorizontalFirst(fillMoves);
        ResolveVerticalSecond(fillMoves);

        return fillMoves;
    }

    private void ResolveHorizontalFirst(List<FillMove> fillMoves)
    {
        CopyIfEmpty(MiniSlot.TopLeft, MiniSlot.TopRight, fillMoves);
        CopyIfEmpty(MiniSlot.TopRight, MiniSlot.TopLeft, fillMoves);

        CopyIfEmpty(MiniSlot.BottomLeft, MiniSlot.BottomRight, fillMoves);
        CopyIfEmpty(MiniSlot.BottomRight, MiniSlot.BottomLeft, fillMoves);
    }

    private void ResolveVerticalSecond(List<FillMove> fillMoves)
    {
        CopyIfEmpty(MiniSlot.TopLeft, MiniSlot.BottomLeft, fillMoves);
        CopyIfEmpty(MiniSlot.TopRight, MiniSlot.BottomRight, fillMoves);

        CopyIfEmpty(MiniSlot.BottomLeft, MiniSlot.TopLeft, fillMoves);
        CopyIfEmpty(MiniSlot.BottomRight, MiniSlot.TopRight, fillMoves);
    }

    private bool CopyIfEmpty(MiniSlot emptySlot, MiniSlot sourceSlot, List<FillMove> fillMoves)
    {
        int emptyIndex = (int)emptySlot;
        int sourceIndex = (int)sourceSlot;

        if (Colors[emptyIndex] != ColorId.None)
            return false;

        if (Colors[sourceIndex] == ColorId.None)
            return false;

        Colors[emptyIndex] = Colors[sourceIndex];
        fillMoves.Add(new FillMove(sourceSlot, emptySlot, Colors[sourceIndex]));

        return true;
    }

    public bool IsEmpty()
    {
        for (int i = 0; i < Colors.Length; i++)
        {
            if (Colors[i] != ColorId.None)
                return false;
        }

        return true;
    }
}