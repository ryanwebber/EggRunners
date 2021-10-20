using System;
using UnityEngine;

public interface IInputSource
{
    Vector2 MovementValue { get; }
    bool IsJumpPressed { get; }

    PlayerIdentifier InputIdentifier { get; }
}
