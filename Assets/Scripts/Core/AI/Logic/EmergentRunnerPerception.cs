using UnityEngine;
using System.Collections;

public class EmergentRunnerPerception : MonoBehaviour
{
    [SerializeField]
    private JumpSensorComponent jumpSensor;

    [SerializeField]
    private WalkSensorComponent walkSensor;

    private void FixedUpdate()
    {
        jumpSensor.RecordObservations();
        walkSensor.RecordObservations();
    }
}
