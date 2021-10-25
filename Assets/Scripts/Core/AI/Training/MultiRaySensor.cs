using System;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MultiRaySensor : ISensor
{
    [System.Serializable]
    public struct RaySettings
    {
        public LayerMask layerMask;
        public float maxRayDistance;
    };

    public string Name { get; }
    public IEnumerable<Ray> Rays => rays;

    private float[] observations;
    private RayPerceptionOutput sensorOutput;
    private ObservationSpec observationSpec;

    private IReadOnlyList<Ray> rays;
    private IReadOnlyList<string> tags;

    private Transform localTransform;

    private RaySettings settings;

    public MultiRaySensor(string name, Transform localTransform, RaySettings settings, IEnumerable<Ray> rays, IEnumerable<string> tags)
    {
        this.Name = name;
        this.localTransform = localTransform;
        this.settings = settings;
        this.rays = new List<Ray>(rays);
        this.tags = new List<string>(tags);

        this.sensorOutput = new RayPerceptionOutput();

        var numObservations = (this.tags.Count + 2) * this.rays.Count;
        this.observations = new float[numObservations];
        this.observationSpec = ObservationSpec.Vector(numObservations);
        this.sensorOutput.RayOutputs = new RayPerceptionOutput.RayOutput[this.rays.Count];
    }

    public byte[] GetCompressedObservation()
    {
        return null;
    }

    public CompressionSpec GetCompressionSpec()
    {
        return CompressionSpec.Default();
    }

    public string GetName()
    {
        return Name;
    }

    public ObservationSpec GetObservationSpec()
    {
        return observationSpec;
    }

    public void Reset()
    {
    }

    public void Update()
    {
        for (var rayIndex = 0; rayIndex < rays.Count; rayIndex++)
        {
            sensorOutput.RayOutputs[rayIndex] = RecordPerception(rays[rayIndex], settings, localTransform, tags);
        }
    }

    public int Write(ObservationWriter writer)
    {
        Array.Clear(observations, 0, observations.Length);
        var numRays = rays.Count;
        var numDetectableTags = tags.Count;

        // For each ray, write the information to the observation buffer
        for (var rayIndex = 0; rayIndex < numRays; rayIndex++)
        {
            sensorOutput.RayOutputs?[rayIndex].ToFloatArray(numDetectableTags, rayIndex, observations);
        }

        // Finally, add the observations to the ObservationWriter
        writer.AddList(observations);

        return observations.Length;
    }

    public static RayPerceptionOutput.RayOutput RecordPerception(Ray ray, RaySettings settings, Transform transform, IReadOnlyList<string> tags)
    {
        Vector3 origin = ray.origin;
        Vector3 direction = ray.direction;
        if (transform != null)
        {
            origin = transform.TransformPoint(origin);
            direction = transform.TransformDirection(direction);
        }

        bool castHit = Physics.Raycast(origin, direction, out var rayHit, settings.maxRayDistance, settings.layerMask, QueryTriggerInteraction.Ignore);

        var hitFraction = castHit ? rayHit.distance / settings.maxRayDistance : 1.0f;
        var hitObject = castHit ? rayHit.collider.gameObject : null;

        var rayOutput = new RayPerceptionOutput.RayOutput
        {
            HasHit = castHit,
            HitFraction = hitFraction,
            HitTaggedObject = false,
            HitTagIndex = -1,
            HitGameObject = hitObject,
            StartPositionWorld = origin,
            EndPositionWorld = origin + direction * settings.maxRayDistance,
            ScaledCastRadius = 0f,
        };

        if (castHit)
        {
            // Find the index of the tag of the object that was hit.
            var numTags = tags == null ? 0 : tags.Count;
            for (var i = 0; i < numTags; i++)
            {
                var tagsEqual = false;
                try
                {
                    var tag = tags[i];
                    if (!string.IsNullOrEmpty(tag))
                    {
                        tagsEqual = hitObject.CompareTag(tag);
                    }
                }
                catch (UnityException)
                {
                    // If the tag is null, empty, or not a valid tag, just ignore it.
                }

                if (tagsEqual)
                {
                    rayOutput.HitTaggedObject = true;
                    rayOutput.HitTagIndex = i;
                    break;
                }
            }
        }

        return rayOutput;
    }
}
