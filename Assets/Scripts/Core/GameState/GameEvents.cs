using System;

public class GameEvents
{
    public Event OnCountDownBegin;
    public Event<CountDownState> OnCountDownTick;
    public Event OnCountDownEnd;
}
