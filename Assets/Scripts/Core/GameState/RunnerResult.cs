using System;

public struct RunnerResult
{
    public int playerIndex;
    public float runDuration;
    public Nullable<Elimination> eliminationDetails;

    public bool WasEliminated => eliminationDetails.HasValue;
    public bool DidFinish => !WasEliminated;
}
