using UnityEngine;
using System.Collections;

public class BehaviorController : MonoBehaviour
{
    public Event OnBehaviorEnabled;
    public Event OnBehaviorDisabled;

    public bool IsBehaviorEnabled { get; private set; } = false;

    [SerializeField]
    private Component behavior;
    public Component MainBehavior => behavior;

    private void Awake()
    {
        OnBehaviorEnabled += () => IsBehaviorEnabled = true;
        OnBehaviorDisabled += () => IsBehaviorEnabled = false;
    }
}
