using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class JumpSensorComponent : MonoBehaviour
{
    [System.Serializable]
    public struct ArcPath
    {
        public int numTestSteps;
        public float stepLength;
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
    private ArcPath jumpPath;

    [SerializeField]
    private ArcPath dropPath;

    [SerializeField]
    private List<float> trajectoryAngles;

    private Observation observation;
    public Observation CurrentObservation => observation;

    public void RecordObservations()
    {
        observation = default;

        // Drop path
        foreach (var angle in trajectoryAngles)
        {
            if (TryFindUniqueCollisionAlongPath(GetDropSteps(angle), out var hit))
            {
                observation.availableDropLanding = new Landing
                {
                    collider = hit.collider,
                    surfaceNormal = hit.normal,
                    landingPosition = hit.point,
                    relativeRotationOffset = angle,
                };

                break;
            }
        }

        // Jump path
        foreach (var angle in trajectoryAngles)
        {
            if (TryFindUniqueCollisionAlongPath(GetJumpSteps(angle), out var hit))
            {
                observation.availableJumpLanding = new Landing
                {
                    collider = hit.collider,
                    surfaceNormal = hit.normal,
                    landingPosition = hit.point,
                    relativeRotationOffset = angle,
                };

                break;
            }
        }
    }

    private IEnumerable<(Vector3 p1, Vector3 p2)> GetDropSteps(float angle)
    {
        Vector3 previousPoint = transform.position;
        Vector3 velocity = transform.rotation * Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, movementController.movementSpeed);
        for (int i = 0; i < dropPath.numTestSteps + 1; i++)
        {
            Vector3 nextPoint = previousPoint + velocity * dropPath.stepLength;

            yield return (previousPoint, nextPoint);

            velocity.y -= movementController.gravity * dropPath.stepLength;
            previousPoint = nextPoint;
        }
    }

    private IEnumerable<(Vector3 p1, Vector3 p2)> GetJumpSteps(float angle)
    {
        Vector3 previousPoint = transform.position;
        Vector3 velocity = transform.rotation * Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, movementController.jumpSpeed, movementController.movementSpeed);
        for (int i = 0; i < jumpPath.numTestSteps + 1; i++)
        {
            Vector3 nextPoint = previousPoint + velocity * jumpPath.stepLength;

            yield return (previousPoint, nextPoint);

            velocity.y -= movementController.gravity * jumpPath.stepLength;
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

        foreach (var angle in trajectoryAngles)
        {
            // Drop path
            foreach (var segment in GetDropSteps(angle))
                Gizmos.DrawLine(segment.p1, segment.p2);

            // Jump path
            foreach (var segment in GetJumpSteps(angle))
                Gizmos.DrawLine(segment.p1, segment.p2);
        }
    }
}
