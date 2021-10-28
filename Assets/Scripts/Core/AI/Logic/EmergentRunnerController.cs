using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CourseRunner))]
[RequireComponent(typeof(VirtualRunnerInput))]
[RequireComponent(typeof(CMFMovementController))]
public class EmergentRunnerController : MonoBehaviour
{
    private CourseRunner runner;
    private VirtualRunnerInput input;
    private CMFMovementController movementController;

    private void Awake()
    {
        runner = GetComponent<CourseRunner>();
        input = GetComponent<VirtualRunnerInput>();
        movementController = GetComponent<CMFMovementController>();
    }

    private void FixedUpdate()
    {
        
    }
}
