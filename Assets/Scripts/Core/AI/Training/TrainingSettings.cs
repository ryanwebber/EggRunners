using System;

[System.Serializable]
public class TrainingSettings
{
    public float ProgressReward = 1f;
    public float JumpPenalty = -0.25f;
    public float StepPenalty = -0.001f;
    public float RotatePenalty = -0.01f;

    public float EliminationPenalty = -10f;
    public float FinishingReward = 100f;
}
