#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ChineseTextTool
{
    public static class GenerateChineseTextMenu
    {
        private const string OutputPath = "Assets/Resources/ChineseText/重新开始.png";

        [MenuItem("Tools/Generate Chinese Text Images")]
        public static void Generate()
        {
            EnsureOutputFolder();

            var tex = ChineseTextGenerator.RenderText(
                text: "重新开始",
                fontSize: 48,
                textColor: Color.white,
                fontStyle: System.Drawing.FontStyle.Bold,
                padding: 8
            );

            byte[] png = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(OutputPath, png);
            Debug.Log("[Generate] Saved: " + OutputPath + " (" + tex.width + "x" + tex.height + ")");

            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();
        }

        private static void EnsureOutputFolder()
        {
            string folder = "Assets/Resources/ChineseText";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "ChineseText");
            }
        }
    }
}
#endif
