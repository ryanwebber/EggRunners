using UnityEngine;
using System.Collections;

public class PlayerInputBinder : MonoBehaviour
{
    public Event<IInputSource> OnAttachToInput;
    public Event<IInputFeedback> OnAttatchToFeedback;

    public void Bind(IInputSource source, IInputFeedback feedback)
    {
        OnAttachToInput?.Invoke(source);
        OnAttatchToFeedback?.Invoke(feedback);
    }

    public void Bind(AttachableInputSource attachableInputSource)
    {
        Bind(attachableInputSource.MainSource, attachableInputSource.MainFeedback);
    }
}
