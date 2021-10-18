using System;

public class SceneStoreKey
{
    private static int COUNTER = 0;

    private int id;

    private SceneStoreKey()
    {
        id = COUNTER++;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj != null && obj is SceneStoreKey key)
            return key.id == id;

        return false;
    }

    public static SceneStoreKey CreateUnique()
    {
        return new SceneStoreKey();
    }
}
