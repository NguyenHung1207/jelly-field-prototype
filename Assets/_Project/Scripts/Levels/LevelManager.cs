using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public event Action OnScoreChanged;
    public event Action OnLevelCompleted;
    public event Action OnLevelFailed;

    [Header("Level Target")]
    [SerializeField] private TargetGoal[] targetGoals =
    {
        new TargetGoal { Color = ColorId.Yellow, RequiredScore = 6 }
    };

    private readonly Dictionary<ColorId, int> requiredScores = new Dictionary<ColorId, int>();
    private readonly Dictionary<ColorId, int> currentScores = new Dictionary<ColorId, int>();

    public bool IsLevelEnded { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeTargets();
    }

    private void Start()
    {
        OnScoreChanged?.Invoke();
    }

    private void InitializeTargets()
    {
        requiredScores.Clear();
        currentScores.Clear();
        IsLevelEnded = false;

        foreach (TargetGoal goal in targetGoals)
        {
            if (goal == null)
                continue;

            if (goal.Color == ColorId.None)
                continue;

            if (goal.RequiredScore <= 0)
                continue;

            requiredScores[goal.Color] = goal.RequiredScore;
            currentScores[goal.Color] = 0;
        }

        Debug.Log("Level started.");
        LogTargetProgress();
    }

    public void AddScores(Dictionary<ColorId, int> scoreByColor)
    {
        if (IsLevelEnded)
            return;

        if (scoreByColor == null || scoreByColor.Count == 0)
            return;

        foreach (KeyValuePair<ColorId, int> pair in scoreByColor)
        {
            ColorId color = pair.Key;
            int score = pair.Value;

            if (!requiredScores.ContainsKey(color))
                continue;

            currentScores[color] += score;

            if (currentScores[color] > requiredScores[color])
            {
                currentScores[color] = requiredScores[color];
            }

            Debug.Log($"Score +{score} for {color}");
        }

        LogTargetProgress();
        OnScoreChanged?.Invoke();

        if (IsWinConditionMet())
        {
            CompleteLevel();
        }
    }

    public bool HasTarget(ColorId color)
    {
        return requiredScores.ContainsKey(color);
    }

    public IReadOnlyDictionary<ColorId, int> GetRequiredScores()
    {
        return requiredScores;
    }

    public IReadOnlyDictionary<ColorId, int> GetCurrentScores()
    {
        return currentScores;
    }

    public void LoseLevel()
    {
        if (IsLevelEnded)
            return;

        IsLevelEnded = true;
        Debug.Log("LEVEL FAILED: Board is full and targets are not completed.");
        OnLevelFailed?.Invoke();
    }

    private bool IsWinConditionMet()
    {
        foreach (KeyValuePair<ColorId, int> pair in requiredScores)
        {
            ColorId color = pair.Key;
            int required = pair.Value;

            if (!currentScores.ContainsKey(color))
                return false;

            if (currentScores[color] < required)
                return false;
        }

        return true;
    }

    private void CompleteLevel()
    {
        if (IsLevelEnded)
            return;

        IsLevelEnded = true;
        Debug.Log("LEVEL COMPLETE!");
        OnLevelCompleted?.Invoke();
    }

    private void LogTargetProgress()
    {
        foreach (KeyValuePair<ColorId, int> pair in requiredScores)
        {
            ColorId color = pair.Key;
            int required = pair.Value;
            int current = currentScores.ContainsKey(color) ? currentScores[color] : 0;

            Debug.Log($"Target {color}: {current}/{required}");
        }
    }
}