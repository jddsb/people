using UnityEngine;

public partial class WheelRunnerBootstrap
{
    private const float TrackWidth = 7.2f;
    private const float TrackLoopStartZ = 2f;
    private const float TrackLoopLength = 718f;
    private const int LoopSegmentCount = 4;
    private const float LoopSegmentRecycleMargin = 80f;
    private const float TrackContentZScale = 2f;
    private const float TrackStripeSpacing = 9f;
    private const float PadHitExtraHalfWidth = 0.38f;
    private const float SpikeHitExtraHalfWidth = 0.42f;
    private const float BaffleSpacing = 150f;

    private void BuildLoopSegments()
    {
        for (int i = 0; i < LoopSegmentCount; i++)
        {
            float segmentStart = TrackLoopStartZ + i * TrackLoopLength;
            loopSegmentStarts[i] = segmentStart;

            GameObject segmentRoot = new GameObject("Loop Segment " + i);
            Register(segmentRoot);
            segmentRoot.transform.position = new Vector3(0f, 0f, segmentStart);
            loopSegmentRoots[i] = segmentRoot.transform;

            BuildTrackSegmentVisuals(segmentRoot.transform);
            BuildColorPadsForSegment(segmentRoot.transform, i == 0);
            BuildSpikeTrapsForSegment(segmentRoot.transform, i == 0);
            BuildColorBafflesForSegment(segmentRoot.transform, i == 0);
        }
    }

    private void BuildTrackSegmentVisuals(Transform parent)
    {
        GameObject track = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Register(track);
        track.name = "Loop Pastel Track";
        track.transform.SetParent(parent, false);
        track.transform.localPosition = new Vector3(0f, -0.05f, TrackLoopLength * 0.5f);
        track.transform.localScale = new Vector3(TrackWidth, 0.1f, TrackLoopLength);
        SetMaterial(track, trackMaterial);

        int stripeCount = Mathf.FloorToInt((TrackLoopLength - 4f) / TrackStripeSpacing);
        for (int i = 0; i < stripeCount; i++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Register(stripe);
            stripe.name = "Track Soft Stripe";
            stripe.transform.SetParent(parent, false);
            stripe.transform.localPosition = new Vector3(0f, 0.01f, 3f + i * TrackStripeSpacing);
            stripe.transform.localScale = new Vector3(TrackWidth, 0.025f, 4.4f);
            SetMaterial(stripe, i % 2 == 0 ? trackMaterial : CreateMaterial("Runtime Track Stripe", new Color(0.91f, 0.84f, 0.96f)));
        }

        CreateWallChild(parent, "Left Purple Wall", new Vector3(-TrackWidth * 0.5f - 0.45f, 1.4f, TrackLoopLength * 0.5f), new Vector3(0.55f, 2.8f, TrackLoopLength));
        CreateWallChild(parent, "Right Purple Wall", new Vector3(TrackWidth * 0.5f + 0.45f, 1.4f, TrackLoopLength * 0.5f), new Vector3(0.55f, 2.8f, TrackLoopLength));
    }

    private void CreateWallChild(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Register(wall);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.localPosition = localPosition;
        wall.transform.localScale = localScale;
        SetMaterial(wall, wallMaterial);
    }

    private void BuildColorPadsForSegment(Transform parent, bool registerGameplay)
    {
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 1", WheelRunnerColor.Green, -1.45f, 17f, 5.7f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 1", WheelRunnerColor.Blue, 1.45f, 17f, 5.7f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 1", WheelRunnerColor.Yellow, 0.35f, 31f, 7f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 2", WheelRunnerColor.Green, -1.35f, 43f, 6.4f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 2", WheelRunnerColor.Blue, 1.35f, 43f, 6.4f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 3", WheelRunnerColor.Green, 0.25f, 58f, 8.5f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 2", WheelRunnerColor.Yellow, -1.55f, 73f, 6.2f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 3", WheelRunnerColor.Blue, 1.45f, 86f, 7.4f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 4", WheelRunnerColor.Green, -0.7f, 100f, 8.2f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 3", WheelRunnerColor.Yellow, 1.25f, 116f, 7f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 5", WheelRunnerColor.Green, 0f, 130f, 6.8f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 4", WheelRunnerColor.Blue, 1.4f, 148f, 6.5f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 4", WheelRunnerColor.Yellow, -0.9f, 163f, 7f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 6", WheelRunnerColor.Green, -1.2f, 178f, 6.8f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 5", WheelRunnerColor.Blue, 1.35f, 193f, 6.4f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 5", WheelRunnerColor.Yellow, 0.5f, 208f, 7.2f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 7", WheelRunnerColor.Green, -0.6f, 223f, 6.6f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 6", WheelRunnerColor.Blue, 1.5f, 238f, 6.2f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 6", WheelRunnerColor.Yellow, -0.4f, 253f, 6.8f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 8", WheelRunnerColor.Green, -1.3f, 268f, 6.5f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 7", WheelRunnerColor.Blue, 1.25f, 283f, 6.6f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 7", WheelRunnerColor.Yellow, 0.8f, 298f, 7f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 9", WheelRunnerColor.Green, -0.5f, 313f, 6.7f);
        AddScaledPad(parent, registerGameplay, "Blue Shrink Pad 8", WheelRunnerColor.Blue, 1.45f, 328f, 6.4f);
        AddScaledPad(parent, registerGameplay, "Yellow Shrink Pad 8", WheelRunnerColor.Yellow, -1.1f, 343f, 6.9f);
        AddScaledPad(parent, registerGameplay, "Green Growth Pad 10", WheelRunnerColor.Green, 0.35f, 358f, 6.3f);
    }

