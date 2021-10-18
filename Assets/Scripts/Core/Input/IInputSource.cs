using System;
using UnityEngine;

public interface IInputSource
{
    Event OnMovementSpecialAction { get; set; }
    Vector2 MovementValue { get; }
    PlayerIdentifier InputIdentifier { get; }
}
