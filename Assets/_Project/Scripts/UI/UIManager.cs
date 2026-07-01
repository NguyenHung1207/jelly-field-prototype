using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Runtime UI")]
    [SerializeField] private Canvas canvas;

    private Text levelText;
    private Transform targetContainer;
    private Text coinText;

    private GameObject resultPanel;
    private Text resultTitleText;
    private Button retryButton;
    private Button nextLevelButton;

    private readonly List<GameObject> targetItems = new List<GameObject>();
    private readonly Dictionary<ColorId, RectTransform> targetIconRects = new Dictionary<ColorId, RectTransform>();

    private Sprite roundedPanelSprite;
    private Sprite circleSprite;
    private Sprite roundedSmallSprite;

    private RectTransform canvasRect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        BuildUI();
        SubscribeLevelEvents();
        RefreshTargetText();
        HideResultPanel();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

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
        CreateSprites();

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

        canvasRect = canvas.GetComponent<RectTransform>();

        SetupCanvasScaler();
        CreateTopUI();
        CreateResultPanel();
    }

    private void CreateSprites()
    {
        roundedPanelSprite = CreateRoundedSprite(128, 128, 30, Color.white);
        roundedSmallSprite = CreateRoundedSprite(128, 128, 20, Color.white);
        circleSprite = CreateCircleSprite(128, Color.white);
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

    private void CreateTopUI()
    {
        CreateSettingsButton();
        CreateCoinUI();
        CreateLevelText();
        CreateTargetContainer();
    }

    private void CreateSettingsButton()
    {
        GameObject buttonObject = new GameObject("Settings Button");
        buttonObject.transform.SetParent(canvas.transform, false);

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = circleSprite;
        image.color = new Color(1f, 1f, 1f, 0.95f);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(78f, -78f);
        rect.sizeDelta = new Vector2(58f, 58f);

        Text gear = CreateText("Gear Icon", buttonObject.transform, "⚙", 34, TextAnchor.MiddleCenter, Color.gray);
        RectTransform gearRect = gear.GetComponent<RectTransform>();
        gearRect.anchorMin = Vector2.zero;
        gearRect.anchorMax = Vector2.one;
        gearRect.offsetMin = Vector2.zero;
        gearRect.offsetMax = Vector2.zero;
    }

    private void CreateCoinUI()
    {
        GameObject coinRoot = new GameObject("Coin UI");
        coinRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = coinRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(1f, 1f);
        rootRect.anchorMax = new Vector2(1f, 1f);
        rootRect.pivot = new Vector2(1f, 0.5f);
        rootRect.anchoredPosition = new Vector2(-62f, -78f);
        rootRect.sizeDelta = new Vector2(220f, 58f);

        GameObject bar = new GameObject("Coin Bar");
        bar.transform.SetParent(coinRoot.transform, false);

        Image barImage = bar.AddComponent<Image>();
        barImage.sprite = roundedPanelSprite;
        barImage.type = Image.Type.Sliced;
        barImage.color = new Color(0.45f, 0.53f, 0.58f, 0.85f);

        RectTransform barRect = bar.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0.18f);
        barRect.anchorMax = new Vector2(0.82f, 0.82f);
        barRect.offsetMin = Vector2.zero;
        barRect.offsetMax = Vector2.zero;

        coinText = CreateText("Coin Text", bar.transform, "0", 30, TextAnchor.MiddleRight, Color.white);
        RectTransform coinTextRect = coinText.GetComponent<RectTransform>();
        coinTextRect.anchorMin = Vector2.zero;
        coinTextRect.anchorMax = Vector2.one;
        coinTextRect.offsetMin = new Vector2(0f, 0f);
        coinTextRect.offsetMax = new Vector2(-28f, 0f);

        GameObject coin = new GameObject("Coin Icon");
        coin.transform.SetParent(coinRoot.transform, false);

        Image coinImage = coin.AddComponent<Image>();
        coinImage.sprite = circleSprite;
        coinImage.color = new Color(1f, 0.67f, 0.06f, 1f);

        RectTransform coinRect = coin.GetComponent<RectTransform>();
        coinRect.anchorMin = new Vector2(0.72f, 0f);
        coinRect.anchorMax = new Vector2(1f, 1f);
        coinRect.offsetMin = Vector2.zero;
        coinRect.offsetMax = Vector2.zero;

        GameObject coinInner = new GameObject("Coin Inner");
        coinInner.transform.SetParent(coin.transform, false);

        Image coinInnerImage = coinInner.AddComponent<Image>();
        coinInnerImage.sprite = circleSprite;
        coinInnerImage.color = new Color(1f, 0.88f, 0.2f, 1f);

        RectTransform innerRect = coinInner.GetComponent<RectTransform>();
        innerRect.anchorMin = new Vector2(0.18f, 0.18f);
        innerRect.anchorMax = new Vector2(0.82f, 0.82f);
        innerRect.offsetMin = Vector2.zero;
        innerRect.offsetMax = Vector2.zero;
    }

    private void CreateLevelText()
    {
        GameObject textObject = new GameObject("Level Text");
        textObject.transform.SetParent(canvas.transform, false);

        levelText = textObject.AddComponent<Text>();
        levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        levelText.fontSize = 34;
        levelText.fontStyle = FontStyle.Bold;
        levelText.alignment = TextAnchor.MiddleCenter;
        levelText.color = Color.white;
        levelText.text = "LEVEL 1";

        RectTransform rect = levelText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -98f);
        rect.sizeDelta = new Vector2(420f, 52f);
    }

    private void CreateTargetContainer()
    {
        GameObject containerObject = new GameObject("Target Container");
        containerObject.transform.SetParent(canvas.transform, false);

        Image image = containerObject.AddComponent<Image>();
        image.sprite = roundedPanelSprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.45f, 0.52f, 0.57f, 0.95f);

        RectTransform rect = containerObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -152f);
        rect.sizeDelta = new Vector2(310f, 74f);

        HorizontalLayoutGroup layout = containerObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 20f;
        layout.padding = new RectOffset(20, 20, 7, 7);
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        targetContainer = containerObject.transform;
    }

    private void RefreshTargetText()
    {
        if (LevelManager.Instance == null)
            return;

        string levelName = LevelManager.Instance.CurrentConfig != null
            ? LevelManager.Instance.CurrentConfig.LevelName
            : $"LEVEL {LevelManager.Instance.CurrentLevelNumber}";

        levelText.text = levelName.ToUpper();

        foreach (GameObject item in targetItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }

        targetItems.Clear();
        targetIconRects.Clear();

        IReadOnlyDictionary<ColorId, int> requiredScores = LevelManager.Instance.GetRequiredScores();
        IReadOnlyDictionary<ColorId, int> currentScores = LevelManager.Instance.GetCurrentScores();

        foreach (KeyValuePair<ColorId, int> pair in requiredScores)
        {
            ColorId color = pair.Key;
            int required = pair.Value;
            int current = currentScores.ContainsKey(color) ? currentScores[color] : 0;

            GameObject item = CreateTargetItem(color, current, required);
            targetItems.Add(item);
        }

        ResizeTargetContainer(requiredScores.Count);
    }

    private GameObject CreateTargetItem(ColorId colorId, int current, int required)
    {
        GameObject item = new GameObject($"Target {colorId}");
        item.transform.SetParent(targetContainer, false);

        RectTransform itemRect = item.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(108f, 56f);

        GameObject icon = new GameObject("Color Icon");
        icon.transform.SetParent(item.transform, false);

        Image iconImage = icon.AddComponent<Image>();
        iconImage.sprite = roundedSmallSprite;
        iconImage.type = Image.Type.Sliced;
        iconImage.color = GetUIColor(colorId);

        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(0f, 0f);
        iconRect.sizeDelta = new Vector2(54f, 54f);

        targetIconRects[colorId] = iconRect;

        GameObject shine = new GameObject("Icon Shine");
        shine.transform.SetParent(icon.transform, false);

        Image shineImage = shine.AddComponent<Image>();
        shineImage.sprite = roundedSmallSprite;
        shineImage.type = Image.Type.Sliced;
        shineImage.color = new Color(1f, 1f, 1f, 0.35f);

        RectTransform shineRect = shine.GetComponent<RectTransform>();
        shineRect.anchorMin = new Vector2(0.15f, 0.60f);
        shineRect.anchorMax = new Vector2(0.65f, 0.90f);
        shineRect.offsetMin = Vector2.zero;
        shineRect.offsetMax = Vector2.zero;

        int remaining = Mathf.Max(0, required - current);
        Text countText = CreateText("Target Count", item.transform, remaining.ToString(), 30, TextAnchor.MiddleLeft, Color.white);
        countText.fontStyle = FontStyle.Bold;

        RectTransform countRect = countText.GetComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0f, 0f);
        countRect.anchorMax = new Vector2(1f, 1f);
        countRect.offsetMin = new Vector2(66f, 0f);
        countRect.offsetMax = Vector2.zero;

        return item;
    }

    private void ResizeTargetContainer(int targetCount)
    {
        RectTransform rect = targetContainer.GetComponent<RectTransform>();

        if (targetCount <= 1)
        {
            rect.sizeDelta = new Vector2(172f, 74f);
        }
        else if (targetCount == 2)
        {
            rect.sizeDelta = new Vector2(300f, 74f);
        }
        else
        {
            rect.sizeDelta = new Vector2(420f, 74f);
        }
    }

    public IEnumerator PlayTargetCollectEffects(Dictionary<ColorId, int> scoreByColor, Vector3 worldStartPosition)
    {
        if (scoreByColor == null || scoreByColor.Count == 0)
            yield break;

        if (canvasRect == null)
            canvasRect = canvas.GetComponent<RectTransform>();

        float longestDelay = 0f;
        bool hasEffect = false;

        foreach (KeyValuePair<ColorId, int> pair in scoreByColor)
        {
            ColorId color = pair.Key;
            int count = pair.Value;

            if (count <= 0)
                continue;

            if (!targetIconRects.ContainsKey(color))
                continue;

            for (int i = 0; i < count; i++)
            {
                float delay = i * 0.07f;
                longestDelay = Mathf.Max(longestDelay, delay);
                hasEffect = true;

                StartCoroutine(PlaySingleTargetCollectEffect(
                    color,
                    worldStartPosition,
                    targetIconRects[color],
                    delay
                ));
            }
        }

        if (!hasEffect)
            yield break;

        yield return new WaitForSeconds(longestDelay + 0.45f);
    }

    private IEnumerator PlaySingleTargetCollectEffect(ColorId color, Vector3 worldStartPosition, RectTransform targetRect, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (targetRect == null)
            yield break;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCollect();
        }

        GameObject diamond = new GameObject($"Diamond_{color}");
        diamond.transform.SetParent(canvas.transform, false);

        Image image = diamond.AddComponent<Image>();
        image.sprite = roundedSmallSprite;
        image.type = Image.Type.Sliced;
        image.color = GetUIColor(color);

        RectTransform rect = diamond.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(28f, 28f);
        rect.localRotation = Quaternion.Euler(0f, 0f, 45f);

        Vector2 startPosition = WorldToCanvasPosition(worldStartPosition);
        Vector2 targetPosition = GetRectCanvasPosition(targetRect);

        startPosition += new Vector2(Random.Range(-36f, 36f), Random.Range(-18f, 22f));

        rect.anchoredPosition = startPosition;
        rect.localScale = Vector3.one;

        float duration = 0.38f;
        float elapsed = 0f;

        while (elapsed < duration && diamond != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            Vector2 arcOffset = new Vector2(0f, Mathf.Sin(t * Mathf.PI) * 90f);
            rect.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, easedT) + arcOffset;

            float scale = Mathf.Lerp(1f, 0.45f, easedT);
            rect.localScale = new Vector3(scale, scale, scale);

            yield return null;
        }

        if (diamond != null)
        {
            Destroy(diamond);
        }

        StartCoroutine(PulseTarget(targetRect));
    }

    private IEnumerator PulseTarget(RectTransform targetRect)
    {
        if (targetRect == null)
            yield break;

        Vector3 originalScale = targetRect.localScale;
        Vector3 punchScale = originalScale * 1.18f;

        float duration = 0.16f;
        float elapsed = 0f;

        while (elapsed < duration && targetRect != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (t < 0.5f)
            {
                targetRect.localScale = Vector3.Lerp(originalScale, punchScale, t / 0.5f);
            }
            else
            {
                targetRect.localScale = Vector3.Lerp(punchScale, originalScale, (t - 0.5f) / 0.5f);
            }

            yield return null;
        }

        if (targetRect != null)
        {
            targetRect.localScale = originalScale;
        }
    }

    private Vector2 WorldToCanvasPosition(Vector3 worldPosition)
    {
        Camera mainCamera = Camera.main;

        Vector2 screenPosition;

        if (mainCamera != null)
        {
            screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        }
        else
        {
            screenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            null,
            out Vector2 canvasPosition
        );

        return canvasPosition;
    }

    private Vector2 GetRectCanvasPosition(RectTransform targetRect)
    {
        Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            null,
            out Vector2 canvasPosition
        );

        return canvasPosition;
    }

    private void CreateResultPanel()
    {
        resultPanel = new GameObject("Result Panel");
        resultPanel.transform.SetParent(canvas.transform, false);

        Image panelImage = resultPanel.AddComponent<Image>();
        panelImage.sprite = roundedPanelSprite;
        panelImage.type = Image.Type.Sliced;
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);

        RectTransform panelRect = resultPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.12f, 0.35f);
        panelRect.anchorMax = new Vector2(0.88f, 0.65f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        resultTitleText = CreatePanelText("Result Title", resultPanel.transform, "LEVEL COMPLETE", 50, new Vector2(0f, 0.58f), new Vector2(1f, 0.95f));

        retryButton = CreateButton("Retry Button", resultPanel.transform, "Retry", new Vector2(0.08f, 0.12f), new Vector2(0.46f, 0.42f));
        retryButton.onClick.AddListener(RetryLevel);

        nextLevelButton = CreateButton("Next Level Button", resultPanel.transform, "Next", new Vector2(0.54f, 0.12f), new Vector2(0.92f, 0.42f));
        nextLevelButton.onClick.AddListener(GoToNextLevel);
    }

    private Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.text = content;

        return text;
    }

    private Text CreatePanelText(string name, Transform parent, string content, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
    {
        Text text = CreateText(name, parent, content, fontSize, TextAnchor.MiddleCenter, Color.white);

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
        image.sprite = roundedSmallSprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 1f, 1f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();

        button.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButton();
            }
        });

        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorMin;
        buttonRect.anchorMax = anchorMax;
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        Text labelText = CreatePanelText("Label", buttonObject.transform, label, 34, Vector2.zero, Vector2.one);
        labelText.color = Color.black;

        return button;
    }

    private void ShowLevelComplete()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWin();
        }

        resultTitleText.text = "LEVEL COMPLETE!";
        nextLevelButton.gameObject.SetActive(LevelManager.Instance != null && LevelManager.Instance.HasNextLevel);
        resultPanel.SetActive(true);
    }

    private void ShowLevelFailed()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLose();
        }

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
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RetryLevel();
        }
    }

    private void GoToNextLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.GoToNextLevel();
        }
    }

    private Color GetUIColor(ColorId colorId)
    {
        return colorId switch
        {
            ColorId.Yellow => new Color(1f, 0.82f, 0.05f),
            ColorId.Green => new Color(0.08f, 0.85f, 0.25f),
            ColorId.Red => new Color(0.95f, 0.12f, 0.1f),
            ColorId.Purple => new Color(0.58f, 0.12f, 1f),
            ColorId.Pink => new Color(1f, 0.25f, 0.78f),
            ColorId.Blue => new Color(0.08f, 0.75f, 1f),
            _ => new Color(0.8f, 0.8f, 0.8f)
        };
    }

    private Sprite CreateRoundedSprite(int width, int height, int radius, Color color)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;

        Color transparent = new Color(1f, 1f, 1f, 0f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inside = IsInsideRoundedRect(x, y, width, height, radius);
                texture.SetPixel(x, y, inside ? color : transparent);
            }
        }

        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius)
        );

        return sprite;
    }

    private Sprite CreateCircleSprite(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;

        Color transparent = new Color(1f, 1f, 1f, 0f);
        float center = (size - 1) * 0.5f;
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                bool inside = dx * dx + dy * dy <= radius * radius;
                texture.SetPixel(x, y, inside ? color : transparent);
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
    {
        int left = radius;
        int right = width - radius - 1;
        int bottom = radius;
        int top = height - radius - 1;

        if (x >= left && x <= right)
            return true;

        if (y >= bottom && y <= top)
            return true;

        int cx = x < left ? left : right;
        int cy = y < bottom ? bottom : top;

        int dx = x - cx;
        int dy = y - cy;

        return dx * dx + dy * dy <= radius * radius;
    }
}