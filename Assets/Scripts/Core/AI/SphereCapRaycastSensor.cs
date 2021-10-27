using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;

public class SphereCapRaycastSensor : SensorComponent
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
    private Quaternion direction = Quaternion.identity;

    [SerializeField]
    private int density = 25;

    [SerializeField]
    [Range(0f, 2f)]
    private float arcSize = 0.25f;

    [SerializeField]
    private List<string> detectableTags;

    [SerializeField]
    private MultiRaySensor.RaySettings raySettings;

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

        int nPoints = Mathf.Max(1, density);

        Vector3 forward = direction * transform.forward;

        float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
        float off = 2.0f / nPoints;
        float x = 0;
        float y = 0;
        float z = 0;
        float r = 0;
        float phi = 0;

        for (var k = 0; k < nPoints; k++)
        {
            y = k * off - 1 + (off / 2);
            r = Mathf.Sqrt(1 - y * y);
            phi = k * inc;
            x = Mathf.Cos(phi) * r;
            z = Mathf.Sin(phi) * r;

            Vector3 surfacePosition = new Vector3(x, y, z);
            if (Vector3.Distance(surfacePosition, forward) <= arcSize + 0.001f)
                computedRays.Add(new Ray(Vector3.zero, surfacePosition));
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
            var lerpT = rayOutput.HitFraction;
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
