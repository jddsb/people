using UnityEngine;

public partial class WheelRunnerBootstrap
{
    private void BuildRunner()
    {
        GameObject root = new GameObject("Wheel Runner");
        Register(root);
        runnerRoot = root.transform;
        runnerRoot.position = new Vector3(xPosition, 0f, zPosition);

        GameObject wheelObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Register(wheelObject);
        wheelObject.name = "Rolling Color Wheel";
        wheelObject.transform.SetParent(runnerRoot, false);
        wheelObject.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        wheel = wheelObject.transform;
        SetMaterial(wheelObject, GetMaterial(wheelColor));

        GameObject character = new GameObject("Runner Character");
        Register(character);
        character.transform.SetParent(runnerRoot, false);
        characterRoot = character.transform;
        BuildCharacter(characterRoot);
    }

    private void BuildCharacter(Transform parent)
    {
        CreatePrimitiveChild(parent, "Body", PrimitiveType.Capsule, new Vector3(0f, 1.15f, -0.1f), new Vector3(0.42f, 0.58f, 0.42f), shirtMaterial);
        CreatePrimitiveChild(parent, "Head", PrimitiveType.Sphere, new Vector3(0f, 1.93f, -0.1f), new Vector3(0.42f, 0.42f, 0.42f), skinMaterial);
        CreatePrimitiveChild(parent, "Hair", PrimitiveType.Sphere, new Vector3(0f, 2.08f, -0.13f), new Vector3(0.43f, 0.24f, 0.43f), hairMaterial);
        CreatePrimitiveChild(parent, "Shorts", PrimitiveType.Cube, new Vector3(0f, 0.68f, -0.1f), new Vector3(0.48f, 0.32f, 0.36f), shortsMaterial);
        CreateLimb(parent, "Left Arm", new Vector3(-0.37f, 1.16f, -0.1f), new Vector3(0.14f, 0.56f, 0.14f), Quaternion.Euler(0f, 0f, -14f), skinMaterial);
        CreateLimb(parent, "Right Arm", new Vector3(0.37f, 1.16f, -0.1f), new Vector3(0.14f, 0.56f, 0.14f), Quaternion.Euler(0f, 0f, 14f), skinMaterial);
        CreateLimb(parent, "Left Leg", new Vector3(-0.15f, 0.23f, -0.08f), new Vector3(0.15f, 0.52f, 0.15f), Quaternion.Euler(8f, 0f, 4f), skinMaterial);
        CreateLimb(parent, "Right Leg", new Vector3(0.15f, 0.23f, -0.08f), new Vector3(0.15f, 0.52f, 0.15f), Quaternion.Euler(-8f, 0f, -4f), skinMaterial);
        CreatePrimitiveChild(parent, "Left Shoe", PrimitiveType.Cube, new Vector3(-0.15f, -0.06f, -0.02f), new Vector3(0.2f, 0.1f, 0.36f), whiteMaterial);
        CreatePrimitiveChild(parent, "Right Shoe", PrimitiveType.Cube, new Vector3(0.15f, -0.06f, -0.02f), new Vector3(0.2f, 0.1f, 0.36f), whiteMaterial);
    }

    private void CreateLimb(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material)
    {
        GameObject limb = CreatePrimitiveChild(parent, name, PrimitiveType.Capsule, localPosition, localScale, material);
        limb.transform.localRotation = localRotation;
    }

    private void UpdateWheelScale(bool immediate)
    {
        if (immediate)
        {
            currentRadius = targetRadius;
        }

        if (wheel != null)
        {
            wheel.localPosition = new Vector3(0f, currentRadius, 0f);
            wheel.localScale = new Vector3(currentRadius * 2f, 0.34f, currentRadius * 2f);
        }

        if (characterRoot != null)
        {
            characterRoot.localPosition = new Vector3(0f, currentRadius * 2f + 0.04f, -0.06f);
        }
    }

    private GameObject CreatePrimitiveChild(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject child = GameObject.CreatePrimitive(primitiveType);
        Register(child);
        child.name = name;
        child.transform.SetParent(parent, false);
        child.transform.localPosition = localPosition;
        child.transform.localScale = localScale;
        SetMaterial(child, material);
        return child;
    }
}
