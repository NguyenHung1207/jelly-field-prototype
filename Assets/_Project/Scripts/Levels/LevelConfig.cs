using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Jelly Field/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Level Info")]
    public string LevelName = "Level 1";

    [Header("Board")]
    public int Width = 5;
    public int Height = 5;
    public Vector2Int[] BlockedCells;

    [Header("Targets")]
    public TargetGoal[] TargetGoals;

    [Header("Spawn")]
    [Range(1, 2)]
    public int SpawnSlotCount = 1;

    [Header("Piece Sequence")]
    public PiecePattern[] PieceSequence;
}