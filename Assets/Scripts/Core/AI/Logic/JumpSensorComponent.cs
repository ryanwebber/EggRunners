using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class JumpSensorComponent : MonoBehaviour
{
    [System.Serializable]
    public class Sensor
    {
        [SerializeField, Min(1f)]
        public int numSteps = 5;

        [SerializeField, Min(0f)]
        public float stepSize = 2;

        [SerializeField]
        public float angleOffset = 0;
    }

    [System.Serializable]
    public struct Landing
    {
        public Collider collider;
        public Vector3 surfaceNormal;
        public Vector3 landingPosition;
        public float relativeRotationOffset;
    }

    [System.Serializable]
    public struct Observation
    {
        public Nullable<Landing> availableJumpLanding;
        public Nullable<Landing> availableDropLanding;
    }

    [SerializeField]
    private LayerMask groundLayerMask;

    [SerializeField]
    private CMFMovementController movementController;

    [SerializeField]
    private List<Sensor> jumpSensors;

    [SerializeField]
    private List<Sensor> dropSensors;

    private Observation observation;
    public Observation CurrentObservation => observation;

    public void RecordObservations()
    {
        observation = default;

        // Drop path
        foreach (var sensor in dropSensors)
        {
            if (TryFindUniqueCollisionAlongPath(GetDropSteps(sensor), out var hit))
            {
                observation.availableDropLanding = new Landing
                {
                    collider = hit.collider,
                    surfaceNormal = hit.normal,
                    landingPosition = hit.point,
                    relativeRotationOffset = sensor.angleOffset,
                };

                break;
            }
        }

        // Jump path
        foreach (var sensor in jumpSensors)
        {
            if (TryFindUniqueCollisionAlongPath(GetJumpSteps(sensor), out var hit))
            {
                observation.availableJumpLanding = new Landing
                {
                    collider = hit.collider,
                    surfaceNormal = hit.normal,
                    landingPosition = hit.point,
                    relativeRotationOffset = sensor.angleOffset,
                };

                break;
            }
        }
    }

    private IEnumerable<(Vector3 p1, Vector3 p2)> GetDropSteps(Sensor sensor)
    {
        Vector3 previousPoint = transform.position;
        Vector3 velocity = transform.rotation * Quaternion.Euler(0f, sensor.angleOffset, 0f) * new Vector3(0f, 0f, movementController.movementSpeed);
        for (int i = 0; i < sensor.numSteps+ 1; i++)
        {
            Vector3 nextPoint = previousPoint + velocity * sensor.stepSize;

            yield return (previousPoint, nextPoint);

            velocity.y -= movementController.gravity * sensor.stepSize;
            previousPoint = nextPoint;
        }
    }

    private IEnumerable<(Vector3 p1, Vector3 p2)> GetJumpSteps(Sensor sensor)
    {
        Vector3 previousPoint = transform.position;
        Vector3 velocity = transform.rotation * Quaternion.Euler(0f, sensor.angleOffset, 0f) * new Vector3(0f, movementController.jumpSpeed, movementController.movementSpeed);
        for (int i = 0; i < sensor.numSteps + 1; i++)
        {
            Vector3 nextPoint = previousPoint + velocity * sensor.stepSize;

            yield return (previousPoint, nextPoint);

            velocity.y -= movementController.gravity * sensor.stepSize;
            previousPoint = nextPoint;
        }
    }

    private bool TryFindUniqueCollisionAlongPath(IEnumerable<(Vector3 p1, Vector3 p2)> path, out RaycastHit hit)
    {
        foreach (var segment in path)
        {
            Vector3 diff = segment.p2 - segment.p1;
            Ray ray = new Ray(segment.p1, segment.p2 - segment.p1);
            if (Physics.Raycast(ray, out hit, diff.magnitude, groundLayerMask, QueryTriggerInteraction.Ignore))
            {
                // Only testing things we can land on when heading downwards. If we hit something, we make
                // an assumption we can't compete the jump/drop
                if (Vector3.Dot(-transform.up, (segment.p2 - segment.p1)) <= 0f)
                    return false;

                // Check if the thing we've hit on our way down is not what we're standing on
                if (hit.collider == null || hit.collider == movementController.Mover.GetGroundColliderOrDefault())
                    return false;

                // Check to make sure the surface normal is up-ish
                if (Vector3.Dot(hit.normal, transform.up) < 0.6f)
                    return false;


                return true;
            }
        }

        hit = default;
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (var sensor in dropSensors)
            foreach (var segment in GetDropSteps(sensor))
                Gizmos.DrawLine(segment.p1, segment.p2);

        foreach (var sensor in jumpSensors)
            foreach (var segment in GetJumpSteps(sensor))
                Gizmos.DrawLine(segment.p1, segment.p2);
    }
}
