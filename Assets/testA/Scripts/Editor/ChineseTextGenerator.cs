#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;

namespace ChineseTextTool
{
    /// <summary>
    /// 编辑器工具：将中文字符渲染成 PNG 图片。
    /// 放在 Editor 目录下，仅编辑器使用。
    /// 使用 System.Drawing，Build 时不打包。
    /// </summary>
    public static class ChineseTextGenerator
    {
        private static readonly string[] FontNames = {
            "Microsoft YaHei",
            "SimHei",
            "SimSun",
            "Arial Unicode MS",
            "MS Gothic",
            "Malgun Gothic",
            "Noto Sans CJK SC",
            "PingFang SC"
        };

        private static string _cachedFontName;

        public static string PreferredFontName
        {
            get
            {
                if (_cachedFontName != null) return _cachedFontName;
                foreach (string name in FontNames)
                {
                    try
                    {
                        using (var f = new System.Drawing.Font(name, 12))
                        {
                            if (f != null && f.Name == name)
                            {
                                _cachedFontName = name;
                                Debug.Log("[ChineseTextGenerator] Using font: " + name);
                                return name;
                            }
                        }
                    }
                    catch { }
                }
                _cachedFontName = "Arial";
                Debug.LogWarning("[ChineseTextGenerator] No Chinese font found, falling back to Arial.");
                return _cachedFontName;
            }
        }

        public static Texture2D RenderText(
            string text,
            int fontSize,
            Color textColor,
            System.Drawing.FontStyle fontStyle = System.Drawing.FontStyle.Bold,
            int padding = 10)
        {
            string fontName = PreferredFontName;

            // 先测量文字尺寸
            float measuredW = 0f, measuredH = 0f;
            using (var tempFont = new System.Drawing.Font(fontName, fontSize, fontStyle, System.Drawing.GraphicsUnit.Pixel))
            using (var tempBmp = new System.Drawing.Bitmap(1, 1))
            using (var tempG = System.Drawing.Graphics.FromImage(tempBmp))
            {
                tempG.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                var sizeF = tempG.MeasureString(text, tempFont);
                measuredW = sizeF.Width;
                measuredH = sizeF.Height;
            }

            int width = Mathf.CeilToInt(measuredW) + padding * 2;
            int height = Mathf.CeilToInt(measuredH) + padding * 2;

            using (var font = new System.Drawing.Font(fontName, fontSize, fontStyle, System.Drawing.GraphicsUnit.Pixel))
            using (var bmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.Clear(System.Drawing.Color.Transparent);

                int r = (int)(textColor.r * 255);
                int g2 = (int)(textColor.g * 255);
                int b = (int)(textColor.b * 255);
                int a = (int)(textColor.a * 255);

                using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(a, r, g2, b)))
                {
                    float x = padding;
                    float y = (height - measuredH) * 0.5f;
                    g.DrawString(text, font, brush, x, y);
                }

                bmp.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

                var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                tex.hideFlags = HideFlags.None;

                var pixels = new Color[width * height];
                for (int row = 0; row < height; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        var px = bmp.GetPixel(col, row);
                        pixels[row * width + col] = new Color(
                            px.R / 255f,
                            px.G / 255f,
                            px.B / 255f,
                            px.A / 255f);
                    }
                }

                tex.SetPixels(pixels);
                tex.Apply();
                return tex;
            }
        }

        public static void RenderAndSave(
            string text,
            string outputPath,
            int fontSize,
            Color textColor,
            System.Drawing.FontStyle fontStyle = System.Drawing.FontStyle.Bold,
            int padding = 10)
        {
            var tex = RenderText(text, fontSize, textColor, fontStyle, padding);
            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(outputPath, png);
            Debug.Log("[ChineseTextGenerator] Saved: " + outputPath + " (" + tex.width + "x" + tex.height + ")");
            UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}
#endif
