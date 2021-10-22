using System;

public class GameEvents
{
    // Countdown
    public Event OnCountDownBegin;
    public Event<CountDownState> OnCountDownTick;
    public Event OnCountDownEnd;


    // Round end / reset
    public Event<RunResult> OnAllRunnersFinished;
    public Event OnCourseShouldReset;
}
