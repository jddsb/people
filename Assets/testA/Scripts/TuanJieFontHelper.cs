using System;
using UnityEngine;
using TMPro;

public static class TuanJieFontHelper
{
    private static Font cachedSystemFont;
    private static bool fontSearched;

    public static Font GetSystemFontWithChinese()
    {
        if (fontSearched) return cachedSystemFont;
        fontSearched = true;

        string[] chineseFonts = {
            "Arial Unicode MS",
            "Noto Sans CJK SC",
            "Noto Sans CJK TC",
            "Noto Sans CJK JP",
            "Microsoft YaHei",
            "PingFang SC",
            "PingFang TC",
            "PingFang HK",
            "Source Han Sans SC",
            "Source Han Sans CN",
            "Heiti SC",
            "STHeiti",
            "SimHei",
            "SimSun",
            "FZLanTingHei-R",
            "FZLanTingHei-B"
        };

        foreach (string fontName in chineseFonts)
        {
            Font f = Font.CreateDynamicFontFromOSFont(fontName, 12);
            if (f != null)
            {
                cachedSystemFont = f;
                Debug.Log("[TuanJieFontHelper] Using system font: " + fontName);
                return f;
            }
        }

        Font arial = Font.CreateDynamicFontFromOSFont("Arial", 12);
        if (arial != null)
        {
            cachedSystemFont = arial;
            Debug.Log("[TuanJieFontHelper] Falling back to Arial");
            return arial;
        }

        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    public static TMP_FontAsset GetTMPFontAsset()
    {
        TMP_FontAsset defaultAsset = TMP_Settings.defaultFontAsset;
        if (defaultAsset != null)
        {
            return defaultAsset;
        }

        TMP_FontAsset[] found = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        if (found != null && found.Length > 0)
        {
            return found[0];
        }

        return null;
    }
}
