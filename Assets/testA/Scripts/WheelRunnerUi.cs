using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class WheelRunnerBootstrap
{
    private void BuildUi()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        Register(eventSystem);
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        GameObject canvasObject = new GameObject("Game UI");
        Register(canvasObject);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        scoreText = CreateText(canvasObject.transform, "Score Text", "分数\n0", new Vector2(28f, -150f), TextAnchor.UpperLeft, 32, whiteMaterial.color);
        heightText = CreateText(canvasObject.transform, "Height Text", "高度 0.72", new Vector2(-28f, -150f), TextAnchor.UpperRight, 32, whiteMaterial.color);
        //messageText = CreateText(canvasObject.transform, "Message Text", "拖动左右移动；跑道循环无尽，速度会越来越快", new Vector2(0f, 72f), TextAnchor.LowerCenter, 20, whiteMaterial.color);
        BuildTutorialGuide(canvasObject.transform);
        BuildRetryButton(canvasObject.transform);

        GameObject sliderObject = new GameObject("Level Progress");
        Register(sliderObject);
        sliderObject.transform.SetParent(canvasObject.transform, false);
        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 1f);
        sliderRect.anchorMax = new Vector2(0.5f, 1f);
        sliderRect.pivot = new Vector2(0.5f, 1f);
        sliderRect.anchoredPosition = new Vector2(0f, -150f);
        sliderRect.sizeDelta = new Vector2(320f, 28f);
        progressSlider = sliderObject.AddComponent<Slider>();
        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;
        progressSlider.interactable = false;

        GameObject background = CreateUiImage(sliderObject.transform, "Background", new Color(1f, 1f, 1f, 0.9f));
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area");
        Register(fillArea);
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0f);
        fillAreaRect.anchorMax = new Vector2(1f, 1f);
        fillAreaRect.offsetMin = new Vector2(4f, 4f);
        fillAreaRect.offsetMax = new Vector2(-4f, -4f);

        GameObject fill = CreateUiImage(fillArea.transform, "Fill", GetColor(wheelColor));
        progressSlider.fillRect = fill.GetComponent<RectTransform>();
        progressSlider.targetGraphic = background.GetComponent<Image>();
    }

    private Text CreateText(Transform parent, string name, string text, Vector2 anchoredPosition, TextAnchor alignment, int fontSize, Color color)
    {
        GameObject textObject = new GameObject(name);
        Register(textObject);
        textObject.transform.SetParent(parent, false);
        Text uiText = textObject.AddComponent<Text>();
        uiText.text = text;
        uiText.font = TuanJieFontHelper.GetSystemFontWithChinese();
        uiText.fontSize = fontSize;
        uiText.fontStyle = FontStyle.Bold;
        uiText.alignment = alignment;
        uiText.color = color;
        uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
        uiText.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        float anchorX = alignment == TextAnchor.UpperRight ? 1f : alignment == TextAnchor.LowerCenter ? 0.5f : 0f;
        float anchorY = alignment == TextAnchor.LowerCenter ? 0f : 1f;
        rectTransform.anchorMin = new Vector2(anchorX, anchorY);
        rectTransform.anchorMax = rectTransform.anchorMin;
        rectTransform.pivot = new Vector2(anchorX, anchorY);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(720f, 110f);
        return uiText;
    }

    private GameObject CreateUiImage(Transform parent, string name, Color color)
    {
        GameObject imageObject = new GameObject(name);
        Register(imageObject);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        return imageObject;
    }

    private Image tutorialHandImage;

    private void BuildTutorialGuide(Transform parent)
    {
        GameObject rootObject = new GameObject("Tutorial Guide");
        Register(rootObject);
        rootObject.transform.SetParent(parent, false);
        tutorialRoot = rootObject.AddComponent<RectTransform>();
        tutorialRoot.anchorMin = new Vector2(0.5f, 0f);
        tutorialRoot.anchorMax = new Vector2(0.5f, 0f);
        tutorialRoot.pivot = new Vector2(0.5f, 0.5f);
        tutorialRoot.anchoredPosition = new Vector2(0f, 700f);
        tutorialRoot.sizeDelta = new Vector2(360f, 190f);

        BuildTutorialHandSprite(rootObject.transform);

       /* Text title = CreateText(rootObject.transform, "Tutorial Text", "Drag to Move", new Vector2(0f, 50f), TextAnchor.MiddleCenter, 34, whiteMaterial.color);
        title.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        title.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        title.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        title.rectTransform.sizeDelta = new Vector2(420f, 70f);
        title.fontStyle = FontStyle.Bold;*/

        Text arrow = CreateText(rootObject.transform, "Tutorial Swipe Arrows", "<      >", new Vector2(0f, 10f), TextAnchor.MiddleCenter, 38, new Color(0f, 1f, 0f, 0.88f));
        arrow.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        arrow.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        arrow.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        arrow.rectTransform.sizeDelta = new Vector2(260f, 60f);
    }

    private void BuildTutorialHandSprite(Transform parent)
    {
        GameObject handObject = new GameObject("Tutorial Hand");
        Register(handObject);
        handObject.transform.SetParent(parent, false);
        tutorialHand = handObject.AddComponent<RectTransform>();
        tutorialHand.anchorMin = new Vector2(0.5f, 0.5f);
        tutorialHand.anchorMax = new Vector2(0.5f, 0.5f);
        tutorialHand.pivot = new Vector2(0.5f, 0.5f);
        tutorialHand.anchoredPosition = new Vector2(0f, -46f);
        tutorialHand.sizeDelta = new Vector2(90f, 90f);

        tutorialHandImage = handObject.AddComponent<Image>();
        tutorialHandImage.raycastTarget = false;

        Texture2D tex = CreateHandTexture(64, 64);
        Sprite handSprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        handSprite.name = "Runtime Tutorial Hand";

        tutorialHandImage.sprite = handSprite;
        tutorialHandImage.color = whiteMaterial.color;
    }

    private static Texture2D CreateHandTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color32[] pixels = new Color32[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(0, 0, 0, 0);

        int SetIdx(int x, int y)
        {
            if (x >= 0 && x < w && y >= 0 && y < h) return y * w + x;
            return -1;
        }

        void SetPixel(int x, int y, byte r, byte g, byte b)
        {
            int idx = SetIdx(x, y);
            if (idx >= 0) pixels[idx] = new Color32(r, g, b, 255);
        }

        void FillCircle(int cx, int cy, int rad, byte r, byte g, byte b)
        {
            for (int dy = -rad - 1; dy <= rad + 1; dy++)
            for (int dx = -rad - 1; dx <= rad + 1; dx++)
                if (dx * dx + dy * dy <= rad * rad)
                    SetPixel(cx + dx, cy + dy, r, g, b);
        }

        void FillRect(int x1, int y1, int x2, int y2, byte r, byte g, byte b)
        {
            for (int y = y1; y < y2; y++)
            for (int x = x1; x < x2; x++)
                SetPixel(x, y, r, g, b);
        }

        byte c = 255;
        FillCircle(32, 54, 16, c, c, c);
        FillRect(24, 10, 40, 50, c, c, c);
        FillCircle(26, 13, 7, c, c, c);
        FillCircle(38, 13, 7, c, c, c);
        FillCircle(32, 8,  7, c, c, c);
        FillCircle(20, 24, 6, c, c, c);
        FillCircle(44, 24, 6, c, c, c);

        tex.SetPixels32(pixels);
        tex.Apply();
        return tex;
    }

    private void BuildRetryButton(Transform parent)
    {
        retryButtonObject = new GameObject("Retry Button");
        Register(retryButtonObject);
        retryButtonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = retryButtonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, -80f);
        rectTransform.sizeDelta = new Vector2(260f, 82f);

        Image background = retryButtonObject.AddComponent<Image>();
        background.color = new Color(0.1f, 0.82f, 0.25f, 0.96f);

        Button button = retryButtonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(RestartScene);

        BuildRetryButtonLabel(retryButtonObject.transform);

        retryButtonObject.SetActive(false);
    }

    private void BuildRetryButtonLabel(Transform parent)
    {
        GameObject labelObject = new GameObject("Retry Button Label");
        Register(labelObject);
        labelObject.transform.SetParent(parent, false);

        RectTransform labelRect = labelObject.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.sizeDelta = Vector2.zero;

        ChineseTextRenderer label = labelObject.AddComponent<ChineseTextRenderer>();
        label.SetText("重新开始");
        label.fontSize = 34;
        label.textColor = whiteMaterial.color;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.textureWidth = 260;
        label.textureHeight = 82;
    }

    private void UpdateTutorialGuide()
    {
        if (tutorialRoot == null)
        {
            return;
        }

        tutorialRoot.gameObject.SetActive(true);
        if (tutorialHand == null)
        {
            return;
        }

        float x = Mathf.Sin(Time.unscaledTime * 4f) * 58f;
        float tilt = Mathf.Sin(Time.unscaledTime * 4f) * -10f;
        tutorialHand.anchoredPosition = new Vector2(x, -46f);
        tutorialHand.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }

    private void StartGame()
    {
        gameStarted = true;
        isDragging = true;
        lastPointerX = Input.mousePosition.x;
        nextFallingBallSpawnTime = Time.time + 1.4f;

        if (tutorialRoot != null)
        {
            tutorialRoot.gameObject.SetActive(false);
        }

        if (messageText != null)
        {
            messageText.text = string.Empty;
        }
    }

    private void ShowRetryButton()
    {
        if (retryButtonObject != null)
        {
            retryButtonObject.SetActive(true);
        }

        if (messageText != null)
        {
            messageText.text = "挑战失败";
            messageText.color = whiteMaterial.color;
        }
    }

    private void ShowLapMessage()
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = "第 " + (lapCount + 1) + " 圈";
        messageText.color = whiteMaterial.color;
    }

    private void ShowPadMessage(bool isMatch, WheelRunnerColor padColor, bool radiusChanged)
    {
        if (messageText == null)
        {
            return;
        }

        if (isMatch && !radiusChanged)
        {
            messageText.text = "已达最大高度！";
        }
        else
        {
            messageText.text = isMatch ? "颜色相同：轮子升高！" : "颜色不同：轮子降低并减速！";
        }

        messageText.color = isMatch ? GetColor(wheelColor) : GetColor(padColor);
    }

    private void ShowBaffleMessage(WheelRunnerColor baffleColor)
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = "Very Good!";
        messageText.color = GetColor(baffleColor);
    }

    private void ShowSpikeTrapMessage()
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = "碰到地刺：轮子大幅降低并严重减速！";
        messageText.color = spikeMaterial != null ? spikeMaterial.color : Color.gray;
    }

    private void ShowFallingBallMessage(WheelRunnerColor ballColor)
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = "撞到异色球：轮子大幅降低并减速！";
        messageText.color = GetColor(ballColor);
    }

    private void UpdateUi()
    {
        if (scoreText != null)
        {
           // scoreText.text = "分数\n" + score + "\nLap " + (lapCount + 1);
           scoreText.text = "分数\n" + score ;
        }

        if (heightText != null)
        {
            //heightText.text = "Color " + wheelColor + "\n高度 " + targetRadius.ToString("0.00");
            heightText.text = "高度\n" + targetRadius.ToString("0.00");
        }

        if (progressSlider != null)
        {
            progressSlider.value = Mathf.Clamp01((GetLoopZ(zPosition) - TrackLoopStartZ) / TrackLoopLength);
            if (progressSlider.fillRect != null)
            {
                Image fillImage = progressSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = GetColor(wheelColor);
                }
            }
        }
    }
}
