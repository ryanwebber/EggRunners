using System;

public interface ISceneRequest
{
    public SceneIdentifier Identifier { get; }
    public void UnloadScene(ISceneUnloader unloader);
}
