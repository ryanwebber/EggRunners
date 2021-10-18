using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerInputBinder))]
public class PlayerInputFeedback : MonoBehaviour
{
    private IInputFeedback feedback;

    private void Awake()
    {
        GetComponent<PlayerInputBinder>().OnAttatchToFeedback += feedback =>
        {
            this.feedback = feedback;
        };

        this.feedback = InputFeedbackUtils.DetachedFeedback;
    }

    public void TriggerHapticInstant()
    {
        feedback.OnTriggerHapticFeedback?.Invoke();
    }
}
