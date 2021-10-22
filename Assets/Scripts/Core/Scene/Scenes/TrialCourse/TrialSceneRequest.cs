using System;

public sealed class TrialSceneRequest : ISceneRequest
{
    private TrialCourseLayout layout;
    private PlayerRegistration playerRegistration;

    public SceneIdentifier Identifier => SceneIdentifier.TRIAL_COURSE_SCENE;

    public TrialSceneRequest(TrialCourseLayout layout, PlayerRegistration playerRegistration)
    {
        this.layout = layout;
        this.playerRegistration = playerRegistration;
    }

    public void UnloadScene(ISceneUnloader unloader)
    {
        unloader.SetContext(layout);
        unloader.SetContext(playerRegistration);
    }
}
