using System;

public class GameEvents
{
    // Countdown
    public Event OnCountDownBegin;
    public Event<CountDownState> OnCountDownTick;
    public Event OnCountDownEnd;

    // Player events
    public Event<RunnerResult> OnRunnerResult;

    // Round end / reset
    public Event<RunResult> OnAllRunnersFinished;
    public Event OnCourseShouldReset;
}
