using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CountDownState
{
    public int secondsRemaining;
    public int countdownDuration;

    public static IEnumerable<(CountDownState state, T step)> WithDuration<T>(int seconds, Func<T> fn)
    {
        for (int i = seconds; i >= 0; i--)
        {
            var step = fn.Invoke();
            var state = new CountDownState
            {
                secondsRemaining = i,
                countdownDuration = seconds,
            };

            yield return (state, step);
        }
    }
}
