using UnityEngine;
using System.Collections;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System;

public class PolarGridRaycastSensor : SensorComponent
{
    [System.Serializable]
    private struct GizmoSettings
    {
        public bool showGizmos;
        public Color hitColor;
        public Color missColor;
    }

    [SerializeField]
    private string sensorName;

    [SerializeField]
    private List<string> detectableTags;

    [SerializeField]
    private MultiRaySensor.RaySettings raySettings;

    [Header("Distribution")]

    [SerializeField]
    private int numRows = 8;

    [SerializeField]
    [Range(0f, 360)]
    private float sliceWidthDegrees = 150;

    [SerializeField]
    private float spread = 1f;

    [SerializeField]
    private float radius = 10f;

    [SerializeField]
    private GizmoSettings gizmoSettings;

    [ReadOnly]
    [SerializeField]
    private int rayCount;

    private List<Ray> computedRays;

    public override ISensor[] CreateSensors()
    {
        RecalculateRays();
        var sensor = new MultiRaySensor(sensorName, transform, raySettings, computedRays, detectableTags);
        return new ISensor[] { sensor };
    }

    private void RecalculateRays()
    {
        if (computedRays == null)
            computedRays = new List<Ray>();
        else
            computedRays.Clear();

        if (numRows <= 0)
            return;

        float minAngle = sliceWidthDegrees * 0.5f;

        int row = 0;
        float currentAngle = minAngle;

        int iterations = 0;

        while (row < numRows)
        {
            if (iterations++ > 1000)
                return;

            float distanceFromCenter = ((float)row / numRows) * radius;

            Vector3 originLookAtDirection = transform.rotation * Quaternion.Euler(04, currentAngle, 0f) * transform.forward;

            Vector3 origin = originLookAtDirection * distanceFromCenter;
            Vector3 direction = transform.up * -1f;

            computedRays.Add(new Ray(origin, direction));

            currentAngle -= spread * 1.61803398875f;
            if (currentAngle < -minAngle)
            {
                var tooFar = (-minAngle) - currentAngle;
                currentAngle = minAngle - tooFar;
                row += 1;
            }
        }

        rayCount = computedRays.Count;
    }

    private void OnValidate()
    {
        RecalculateRays();
    }

    private void OnDrawGizmos()
    {
        if (!gizmoSettings.showGizmos)
            return;

        if (computedRays == null)
            RecalculateRays();

        foreach (var ray in computedRays)
        {
            var rayOutput = MultiRaySensor.RecordPerception(ray, raySettings, transform, null);
            var startPositionWorld = rayOutput.StartPositionWorld;
            var endPositionWorld = rayOutput.EndPositionWorld;
            var rayDirection = endPositionWorld - startPositionWorld;
            rayDirection *= rayOutput.HitFraction;

            // hit fraction ^2 will shift "far" hits closer to the hit color
            var lerpT = rayOutput.HitFraction * rayOutput.HitFraction;
            var color = Color.Lerp(gizmoSettings.hitColor, gizmoSettings.missColor, lerpT);
            Gizmos.color = color;
            Gizmos.DrawRay(startPositionWorld, rayDirection);

            // Draw the hit point as a sphere. If using rays to cast (0 radius), use a small sphere.
            if (rayOutput.HasHit)
            {
                var hitRadius = Mathf.Max(rayOutput.ScaledCastRadius, 0.2f);
                Gizmos.DrawWireSphere(startPositionWorld + rayDirection, hitRadius);
            }
        }
    }
}

