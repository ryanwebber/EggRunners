using System;
using System.Collections.Generic;

public struct RunResult
{
    public CourseRoster roster;
    public IReadOnlyList<RunnerResult> runnerResults;
}
