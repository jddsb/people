using UnityEngine;

public partial class WheelRunnerBootstrap
{
    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal") * horizontalSpeed * Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastPointerX = Input.mousePosition.x;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            float delta = Input.mousePosition.x - lastPointerX;
            horizontal += delta * dragSensitivity;
            lastPointerX = Input.mousePosition.x;
        }

        xPosition = Mathf.Clamp(xPosition + horizontal, -TrackWidth * 0.5f + 0.72f, TrackWidth * 0.5f - 0.72f);
    }

    private float GetCurrentForwardSpeed()
    {
        float ramp = 1f - Mathf.Exp(-totalDistanceTraveled / Mathf.Max(speedRampDistance, 1f));
        float lapBoost = lapCount * 0.05f;
        float targetSpeed = Mathf.Lerp(forwardSpeed, maxForwardSpeed, Mathf.Clamp01(ramp + lapBoost));
        currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, targetSpeed, postSlowdownAcceleration * Time.deltaTime);
        return currentForwardSpeed;
    }

    private void MoveRunner()
    {
        float speed = GetCurrentForwardSpeed();
        float delta = speed * Time.deltaTime;
        zPosition += delta;
        totalDistanceTraveled += delta;
        UpdateLapProgress();
        UpdateLoopSegments();
        currentRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * 10f);
        runnerRoot.position = new Vector3(xPosition, 0f, zPosition);
        UpdateWheelScale(false);

        wheelRollAngle += delta / Mathf.Max(currentRadius, 0.05f) * Mathf.Rad2Deg;
        wheel.localRotation = Quaternion.Euler(wheelRollAngle, 0f, 90f);

        if (characterRoot != null)
        {
            float bob = Mathf.Sin(Time.time * 11f) * 0.035f;
            characterRoot.localPosition = new Vector3(0f, currentRadius * 2f + 0.04f + bob, -0.06f);
        }
    }

    private void UpdateFallingBalls()
    {
        if (Time.time >= nextFallingBallSpawnTime)
        {
            SpawnFallingBall();
            nextFallingBallSpawnTime = Time.time + Mathf.Max(0.65f, fallingBallSpawnInterval);
        }

        float groundY = fallingBallRadius;
        for (int i = fallingBalls.Count - 1; i >= 0; i--)
        {
            WheelRunnerFallingBall ball = fallingBalls[i];
            if (ball.Consumed)
            {
                RemoveFallingBallAt(i);
                continue;
            }

            if (!ball.HasLanded)
            {
                ball.Y = Mathf.MoveTowards(ball.Y, groundY, fallingBallFallSpeed * Time.deltaTime);
                ball.HasLanded = Mathf.Approximately(ball.Y, groundY);
            }
            else
            {
                float rollDelta = fallingBallRollSpeed * Time.deltaTime;
                ball.Z -= rollDelta;
                ball.RollAngle += rollDelta / Mathf.Max(fallingBallRadius, 0.05f) * Mathf.Rad2Deg;
            }

            if (ball.Visual != null)
            {
                ball.Visual.transform.position = new Vector3(ball.X, ball.Y, ball.Z);
                ball.Visual.transform.rotation = Quaternion.Euler(ball.RollAngle, 0f, 0f);
            }

            if (ball.Z < zPosition - 28f)
            {
                RemoveFallingBallAt(i);
                continue;
            }

            fallingBalls[i] = ball;
        }
    }

    private void SpawnFallingBall()
    {
        WheelRunnerColor ballColor = GetDifferentBallColor();
        float[] lanes = { -2.2f, 0f, 2.2f };
        float x = lanes[Random.Range(0, lanes.Length)];
        float z = zPosition + fallingBallSpawnDistance + Random.Range(-7f, 13f);
        float y = 8.5f + Random.Range(0f, 3.5f);

        GameObject ballObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Register(ballObject);
        ballObject.name = ballColor + " Falling Ball";
        ballObject.transform.localScale = Vector3.one * (fallingBallRadius * 2f);
        SetMaterial(ballObject, GetMaterial(ballColor));
        RemoveCollider(ballObject);

        fallingBalls.Add(new WheelRunnerFallingBall(ballColor, x, z, y, ballObject));
    }

    private WheelRunnerColor GetDifferentBallColor()
    {
        int offset = Random.Range(1, 3);
        return (WheelRunnerColor)(((int)wheelColor + offset) % 3);
    }

    private void RemoveFallingBallAt(int index)
    {
        if (index < 0 || index >= fallingBalls.Count)
        {
            return;
        }

        GameObject visual = fallingBalls[index].Visual;
        if (visual != null)
        {
            spawnedObjects.Remove(visual);
            Destroy(visual);
        }

        fallingBalls.RemoveAt(index);
    }

    private float GetLoopZ(float worldZ)
    {
        return TrackLoopStartZ + Mathf.Repeat(worldZ - TrackLoopStartZ, TrackLoopLength);
    }

    private void UpdateLapProgress()
    {
        int newLap = Mathf.FloorToInt((zPosition - TrackLoopStartZ) / TrackLoopLength);
        if (newLap <= lapCount)
        {
            return;
        }

        lapCount = newLap;
        ResetLoopInteractables();
        ShowLapMessage();
    }

    private void ResetLoopInteractables()
    {
        for (int i = 0; i < colorPads.Count; i++)
        {
            WheelRunnerColorPad pad = colorPads[i];
            pad.Consumed = false;
            colorPads[i] = pad;
            if (pad.Visual != null)
            {
                pad.Visual.transform.localScale = new Vector3(pad.Visual.transform.localScale.x, 0.12f, pad.Visual.transform.localScale.z);
            }
        }

        for (int i = 0; i < colorBaffles.Count; i++)
        {
            WheelRunnerColorBaffle baffle = colorBaffles[i];
            baffle.Consumed = false;
            colorBaffles[i] = baffle;
            if (baffle.Visual != null)
            {
                baffle.Visual.transform.localScale = new Vector3(TrackWidth - 0.35f, 3.7f, 0.14f);
            }
        }

        for (int i = 0; i < spikeTraps.Count; i++)
        {
            WheelRunnerSpikeTrap spikeTrap = spikeTraps[i];
            spikeTrap.Consumed = false;
            spikeTraps[i] = spikeTrap;
            if (spikeTrap.Visual != null)
            {
                spikeTrap.Visual.transform.localScale = Vector3.one;
            }
        }
    }

    private void CheckPads()
    {
        float loopZ = GetLoopZ(zPosition);
        for (int i = 0; i < colorPads.Count; i++)
        {
            WheelRunnerColorPad pad = colorPads[i];
            if (pad.Consumed || loopZ < pad.StartZ || loopZ > pad.EndZ || Mathf.Abs(xPosition - pad.X) > pad.HalfWidth + PadHitExtraHalfWidth)
            {
                continue;
            }

            bool isMatch = pad.Color == wheelColor;
            float delta = isMatch ? radiusStep : -radiusStep;
            float previousTargetRadius = targetRadius;
            float newTargetRadius = Mathf.Clamp(targetRadius + delta, minWheelRadius, maxWheelRadius);
            bool radiusChanged = !Mathf.Approximately(newTargetRadius, previousTargetRadius);

            pad.Consumed = true;
            colorPads[i] = pad;
            targetRadius = newTargetRadius;
            if (!isMatch)
            {
                float slowdownMultiplier = Mathf.Clamp(mismatchSlowdownMultiplier, 0.05f, 1f);
                currentForwardSpeed = Mathf.Max(0.1f, currentForwardSpeed * slowdownMultiplier);
            }
            score = Mathf.Max(0, score + (isMatch ? 1 : -1));
            PulsePad(pad.Visual, isMatch);
            ShowPadMessage(isMatch, pad.Color, radiusChanged);
        }
    }

    private void PulsePad(GameObject pad, bool isMatch)
    {
        if (pad == null)
        {
            return;
        }

        pad.transform.localScale = new Vector3(pad.transform.localScale.x, isMatch ? 0.22f : 0.05f, pad.transform.localScale.z);
    }

    private void CheckSpikeTraps()
    {
        float loopZ = GetLoopZ(zPosition);
        for (int i = 0; i < spikeTraps.Count; i++)
        {
            WheelRunnerSpikeTrap spikeTrap = spikeTraps[i];
            if (spikeTrap.Consumed || loopZ < spikeTrap.StartZ || loopZ > spikeTrap.EndZ || Mathf.Abs(xPosition - spikeTrap.X) > spikeTrap.HalfWidth)
            {
                continue;
            }

            spikeTrap.Consumed = true;
            spikeTraps[i] = spikeTrap;
            float radiusMultiplier = Mathf.Clamp(spikeRadiusMultiplier, 0.05f, 1f);
            float speedMultiplier = Mathf.Clamp(spikeSlowdownMultiplier, 0.05f, 1f);
            targetRadius = Mathf.Max(minWheelRadius, targetRadius * radiusMultiplier);
            currentRadius = Mathf.Min(currentRadius, targetRadius + radiusStep * 0.25f);
            currentForwardSpeed = Mathf.Max(0.1f, currentForwardSpeed * speedMultiplier);
            score = Mathf.Max(0, score - 3);
            PulseSpikeTrap(spikeTrap.Visual);
            ShowSpikeTrapMessage();
        }
    }

    private void PulseSpikeTrap(GameObject spikeTrap)
    {
        if (spikeTrap == null)
        {
            return;
        }

        spikeTrap.transform.localScale = new Vector3(1.08f, 0.36f, 1.08f);
    }

    private void CheckFallingBalls()
    {
        for (int i = 0; i < fallingBalls.Count; i++)
        {
            WheelRunnerFallingBall ball = fallingBalls[i];
            if (ball.Consumed || !ball.HasLanded)
            {
                continue;
            }

            float characterRadius = 0.35f;
            float hitDistance = characterRadius + fallingBallRadius * 0.25f;
            bool isCloseOnZ = Mathf.Abs(zPosition - ball.Z) <= hitDistance;
            bool isCloseOnX = Mathf.Abs(xPosition - ball.X) <= hitDistance;
            if (!isCloseOnZ || !isCloseOnX)
            {
                continue;
            }

            ball.Consumed = true;
            fallingBalls[i] = ball;

            if (ball.Color != wheelColor)
            {
                float radiusMultiplier = Mathf.Clamp(fallingBallRadiusMultiplier, 0.05f, 1f);
                float speedMultiplier = Mathf.Clamp(fallingBallSlowdownMultiplier, 0.05f, 1f);
                targetRadius = Mathf.Max(minWheelRadius, targetRadius * radiusMultiplier);
                currentRadius = Mathf.Min(currentRadius, targetRadius + radiusStep * 0.2f);
                currentForwardSpeed = Mathf.Max(0.1f, currentForwardSpeed * speedMultiplier);
                score = Mathf.Max(0, score - 4);
                ShowFallingBallMessage(ball.Color);
            }
        }
    }

    private void CheckBaffles()
    {
        for (int i = 0; i < colorBaffles.Count; i++)
        {
            WheelRunnerColorBaffle baffle = colorBaffles[i];
            if (baffle.Consumed || Mathf.Abs(GetLoopZ(zPosition) - baffle.Z) > baffle.HalfThickness)
            {
                continue;
            }

            baffle.Consumed = true;
            colorBaffles[i] = baffle;
            SetWheelColor(baffle.Color);
            ShowBaffleMessage(baffle.Color);
            score += 1;
            PulseBaffle(baffle.Visual);
        }
    }

    private void PulseBaffle(GameObject gate)
    {
        if (gate == null)
        {
            return;
        }

        gate.transform.localScale = new Vector3(gate.transform.localScale.x * 1.06f, gate.transform.localScale.y, gate.transform.localScale.z * 1.35f);
    }

    private void SetWheelColor(WheelRunnerColor color)
    {
        wheelColor = color;
        if (wheel != null)
        {
            Renderer renderer = wheel.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetMaterial(wheelColor);
            }
        }

        UpdateUi();
    }
}
