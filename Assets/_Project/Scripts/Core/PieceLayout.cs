using UnityEngine;

public static class PieceLayout
{
    public const float MiniWidth = 0.44f;
    public const float MiniHeight = 0.44f;
    public const float MiniDepth = 0.44f;

    public const float InnerGap = 0.015f;

    public const float SlotOffset = (MiniWidth + InnerGap) * 0.5f;

    public const float BoardCellFootprint = 0.96f;
    public const float BoardCellHeight = 0.10f;

    public const float PlacedPieceY = 0.23f;

    public static Vector3 GetSlotLocalPosition(MiniSlot slot)
    {
        return slot switch
        {
            MiniSlot.TopLeft => new Vector3(-SlotOffset, 0f, SlotOffset),
            MiniSlot.TopRight => new Vector3(SlotOffset, 0f, SlotOffset),
            MiniSlot.BottomLeft => new Vector3(-SlotOffset, 0f, -SlotOffset),
            MiniSlot.BottomRight => new Vector3(SlotOffset, 0f, -SlotOffset),
            _ => Vector3.zero
        };
    }
}