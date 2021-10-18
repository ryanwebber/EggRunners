using System;
using UnityEngine;

public static class InputSourceUtils
{
    private static IInputSource DetatchedInstance = new DetachedInputSource();
    public static IInputSource DetachedSource => DetatchedInstance;

    private class DetachedInputSource : IInputSource
    {
        public Vector2 MovementValue => Vector2.zero;
        public Event OnMovementSpecialAction { get; set; }
        public PlayerIdentifier InputIdentifier => PlayerIdentifier.Default;
    }
}
