using UnityEngine;

public partial class WheelRunnerBootstrap
{
    private void BuildFallbackMaterials()
    {
        greenMaterial = greenMaterial != null ? greenMaterial : CreateMaterial("Runtime Green", new Color(0.02f, 0.78f, 0.11f));
        blueMaterial = blueMaterial != null ? blueMaterial : CreateMaterial("Runtime Blue", new Color(0.02f, 0.44f, 1f));
        yellowMaterial = yellowMaterial != null ? yellowMaterial : CreateMaterial("Runtime Yellow", new Color(1f, 0.78f, 0.02f));
        trackMaterial = trackMaterial != null ? trackMaterial : CreateMaterial("Runtime Track", new Color(0.87f, 0.78f, 0.93f));
        wallMaterial = wallMaterial != null ? wallMaterial : CreateMaterial("Runtime Wall", new Color(0.42f, 0.31f, 0.68f));
        skinMaterial = skinMaterial != null ? skinMaterial : CreateMaterial("Runtime Skin", new Color(0.94f, 0.56f, 0.32f));
        shirtMaterial = shirtMaterial != null ? shirtMaterial : CreateMaterial("Runtime Shirt", new Color(0.05f, 0.33f, 0.45f));
        shortsMaterial = shortsMaterial != null ? shortsMaterial : CreateMaterial("Runtime Shorts", new Color(0.82f, 0.2f, 0.08f));
        hairMaterial = hairMaterial != null ? hairMaterial : CreateMaterial("Runtime Hair", new Color(0.04f, 0.035f, 0.03f));
        whiteMaterial = whiteMaterial != null ? whiteMaterial : CreateMaterial("Runtime White", Color.white);
        darkMaterial = darkMaterial != null ? darkMaterial : CreateMaterial("Runtime Dark", new Color(0.07f, 0.06f, 0.1f));
        spikeMaterial = spikeMaterial != null ? spikeMaterial : CreateMaterial("Runtime Spike", new Color(0.18f, 0.18f, 0.18f));
    }

    private void SetMaterial(GameObject gameObject, Material material)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }
    }

    private Material GetMaterial(WheelRunnerColor color)
    {
        switch (color)
        {
            case WheelRunnerColor.Blue:
                return blueMaterial;
            case WheelRunnerColor.Yellow:
                return yellowMaterial;
            default:
                return greenMaterial;
        }
    }

    private Color GetColor(WheelRunnerColor color)
    {
        Material material = GetMaterial(color);
        return material != null ? material.color : Color.white;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.name = name;
        material.color = color;
        return material;
    }

    private static Material CreateTransparentMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.name = name;
        Color c = color;
        c.a = 0.42f;
        material.color = c;
        return material;
    }

    private static void RemoveCollider(GameObject gameObject)
    {
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            DestroyImmediate(collider);
#else
            Destroy(collider);
#endif
        }
    }
}
