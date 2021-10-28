using System.Collections;
using System.Collections.Generic;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using UnityEngine;

[RequireComponent(typeof(VirtualRunnerInput))]
public class EmergentRunnerController : MonoBehaviour
{
    [SerializeField]
    private CMFMovementController movementController;

    [Header("Sensors")]

    [SerializeField]
    private JumpSensorComponent jumpSensor;

    [SerializeField]
    private WalkSensorComponent walkSensor;

    [SerializeField]
    private List<GameObject> forwardFacingChildren;

    [Header("Movement")]

    [SerializeField]
    private float turnSpeed = 4f;

    private BehaviorTree bt;
    private VirtualRunnerInput input;
    private float forwardsAngle = 0f;

    private void Awake()
    {
        input = GetComponent<VirtualRunnerInput>();
        bt = new BehaviorTreeBuilder(gameObject)
            .Selector()
                .Sequence("Ground control")
                    .Condition("IsGrounded", () => movementController.IsGrounded())
                    .Selector("Jump, drop, or walk")
                        .Sequence("Walk")
                            .Condition(() => walkSensor.CurrentObservation.approximateObservedWalkableDistance > 1f)
                            .Do(() =>
                            {
                                SteerTowards(walkSensor.CurrentObservation.relativeRotationOffset, 1f);
                                return TaskStatus.Success;
                            })
                        .End()
                        .Sequence("Jump")
                            .Condition(() => jumpSensor.CurrentObservation.availableJumpLanding != null)
                            .Do(() =>
                            {
                                SteerTowards(jumpSensor.CurrentObservation.availableJumpLanding.Value.relativeRotationOffset, 1f);
                                DoJump();
                                return TaskStatus.Success;
                            })
                        .End()
                        .Sequence("Drop")
                            .Condition(() => jumpSensor.CurrentObservation.availableDropLanding != null)
                            .Do(() =>
                            {
                                SteerTowards(jumpSensor.CurrentObservation.availableDropLanding.Value.relativeRotationOffset, 1f);
                                return TaskStatus.Success;
                            })
                        .End()
                    .End()
                .End()
                .Do(() =>
                {
                    input.ResetInput();
                    return TaskStatus.Success;
                })
            .End()
            .Build();
    }

    private void FixedUpdate()
    {
        jumpSensor.RecordObservations();
        walkSensor.RecordObservations();
        bt.Tick();

        foreach (var child in forwardFacingChildren)
        {
            child.transform.rotation = Quaternion.Euler(0f, forwardsAngle, 0f);
        }
    }

    private void DoJump()
    {
        if (!input.CurrentInput.isJumping)
        {
            input.UpdateInput((ref VirtualRunnerInput.Input i) =>
            {
                i.isJumping = true;
            });

            StartCoroutine(Coroutines.Until(() => input.CurrentInput.isJumping, () =>
            {
                input.UpdateInput((ref VirtualRunnerInput.Input i) =>
                {
                    i.isJumping = false;
                });
            }));
        }
    }

    private void SteerTowards(float angle, float throttleScale)
    {
        float direction = Mathf.Sign(angle);
        float turnAmount = Mathf.Min(Mathf.Abs(angle), turnSpeed);

        forwardsAngle = (forwardsAngle + turnAmount) % 360f;
        var movementDirection = Quaternion.Euler(0f, forwardsAngle, 0f) * Vector3.forward;
        var movementInput = new Vector2(movementDirection.x, movementDirection.z) * Mathf.Clamp01(throttleScale);
        input.UpdateInput((ref VirtualRunnerInput.Input i) =>
        {
            i.movementValue = movementInput;
        });
    }
}
