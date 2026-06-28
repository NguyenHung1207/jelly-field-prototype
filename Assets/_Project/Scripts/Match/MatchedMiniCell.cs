public readonly struct MatchedMiniCell
{
    public readonly PieceView Piece;
    public readonly MiniSlot Slot;
    public readonly ColorId Color;

    public MatchedMiniCell(PieceView piece, MiniSlot slot, ColorId color)
    {
        Piece = piece;
        Slot = slot;
        Color = color;
    }
}