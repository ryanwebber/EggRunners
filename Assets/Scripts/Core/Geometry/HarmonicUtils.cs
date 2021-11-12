using System;

public static class HarmonicUtils
{
    public static float GetRotation(float time, float length, float gravity, float initialAngularDisplacement)
    {
        float w = MathF.Sqrt(gravity / length);
        return initialAngularDisplacement * MathF.Cos(w * time);
    }
}
