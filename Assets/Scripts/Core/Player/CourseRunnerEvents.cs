using UnityEngine;
using System.Collections;

public class CourseRunnerEvents : MonoBehaviour
{
    public Event OnRunnerDidSpawn;
    public Event<Elimination> OnRunnerElimination;
}
