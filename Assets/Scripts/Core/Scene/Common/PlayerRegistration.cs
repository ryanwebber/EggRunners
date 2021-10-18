using System;
public class PlayerRegistration
{
    public PlayerRegistration(AttachableInputSource inputSource, CourseRunner playerPrefab)
    {
        InputSource = inputSource;
        PlayerPrefab = playerPrefab;
    }

    public AttachableInputSource InputSource { get; }
    public CourseRunner PlayerPrefab { get; }
}
