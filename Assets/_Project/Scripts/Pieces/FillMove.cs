public readonly struct FillMove
{
    public readonly MiniSlot From;
    public readonly MiniSlot To;
    public readonly ColorId Color;

    public FillMove(MiniSlot from, MiniSlot to, ColorId color)
    {
        From = from;
        To = to;
        Color = color;
    }
}