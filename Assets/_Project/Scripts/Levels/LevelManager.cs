using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public event Action OnScoreChanged;
    public event Action OnLevelCompleted;
    public event Action OnLevelFailed;

    [Header("Level Configs")]
    [SerializeField] private LevelConfig[] levelConfigs;

    private readonly Dictionary<ColorId, int> requiredScores = new Dictionary<ColorId, int>();
    private readonly Dictionary<ColorId, int> currentScores = new Dictionary<ColorId, int>();

    public LevelConfig CurrentConfig { get; private set; }
    public bool IsLevelEnded { get; private set; }

    public int CurrentLevelIndex => LevelProgress.CurrentLevelIndex;
    public int CurrentLevelNumber => CurrentLevelIndex + 1;

    public bool HasNextLevel =>
        levelConfigs != null &&
        LevelProgress.CurrentLevelIndex < levelConfigs.Length - 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadCurrentConfig();
        InitializeTargets();
    }

    private void Start()
    {
        OnScoreChanged?.Invoke();
    }

    private void LoadCurrentConfig()
    {
        if (levelConfigs == null || levelConfigs.Length == 0)
        {
            Debug.LogError("No LevelConfig assigned to LevelManager.");
            return;
        }

        if (LevelProgress.CurrentLevelIndex < 0)
        {
            LevelProgress.CurrentLevelIndex = 0;
        }

        if (LevelProgress.CurrentLevelIndex >= levelConfigs.Length)
        {
            LevelProgress.CurrentLevelIndex = levelConfigs.Length - 1;
        }

        CurrentConfig = levelConfigs[LevelProgress.CurrentLevelIndex];

        if (CurrentConfig == null)
        {
            Debug.LogError($"LevelConfig at index {LevelProgress.CurrentLevelIndex} is null.");
        }
    }

    private void InitializeTargets()
    {
        requiredScores.Clear();
        currentScores.Clear();
        IsLevelEnded = false;

        if (CurrentConfig == null)
            return;

        foreach (TargetGoal goal in CurrentConfig.TargetGoals)
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

        Debug.Log($"Level started: {CurrentConfig.LevelName}");
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

    public void RetryLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void GoToNextLevel()
    {
        if (!HasNextLevel)
        {
            Debug.Log("No next level. Restarting current level.");
            RetryLevel();
            return;
        }

        LevelProgress.CurrentLevelIndex++;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
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