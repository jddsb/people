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
