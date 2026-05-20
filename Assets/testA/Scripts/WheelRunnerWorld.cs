using UnityEngine;

public partial class WheelRunnerBootstrap
{
    private void GetCameraRig(out Vector3 targetPosition, out Vector3 lookTarget)
    {
        float wheelRadius = Mathf.Max(currentRadius, targetRadius);
        float characterTop = wheelRadius * 2f + 2.1f;
        float backDistance = 9.5f + wheelRadius * 0.62f;
        float cameraHeight = characterTop + 5.2f + wheelRadius * 0.42f;
        float lookAheadZ = 14f + wheelRadius * 1.35f;
        float lookHeight = 1.05f;

        targetPosition = runnerRoot.position + new Vector3(0f, cameraHeight, -backDistance);
        lookTarget = runnerRoot.position + new Vector3(0f, lookHeight, lookAheadZ);
    }

    private void BuildWorld()
    {
        ClearSpawnedObjects();
        BuildLightingAndCamera();
        BuildLoopSegments();
        BuildRunner();
        BuildUi();
    }

    private void ClearSpawnedObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }

        spawnedObjects.Clear();
        colorPads.Clear();
        colorBaffles.Clear();
    }

    private void BuildLightingAndCamera()
    {
        Camera[] existingCameras = FindObjectsOfType<Camera>();
        for (int i = 0; i < existingCameras.Length; i++)
        {
            Destroy(existingCameras[i].gameObject);
        }

        Light[] existingLights = FindObjectsOfType<Light>();
        for (int i = 0; i < existingLights.Length; i++)
        {
            Destroy(existingLights[i].gameObject);
        }

        GameObject lightObject = new GameObject("Sun Light");
        Register(lightObject);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.shadows = LightShadows.Soft;
        lightObject.transform.rotation = Quaternion.Euler(48f, -35f, 12f);

        GameObject cameraObject = new GameObject("Main Camera");
        Register(cameraObject);
        mainCamera = cameraObject.AddComponent<Camera>();
        mainCamera.tag = "MainCamera";
        mainCamera.fieldOfView = 58f;
        mainCamera.backgroundColor = new Color(0.18f, 0.14f, 0.28f);
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.transform.position = new Vector3(0f, 5.2f, -8.2f);
        cameraObject.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
    }

    private void Register(GameObject gameObject)
    {
        spawnedObjects.Add(gameObject);
    }
}
