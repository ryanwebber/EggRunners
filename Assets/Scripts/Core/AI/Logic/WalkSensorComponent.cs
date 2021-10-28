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
        public Vector3 averagedHorizonTarget;
    }

    [System.Serializable]
    public class Sensor
    {
        [SerializeField]
        public LayerMask layerMask;

        [SerializeField, Min(1f)]
        public int numSteps = 5;

        [SerializeField, Min(0f)]
        public float stepSize = 2;

        [SerializeField]
        public float angleOffset = 0;

        [SerializeField]
        public float maxSlopeGrade = 1;

        [SerializeField]
        public float weight = 1f;
    }

    [SerializeField]
    private CMFMovementController movementController;

    [SerializeField]
    private List<Sensor> sensors;

    private Observation observation;
    public Observation CurrentObservation => observation;

    public void RecordObservations()
    {
        observation = default;
        if (sensors.Count == 0)
            return;


        Vector3 weightedDirectionalInfluence = Vector3.zero;

        float averageDistanceToMissedRays = 0f;
        int averageDistanceCalculations = 0;

        foreach (var sensor in sensors)
        {
            Vector3 lastGroundPosition = transform.position;

            foreach (var pos in ComputePointsInTrajectory(sensor))
            {
                Vector3 rayOrigin = pos + Vector3.up * sensor.maxSlopeGrade * sensor.stepSize;
                float rayLength = sensor.maxSlopeGrade * sensor.stepSize * 2 + 0.1f;
                Ray ray = new Ray(rayOrigin, Vector3.down);

                if (Physics.Raycast(ray, out var hit, rayLength, sensor.layerMask, QueryTriggerInteraction.Ignore))
                {
                    lastGroundPosition = hit.point + Vector3.up * 0.1f;
                }
                else
                {
                    Vector3 a = new Vector3(transform.position.x, 0f, transform.position.y);
                    Vector3 b = new Vector3(rayOrigin.x, 0f, rayOrigin.y);
                    averageDistanceToMissedRays += Vector3.Distance(a, b);
                    averageDistanceCalculations += 1;
                    break;
                }
            }

            weightedDirectionalInfluence += (lastGroundPosition - transform.position) * (1f / sensors.Count) * sensor.weight;
        }

        Debug.DrawLine(transform.position, transform.position + weightedDirectionalInfluence, Color.blue);

        if (weightedDirectionalInfluence.sqrMagnitude > 0f)
        {
            observation.relativeRotationOffset = Vector3.SignedAngle(transform.forward, weightedDirectionalInfluence, Vector3.up);
            observation.averagedHorizonTarget = transform.position + weightedDirectionalInfluence;
            observation.approximateObservedWalkableDistance = averageDistanceCalculations == 0 ? float.PositiveInfinity : averageDistanceToMissedRays / averageDistanceCalculations;
        }
    }

    private IEnumerable<Vector3> ComputePointsInTrajectory(Sensor sensor)
    {
        Vector3 dir = transform.rotation * Quaternion.Euler(0f, sensor.angleOffset, 0f) * Vector3.forward;
        for (int i = 0; i < sensor.numSteps; i++)
        {
            yield return transform.position + dir * (i + 1) * sensor.stepSize;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        foreach (var sensor in sensors)
        {
            foreach (var pos in ComputePointsInTrajectory(sensor))
            {
                Gizmos.DrawWireSphere(pos, 0.2f);
            }
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, observation.averagedHorizonTarget);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.rotation * Quaternion.Euler(0f, observation.relativeRotationOffset, 0f) * Vector3.forward * observation.approximateObservedWalkableDistance);
    }
}
