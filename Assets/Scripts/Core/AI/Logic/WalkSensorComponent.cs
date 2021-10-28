using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WalkSensorComponent : MonoBehaviour
{
    [System.Serializable]
    public struct Observation
    {
        public float relativeRotationOffset;
        public float approximateObservedWalkableDistance;
        public Vector3 averagedHorizonTargetOffset;
    }

    [SerializeField]
    private LayerMask groundLayerMask;

    [SerializeField]
    private CMFMovementController movementController;

    [SerializeField, Min(1f)]
    private int numSteps;

    [SerializeField, Min(0f)]
    private float stepSize;

    [SerializeField, Min(0f)]
    private float testRadius;

    [SerializeField]
    private float maxSlopeGrade = 1f;

    [SerializeField]
    private List<float> trajectoryAngles;

    private Observation observation;

    public void RecordObservations()
    {
        observation = default;
        if (trajectoryAngles.Count == 0)
            return;


        Vector3 weightedDirectionalInfluence = Vector3.zero;

        foreach (var angle in trajectoryAngles)
        {
            int counter = 0;
            Vector3 lastGroundPosition = transform.position;
            foreach (var pos in ComputePointsInTrajectory(angle))
            {
                Vector3 rayOrigin = pos + Vector3.up * maxSlopeGrade * stepSize;
                float rayLength = maxSlopeGrade * stepSize * 2 + 0.1f;
                Ray ray = new Ray(rayOrigin, Vector3.down);

                if (Physics.Raycast(ray, out var hit, rayLength, groundLayerMask, QueryTriggerInteraction.Ignore))
                {
                    counter += 1;
                    lastGroundPosition = hit.point + Vector3.up * 0.1f;
                }
                else
                {
                    break;
                }
            }

            weightedDirectionalInfluence += (lastGroundPosition - transform.position);
        }

        if (weightedDirectionalInfluence.sqrMagnitude > 0f)
        {
            observation.relativeRotationOffset = Vector3.SignedAngle(transform.forward, weightedDirectionalInfluence, Vector3.up);
            observation.approximateObservedWalkableDistance = weightedDirectionalInfluence.magnitude / trajectoryAngles.Count;
            observation.averagedHorizonTargetOffset = transform.position + weightedDirectionalInfluence / trajectoryAngles.Count;
        }
    }

    private IEnumerable<Vector3> ComputePointsInTrajectory(float angle)
    {
        Vector3 dir = transform.rotation * Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        for (int i = 0; i < numSteps; i++)
        {
            yield return transform.position + dir * (i + 1) * stepSize;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        foreach (var angle in trajectoryAngles)
        {
            foreach (var pos in ComputePointsInTrajectory(angle))
            {
                Gizmos.DrawWireSphere(pos, testRadius);
            }
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, observation.averagedHorizonTargetOffset);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.rotation * Quaternion.Euler(0f, observation.relativeRotationOffset, 0f) * Vector3.forward * observation.approximateObservedWalkableDistance);
    }
}
