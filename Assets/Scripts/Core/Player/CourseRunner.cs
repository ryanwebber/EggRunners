﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(VirtualRunnerInput))]
[RequireComponent(typeof(CourseRunnerEvents))]
public class CourseRunner : MonoBehaviour
{
    [SerializeField]
    private PlayerInputBinder playerInputBinder;

    public CourseRunnerEvents Events { get; private set; }
    public VirtualRunnerInput MainInput { get; private set; }

    private void Awake()
    {
        MainInput = GetComponent<VirtualRunnerInput>();
        MainInput.IsInputLocked = true;

        Events = GetComponent<CourseRunnerEvents>();
        Events.OnRunnerElimination += _ => MainInput.IsInputLocked = true;
    }

    public void MakePlayableByHuman(AttachableInputSource inputSource)
    {
        playerInputBinder?.Bind(inputSource);
    }
}
