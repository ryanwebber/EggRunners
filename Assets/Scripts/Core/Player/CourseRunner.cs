using UnityEngine;
using System.Collections;

[RequireComponent(typeof(VirtualRunnerInput))]
[RequireComponent(typeof(CourseRunnerEvents))]
public class CourseRunner : MonoBehaviour
{
    [SerializeField]
    private PlayerInputBinder playerInputBinder;

    public CourseRunnerEvents Events { get; private set; }
    public VirtualRunnerInput MainInput { get; private set; }

    public Vector3 Center
    {
        get => transform.position;
        set => transform.position = value;
    }

    private void Awake()
    {
        MainInput = GetComponent<VirtualRunnerInput>();
        Events = GetComponent<CourseRunnerEvents>();

        Events.OnRunnerFinishDetected += () =>
        {
            Debug.Log("Player finish detected", this);
            MainInput.IsInputLocked = true;
            MainInput.ForceUpdateInput((ref VirtualRunnerInput.Input i) => i.movementValue = Vector2.up);
            StartCoroutine(Coroutines.After(0.5f, () =>
            {
                // Make sure someone else hasn't rebooted the player and unlocked their input
                // otherwise we're about to re-lock it, and it'll never get unlocked
                if (MainInput.IsInputLocked)
                    MainInput.ResetInputAndLock();
            }));
        };

        Events.OnRunnerEliminationDetected += () =>
        {
            Debug.Log("Player elimination detected", this);
            MainInput.ResetInputAndLock();
        };

        Events.OnRunnerEliminationSequenceComplete += () =>
        {
            Debug.Log("Player eliminated and elimination sequence complete", this);
        };
    }

    public void MakePlayableByHuman(AttachableInputSource inputSource)
    {
        playerInputBinder?.Bind(inputSource);
    }
}