    private void AddScaledPad(Transform parent, bool registerGameplay, string name, WheelRunnerColor padColor, float x, float z, float length)
    {
        AddPad(parent, registerGameplay, name, padColor, x, z * TrackContentZScale, length);
    }

    private void AddPad(Transform parent, bool registerGameplay, string name, WheelRunnerColor padColor, float x, float z, float length)
    {
        float localCenterZ = z - TrackLoopStartZ;
        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Register(pad);
        pad.name = name;
        pad.transform.SetParent(parent, false);
        pad.transform.localPosition = new Vector3(x, 0.06f, localCenterZ);
        pad.transform.localScale = new Vector3(1.15f, 0.12f, length);
        SetMaterial(pad, GetMaterial(padColor));

        if (registerGameplay)
        {
            colorPads.Add(new WheelRunnerColorPad(padColor, x, z - length * 0.5f, z + length * 0.5f, 0.78f, pad));
        }
    }

    private void BuildColorBafflesForSegment(Transform parent, bool registerGameplay)
    {
        int startOffset = ((int)initialWheelColor + 1) % 3;
        int colorIndex = 0;
        for (float z = BaffleSpacing; z < TrackLoopStartZ + TrackLoopLength; z += BaffleSpacing)
        {
            WheelRunnerColor baffleColor = (WheelRunnerColor)((startOffset + colorIndex) % 3);
            AddBaffle(parent, registerGameplay, baffleColor + " Color Baffle", baffleColor, z);
            colorIndex++;
        }
    }

    private void AddBaffle(Transform parent, bool registerGameplay, string name, WheelRunnerColor baffleColor, float z)
    {
        float localCenterZ = z - TrackLoopStartZ;
        Material opaqueMaterial = GetMaterial(baffleColor);
        Material gateMaterial = CreateTransparentMaterial("Runtime Baffle " + baffleColor, GetColor(baffleColor));

        GameObject gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Register(gate);
        gate.name = name;
        gate.transform.SetParent(parent, false);
        gate.transform.localPosition = new Vector3(0f, 1.85f, localCenterZ);
        gate.transform.localScale = new Vector3(TrackWidth - 0.35f, 3.7f, 0.14f);
        SetMaterial(gate, gateMaterial);
        RemoveCollider(gate);

        GameObject baseStrip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Register(baseStrip);
        baseStrip.name = name + " Base";
        baseStrip.transform.SetParent(parent, false);
        baseStrip.transform.localPosition = new Vector3(0f, 0.12f, localCenterZ);
        baseStrip.transform.localScale = new Vector3(TrackWidth - 0.15f, 0.2f, 0.38f);
        SetMaterial(baseStrip, opaqueMaterial);
        RemoveCollider(baseStrip);

        if (registerGameplay)
        {
            colorBaffles.Add(new WheelRunnerColorBaffle(baffleColor, z, 0.42f, gate));
        }
    }

    private void BuildSpikeTrapsForSegment(Transform parent, bool registerGameplay)
    {
        AddScaledSpikeTrap(parent, registerGameplay, "Center Spike Trap 1", 0f, 36f, 3.05f, 8.2f);
        AddScaledSpikeTrap(parent, registerGameplay, "Left Spike Trap 1", -1.35f, 137f, 2.2f, 7f);
        AddScaledSpikeTrap(parent, registerGameplay, "Right Spike Trap 1", 1.25f, 221f, 2.3f, 7.4f);
        AddScaledSpikeTrap(parent, registerGameplay, "Center Spike Trap 2", 0.15f, 302f, 3f, 8f);
    }

