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

        scoreText = CreateText(canvasObject.transform, "Score Text", "Score\n0", new Vector2(28f, -28f), TextAnchor.UpperLeft, 32, whiteMaterial.color);
        heightText = CreateText(canvasObject.transform, "Height Text", "Height 0.72", new Vector2(-28f, -28f), TextAnchor.UpperRight, 24, whiteMaterial.color);
        messageText = CreateText(canvasObject.transform, "Message Text", "拖动左右移动；跑道循环无尽，速度会越来越快，R 重开", new Vector2(0f, 72f), TextAnchor.LowerCenter, 20, whiteMaterial.color);
        BuildTutorialGuide(canvasObject.transform);

        GameObject sliderObject = new GameObject("Level Progress");
        Register(sliderObject);
        sliderObject.transform.SetParent(canvasObject.transform, false);
        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 1f);
        sliderRect.anchorMax = new Vector2(0.5f, 1f);
        sliderRect.pivot = new Vector2(0.5f, 1f);
        sliderRect.anchoredPosition = new Vector2(0f, -30f);
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
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

    private void BuildTutorialGuide(Transform parent)
    {
        GameObject rootObject = new GameObject("Tutorial Guide");
        Register(rootObject);
        rootObject.transform.SetParent(parent, false);
        tutorialRoot = rootObject.AddComponent<RectTransform>();
        tutorialRoot.anchorMin = new Vector2(0.5f, 0f);
        tutorialRoot.anchorMax = new Vector2(0.5f, 0f);
        tutorialRoot.pivot = new Vector2(0.5f, 0.5f);
        tutorialRoot.anchoredPosition = new Vector2(0f, 150f);
        tutorialRoot.sizeDelta = new Vector2(360f, 190f);

        Text title = CreateText(rootObject.transform, "Tutorial Text", "Drag to Move", new Vector2(0f, 50f), TextAnchor.MiddleCenter, 34, whiteMaterial.color);
        title.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        title.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        title.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        title.rectTransform.sizeDelta = new Vector2(420f, 70f);
        title.fontStyle = FontStyle.Bold;

        Text arrow = CreateText(rootObject.transform, "Tutorial Swipe Arrows", "<      >", new Vector2(0f, 2f), TextAnchor.MiddleCenter, 38, new Color(1f, 1f, 1f, 0.88f));
        arrow.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        arrow.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        arrow.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        arrow.rectTransform.sizeDelta = new Vector2(260f, 60f);

        Text hand = CreateText(rootObject.transform, "Tutorial Hand", "☝", new Vector2(0f, -46f), TextAnchor.MiddleCenter, 58, whiteMaterial.color);
        hand.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        hand.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        hand.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        hand.rectTransform.sizeDelta = new Vector2(120f, 100f);
        tutorialHand = hand.rectTransform;
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

        if (tutorialRoot != null)
        {
            tutorialRoot.gameObject.SetActive(false);
        }

        if (messageText != null)
        {
            messageText.text = string.Empty;
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

    private void UpdateUi()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score\n" + score + "\nLap " + (lapCount + 1);
        }

        if (heightText != null)
        {
            heightText.text = "Color " + wheelColor + "\nHeight " + targetRadius.ToString("0.00");
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
