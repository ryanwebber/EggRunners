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
        MainInput.IsInputLocked = true;

        Events = GetComponent<CourseRunnerEvents>();

        Events.OnRunnerFinishDetected += () =>
        {
            Debug.Log("Player finish detected", this);
            MainInput.IsInputLocked = true;
            MainInput.ForceUpdateInput((ref VirtualRunnerInput.Input i) => i.movementValue = Vector2.up);
            StartCoroutine(Coroutines.After(0.5f, () => MainInput.ResetInputAndLock()));
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
