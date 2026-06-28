using System;

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

    public void ResolveEmptySlots()
    {
        ResolveHorizontalFirst();
        ResolveVerticalSecond();
    }

    private void ResolveHorizontalFirst()
    {
        CopyIfEmpty(MiniSlot.TopLeft, MiniSlot.TopRight);
        CopyIfEmpty(MiniSlot.TopRight, MiniSlot.TopLeft);

        CopyIfEmpty(MiniSlot.BottomLeft, MiniSlot.BottomRight);
        CopyIfEmpty(MiniSlot.BottomRight, MiniSlot.BottomLeft);
    }

    private void ResolveVerticalSecond()
    {
        CopyIfEmpty(MiniSlot.TopLeft, MiniSlot.BottomLeft);
        CopyIfEmpty(MiniSlot.TopRight, MiniSlot.BottomRight);

        CopyIfEmpty(MiniSlot.BottomLeft, MiniSlot.TopLeft);
        CopyIfEmpty(MiniSlot.BottomRight, MiniSlot.TopRight);
    }

    private bool CopyIfEmpty(MiniSlot emptySlot, MiniSlot sourceSlot)
    {
        int emptyIndex = (int)emptySlot;
        int sourceIndex = (int)sourceSlot;

        if (Colors[emptyIndex] != ColorId.None)
            return false;

        if (Colors[sourceIndex] == ColorId.None)
            return false;

        Colors[emptyIndex] = Colors[sourceIndex];
        return true;
    }
}