using System;

public class TrialParameters
{
    public CourseRoster PlayerRoster { get; }
    public TrialCourseLayout CourseLayout { get; }

    public TrialParameters(CourseRoster playerRoster, TrialCourseLayout courseLayout)
    {
        PlayerRoster = playerRoster;
        CourseLayout = courseLayout;
    }
}
