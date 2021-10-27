using UnityEngine;
using System.Collections;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CourseRunner))]
[RequireComponent(typeof(Rigidbody))]
public class TrainableRunner : Agent
{
    [SerializeField]
    private TrainingArea area;

    [SerializeField]
    private FinishChunkDetector finishArea;

    [SerializeField]
    private CMFMovementController movementController;

    [SerializeField]
    private Transform sensorObject;

    [SerializeField]
    private bool useVectorObservations;

    [SerializeField]
    private TrainingSettings trainingSettings;

    [SerializeField]
    private float turnStepIncrement = 5;

    private CourseRunner runner;
    private Rigidbody agentRb;
    private float currentDirection = 0f;

    private Vector3 originPosition;
    private Vector3 previousPosition;
    private int episodeNumber;

    private void Start()
    {
        runner.Events.OnRunnerEliminationSequenceComplete += () =>
        {
            Debug.Log("Agent eliminated...", this);
            AddReward(trainingSettings.EliminationPenalty);
            EndEpisode();
        };

        finishArea.OnRunnerEnterFinishArea += detectedRunner =>
        {
            if (detectedRunner == runner)
            {
                Debug.Log("Agent entered the finish area!");
                area.OnCourseCompleted?.Invoke();
                AddReward(trainingSettings.FinishingReward);
                EndEpisode();
            }    
        };
    }

    public override void Initialize()
    {
        runner = GetComponent<CourseRunner>();
        agentRb = GetComponent<Rigidbody>();

        originPosition = transform.position;
        previousPosition = originPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObservations)
        {
            sensor.AddObservation(transform.InverseTransformDirection(agentRb.velocity));
            sensor.AddObservation(movementController.IsGrounded());
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (actionBuffers.DiscreteActions.Length == 0)
            return;

        var jumpInput = actionBuffers.DiscreteActions[2] == 1;
        float turnInput = 0f;
        float forwardsInput = 0f;

        switch (actionBuffers.DiscreteActions[0])
        {
            case 1:
                forwardsInput = 1f;
                break;
            case 2:
                forwardsInput = -1f;
                break;
        }

        switch (actionBuffers.DiscreteActions[1])
        {
            case 1:
                turnInput = -1f;
                break;
            case 2:
                turnInput = 1f;
                break;
        }

        currentDirection = (currentDirection + turnInput * turnStepIncrement) % 360f;
        var movementDirection = Quaternion.Euler(0f, currentDirection, 0f) * Vector3.forward;
        var movementInput = new Vector2(movementDirection.x, movementDirection.z) * forwardsInput;

        sensorObject.localRotation = Quaternion.Euler(0f, currentDirection, 0f);

        var currentPosition = transform.position;
        var deltaForward = currentPosition.z - previousPosition.z;
        if (deltaForward < 0)
            deltaForward *= 10;

        AddReward(deltaForward * trainingSettings.ProgressReward);
        AddReward(trainingSettings.StepPenalty);

        if (jumpInput && movementController.IsGrounded())
        {
            float weight = Mathf.Clamp01(Mathf.InverseLerp(250, 2500, episodeNumber));
            AddReward(trainingSettings.JumpPenalty * weight);
        }

        if (turnInput != 0f)
        {
            float weight = Mathf.Clamp01(Mathf.InverseLerp(250, 2500, episodeNumber));
            AddReward(trainingSettings.RotatePenalty * turnStepIncrement * weight);
        }

        runner.MainInput.UpdateInput((ref VirtualRunnerInput.Input i) =>
        {
            i.movementValue = movementInput;
            i.isJumping = jumpInput;
        });

        previousPosition = transform.position;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        if (Keyboard.current.wKey.IsPressed())
        {
            discreteActionsOut[0] = 1;
        }
        else if (Keyboard.current.sKey.IsPressed())
        {
            discreteActionsOut[0] = 2;
        }

        if (Keyboard.current.aKey.IsPressed())
        {
            discreteActionsOut[1] = 1;
        }
        else if (Keyboard.current.dKey.IsPressed())
        {
            discreteActionsOut[1] = 2;
        }

        if (Keyboard.current.spaceKey.IsPressed())
        {
            discreteActionsOut[2] = 1;
        }
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode begin...", this);

        episodeNumber += 1;
        area.OnCourseReset?.Invoke();

        transform.position = originPosition;
        transform.localScale = Vector3.one;
        transform.forward = Vector3.forward;

        previousPosition = originPosition;
        currentDirection = 0f;

        runner.Events.OnRunnerDidReset?.Invoke();
        runner.MainInput.IsInputLocked = false;

        area.OnCourseActivate?.Invoke();
    }
}
