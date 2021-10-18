using System;

public static class InputFeedbackUtils
{
    private static IInputFeedback DetatchedInstance = new DetachedInputFeedback();
    public static IInputFeedback DetachedFeedback => DetatchedInstance;

    private class DetachedInputFeedback : IInputFeedback
    {
        public Event OnTriggerHapticFeedback { get; set; }
    }
}
