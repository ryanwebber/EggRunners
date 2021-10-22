using System;

public class SceneIdentifier
{
    public static SceneIdentifier PERSISTANT_SCENE = new SceneIdentifier("PersistantScene");
    public static SceneIdentifier TRIAL_COURSE_SCENE = new SceneIdentifier("TrialCourseScene");

    private string name;
    public string Name => name;

    public SceneIdentifier(string name)
    {
        this.name = name;
    }

    public static implicit operator string(SceneIdentifier identifier) => identifier.name;
}
