using UnityEngine;
using System.Collections;

public class CourseRunnerEvents : MonoBehaviour
{
    public Event OnRunnerDidSpawn;
    public Event OnRunnerEliminationDetected;
    public Event OnRunnerEliminationSequenceComplete;

    public Event OnRunnerDidReset;
}
