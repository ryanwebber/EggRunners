using System;

public class TrialParameters
{
    public PlayerRegistration PlayerInfo { get; }
    public TrialCourseLayout CourseLayout { get; }

    public CourseRoster PlayerRoster => new CourseRoster(new PlayerRegistration[] { PlayerInfo });

    public TrialParameters(PlayerRegistration playerInfo, TrialCourseLayout courseLayout)
    {
        PlayerInfo = playerInfo;
        CourseLayout = courseLayout;
    }
}
