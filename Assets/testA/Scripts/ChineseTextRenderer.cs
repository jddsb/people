using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 运行时：加载预生成的文字图片并显示。
/// 完全不依赖任何字体 API。
/// </summary>
[RequireComponent(typeof(Image))]
public class ChineseTextRenderer : MonoBehaviour
{
    [Header("文字内容")]
    [SerializeField] private string _text = "重新开始";

    [Header("字体样式（仅用于 Editor 生成时）")]
    [SerializeField] private int _fontSize = 48;
    [SerializeField] private Color _textColor = Color.white;
    [SerializeField] private FontStyle _fontStyle = FontStyle.Bold;
    [SerializeField] private TextAnchor _alignment = TextAnchor.MiddleCenter;

    [Header("渲染分辨率（仅用于 Editor 生成时）")]
    [SerializeField] private int _textureWidth = 256;
    [SerializeField] private int _textureHeight = 64;

    private Image _image;
    private bool _loaded;

    public string text => _text;
    public int fontSize
    {
        get => _fontSize;
        set => _fontSize = value;
    }
    public Color textColor
    {
        get => _textColor;
        set => _textColor = value;
    }
    public FontStyle fontStyle
    {
        get => _fontStyle;
        set => _fontStyle = value;
    }
    public TextAnchor alignment
    {
        get => _alignment;
        set => _alignment = value;
    }
    public int textureWidth
    {
        get => _textureWidth;
        set => _textureWidth = value;
    }
    public int textureHeight
    {
        get => _textureHeight;
        set => _textureHeight = value;
    }

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.raycastTarget = false;
        _image.enabled = false; // 初始隐藏，等 LoadSprite 后再显示
    }

    private void OnEnable()
    {
        LoadSprite();
    }

    private void OnDisable()
    {
        _image.enabled = false;
        _loaded = false;
    }

    private void LoadSprite()
    {
        if (_loaded) return;

        string spritePath = "ChineseText/" + _text;
        var tex = Resources.Load<Texture2D>(spritePath);

        if (tex == null)
        {
            Debug.LogError("[ChineseTextRenderer] 未找到文字图片: " + spritePath
                + "\n请先在 Unity 菜单: Tools > Generate Chinese Text Images 生成图片。");
            return;
        }

        var sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            tex.width);
        sprite.name = "ChineseText_" + _text;

        _image.sprite = sprite;
        _image.color = Color.white;
        _image.enabled = true;
        _loaded = true;
    }

    public void SetText(string newText)
    {
        if (_text == newText) return;
        _text = newText;
        _loaded = false;
        _image.sprite = null;
        LoadSprite();
    }

    public void SetColor(Color color)
    {
        // 文字颜色由图片决定，Image.color 设为白色让图片保持原色
        _image.color = color;
    }
}
