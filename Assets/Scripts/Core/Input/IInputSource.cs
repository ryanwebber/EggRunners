using System;
using UnityEngine;

public interface IInputSource
{
    Vector2 MovementValue { get; }
    bool IsJumpPressed { get; }
    bool IsDashPressed { get; }

    PlayerIdentifier InputIdentifier { get; }
}
