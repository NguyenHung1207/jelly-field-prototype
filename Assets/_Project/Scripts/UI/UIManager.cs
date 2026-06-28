using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Runtime UI")]
    [SerializeField] private Canvas canvas;

    private Text targetText;
    private GameObject resultPanel;
    private Text resultTitleText;
    private Button retryButton;
    private Button nextLevelButton;

    private void Start()
    {
        BuildUI();
        SubscribeLevelEvents();
        RefreshTargetText();
        HideResultPanel();
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance == null)
            return;

        LevelManager.Instance.OnScoreChanged -= RefreshTargetText;
        LevelManager.Instance.OnLevelCompleted -= ShowLevelComplete;
        LevelManager.Instance.OnLevelFailed -= ShowLevelFailed;
    }

    private void SubscribeLevelEvents()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogWarning("LevelManager is missing. UI cannot subscribe to level events.");
            return;
        }

        LevelManager.Instance.OnScoreChanged += RefreshTargetText;
        LevelManager.Instance.OnLevelCompleted += ShowLevelComplete;
        LevelManager.Instance.OnLevelFailed += ShowLevelFailed;
    }

    private void BuildUI()
    {
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        SetupCanvasScaler();
        CreateTargetText();
        CreateResultPanel();
    }

    private void SetupCanvasScaler()
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();

        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
    }

    private void CreateTargetText()
    {
        GameObject textObject = new GameObject("Target Text");
        textObject.transform.SetParent(canvas.transform, false);

        targetText = textObject.AddComponent<Text>();
        targetText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        targetText.fontSize = 42;
        targetText.alignment = TextAnchor.UpperCenter;
        targetText.color = Color.white;
        targetText.text = "Target";

        RectTransform rectTransform = targetText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.05f, 0.82f);
        rectTransform.anchorMax = new Vector2(0.95f, 0.97f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private void CreateResultPanel()
    {
        resultPanel = new GameObject("Result Panel");
        resultPanel.transform.SetParent(canvas.transform, false);

        Image panelImage = resultPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);

        RectTransform panelRect = resultPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.12f, 0.35f);
        panelRect.anchorMax = new Vector2(0.88f, 0.65f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        resultTitleText = CreatePanelText("Result Title", resultPanel.transform, "LEVEL COMPLETE", 52, new Vector2(0f, 0.58f), new Vector2(1f, 0.95f));

        retryButton = CreateButton("Retry Button", resultPanel.transform, "Retry", new Vector2(0.08f, 0.12f), new Vector2(0.46f, 0.42f));
        retryButton.onClick.AddListener(RetryLevel);

        nextLevelButton = CreateButton("Next Level Button", resultPanel.transform, "Next", new Vector2(0.54f, 0.12f), new Vector2(0.92f, 0.42f));
        nextLevelButton.onClick.AddListener(GoToNextLevelPlaceholder);
    }

    private Text CreatePanelText(string name, Transform parent, string content, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = content;

        RectTransform rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        return text;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();

        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorMin;
        buttonRect.anchorMax = anchorMax;
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        Text labelText = CreatePanelText("Label", buttonObject.transform, label, 36, Vector2.zero, Vector2.one);
        labelText.color = Color.black;

        return button;
    }

    private void RefreshTargetText()
    {
        if (targetText == null)
            return;

        if (LevelManager.Instance == null)
        {
            targetText.text = "Target: missing LevelManager";
            return;
        }

        IReadOnlyDictionary<ColorId, int> requiredScores = LevelManager.Instance.GetRequiredScores();
        IReadOnlyDictionary<ColorId, int> currentScores = LevelManager.Instance.GetCurrentScores();

        StringBuilder builder = new StringBuilder();

        foreach (KeyValuePair<ColorId, int> pair in requiredScores)
        {
            ColorId color = pair.Key;
            int required = pair.Value;
            int current = currentScores.ContainsKey(color) ? currentScores[color] : 0;

            builder.Append(color);
            builder.Append(": ");
            builder.Append(current);
            builder.Append("/");
            builder.Append(required);
            builder.Append("   ");
        }

        targetText.text = builder.ToString();
    }

    private void ShowLevelComplete()
    {
        resultTitleText.text = "LEVEL COMPLETE!";
        nextLevelButton.gameObject.SetActive(true);
        resultPanel.SetActive(true);
    }

    private void ShowLevelFailed()
    {
        resultTitleText.text = "LEVEL FAILED!";
        nextLevelButton.gameObject.SetActive(false);
        resultPanel.SetActive(true);
    }

    private void HideResultPanel()
    {
        resultPanel.SetActive(false);
    }

    private void RetryLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private void GoToNextLevelPlaceholder()
    {
        Debug.Log("Next Level button clicked. Multi-level flow will be added in the next step.");
        RetryLevel();
    }
}