    private void AddScaledSpikeTrap(Transform parent, bool registerGameplay, string name, float x, float z, float width, float length)
    {
        AddSpikeTrap(parent, registerGameplay, name, x, z * TrackContentZScale, width, length);
    }

    private void AddSpikeTrap(Transform parent, bool registerGameplay, string name, float x, float z, float width, float length)
    {
        float localCenterZ = z - TrackLoopStartZ;
        GameObject trapRoot = new GameObject(name);
        Register(trapRoot);
        trapRoot.transform.SetParent(parent, false);
        trapRoot.transform.localPosition = new Vector3(x, 0f, localCenterZ);

        GameObject basePlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Register(basePlate);
        basePlate.name = name + " Base";
        basePlate.transform.SetParent(trapRoot.transform, false);
        basePlate.transform.localPosition = new Vector3(0f, 0.035f, 0f);
        basePlate.transform.localScale = new Vector3(width, 0.07f, length);
        SetMaterial(basePlate, CreateMaterial("Runtime Spike Base", new Color(0.11f, 0.11f, 0.12f)));
        RemoveCollider(basePlate);

        const int columns = 5;
        const int rows = 6;
        float xSpacing = width / (columns + 0.6f);
        float zSpacing = length / (rows + 0.7f);
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Register(spike);
                spike.name = name + " Spike";
                spike.transform.SetParent(trapRoot.transform, false);
                float spikeX = (column - (columns - 1) * 0.5f) * xSpacing;
                float spikeZ = (row - (rows - 1) * 0.5f) * zSpacing;
                spike.transform.localPosition = new Vector3(spikeX, 0.36f, spikeZ);
                spike.transform.localRotation = Quaternion.identity;
                spike.transform.localScale = new Vector3(0.24f, 0.68f, 0.24f);
                SetMaterial(spike, spikeMaterial);
                MakeSpikeMesh(spike);
                RemoveCollider(spike);
            }
        }

        if (registerGameplay)
        {
            spikeTraps.Add(new WheelRunnerSpikeTrap(x, z - length * 0.5f, z + length * 0.5f, width * 0.5f + SpikeHitExtraHalfWidth, trapRoot));
        }
    }

    private static void MakeSpikeMesh(GameObject spike)
    {
        MeshFilter meshFilter = spike.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            return;
        }

        const int sides = 12;
        Vector3[] vertices = new Vector3[sides + 2];
        int[] triangles = new int[sides * 6];
        vertices[0] = new Vector3(0f, 0.5f, 0f);
        vertices[1] = new Vector3(0f, -0.5f, 0f);

        for (int i = 0; i < sides; i++)
        {
            float angle = Mathf.PI * 2f * i / sides;
            vertices[i + 2] = new Vector3(Mathf.Cos(angle) * 0.5f, -0.5f, Mathf.Sin(angle) * 0.5f);
        }

        int triangleIndex = 0;
        for (int i = 0; i < sides; i++)
        {
            int current = i + 2;
            int next = i == sides - 1 ? 2 : i + 3;
            triangles[triangleIndex++] = 0;
            triangles[triangleIndex++] = next;
            triangles[triangleIndex++] = current;
            triangles[triangleIndex++] = 1;
            triangles[triangleIndex++] = current;
            triangles[triangleIndex++] = next;
        }

        Mesh mesh = new Mesh();
        mesh.name = "Runtime Spike Cone";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
    }

    private float GetFrontSegmentStart()
    {
        float front = loopSegmentStarts[0];
        for (int i = 1; i < LoopSegmentCount; i++)
        {
            if (loopSegmentStarts[i] > front)
            {
                front = loopSegmentStarts[i];
            }
        }

        return front;
    }

    private void UpdateLoopSegments()
    {
        float recycleZ = zPosition - TrackLoopLength - LoopSegmentRecycleMargin;
        for (int i = 0; i < LoopSegmentCount; i++)
        {
            if (loopSegmentStarts[i] + TrackLoopLength >= recycleZ || loopSegmentRoots[i] == null)
            {
                continue;
            }

            float newStart = GetFrontSegmentStart() + TrackLoopLength;
            loopSegmentStarts[i] = newStart;
            loopSegmentRoots[i].position = new Vector3(0f, 0f, newStart);
        }
    }
}
