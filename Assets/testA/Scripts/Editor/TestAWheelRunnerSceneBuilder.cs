using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TestAWheelRunnerSceneBuilder
{
    private const string ArtPath = "Assets/testA/Arts";
    private const string ScenePath = "Assets/testA/Scenes/testA.scene";
    private const string PrefabPath = "Assets/testA/Arts/WheelRunnerBootstrap.prefab";

    [MenuItem("Tools/testA/Build Wheel Runner Scene")]
    public static void BuildScene()
    {
        EnsureFolders();

        Material green = CreateOrUpdateMaterial("Wheel_Green.mat", new Color(0.02f, 0.78f, 0.11f));
        Material blue = CreateOrUpdateMaterial("Pad_Blue.mat", new Color(0.02f, 0.44f, 1f));
        Material yellow = CreateOrUpdateMaterial("Pad_Yellow.mat", new Color(1f, 0.78f, 0.02f));
        Material track = CreateOrUpdateMaterial("Track_Pastel.mat", new Color(0.87f, 0.78f, 0.93f));
        Material wall = CreateOrUpdateMaterial("Wall_Purple.mat", new Color(0.42f, 0.31f, 0.68f));
        Material skin = CreateOrUpdateMaterial("Runner_Skin.mat", new Color(0.94f, 0.56f, 0.32f));
        Material shirt = CreateOrUpdateMaterial("Runner_Shirt.mat", new Color(0.05f, 0.33f, 0.45f));
        Material shorts = CreateOrUpdateMaterial("Runner_Shorts.mat", new Color(0.82f, 0.2f, 0.08f));
        Material hair = CreateOrUpdateMaterial("Runner_Hair.mat", new Color(0.04f, 0.035f, 0.03f));
        Material white = CreateOrUpdateMaterial("Runner_White.mat", Color.white);
        Material dark = CreateOrUpdateMaterial("Dark_Detail.mat", new Color(0.07f, 0.06f, 0.1f));

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject gameObject = new GameObject("Wheel Runner Bootstrap");
        Component game = gameObject.AddComponent(typeof(WheelRunnerBootstrap));
        AssignMaterials(game, green, blue, yellow, track, wall, skin, shirt, shorts, hair, white, dark);

        PrefabUtility.SaveAsPrefabAsset(gameObject, PrefabPath);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("testA wheel runner scene generated at " + ScenePath);
    }

    private static void EnsureFolders()
    {
        CreateFolderIfMissing("Assets", "testA");
        CreateFolderIfMissing("Assets/testA", "Arts");
        CreateFolderIfMissing("Assets/testA", "Scripts");
        CreateFolderIfMissing("Assets/testA", "Scenes");
    }

    private static void CreateFolderIfMissing(string parent, string folder)
    {
        string path = parent + "/" + folder;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static Material CreateOrUpdateMaterial(string fileName, Color color)
    {
        string path = ArtPath + "/" + fileName;
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.name = System.IO.Path.GetFileNameWithoutExtension(fileName);
        material.color = color;
        material.SetFloat("_Glossiness", 0.25f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void AssignMaterials(Component game, Material green, Material blue, Material yellow, Material track, Material wall, Material skin, Material shirt, Material shorts, Material hair, Material white, Material dark)
    {
        SerializedObject serializedObject = new SerializedObject(game);
        serializedObject.FindProperty("greenMaterial").objectReferenceValue = green;
        serializedObject.FindProperty("blueMaterial").objectReferenceValue = blue;
        serializedObject.FindProperty("yellowMaterial").objectReferenceValue = yellow;
        serializedObject.FindProperty("trackMaterial").objectReferenceValue = track;
        serializedObject.FindProperty("wallMaterial").objectReferenceValue = wall;
        serializedObject.FindProperty("skinMaterial").objectReferenceValue = skin;
        serializedObject.FindProperty("shirtMaterial").objectReferenceValue = shirt;
        serializedObject.FindProperty("shortsMaterial").objectReferenceValue = shorts;
        serializedObject.FindProperty("hairMaterial").objectReferenceValue = hair;
        serializedObject.FindProperty("whiteMaterial").objectReferenceValue = white;
        serializedObject.FindProperty("darkMaterial").objectReferenceValue = dark;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}
