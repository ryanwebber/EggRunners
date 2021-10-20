using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CountDownState
{
    public int ticksRemaining;
    public int countdownDuration;

    public static IEnumerable<(CountDownState state, T step)> WithCount<T>(int ticks, Func<T> fn)
    {
        for (int i = ticks; i >= 0; i--)
        {
            var step = fn.Invoke();
            var state = new CountDownState
            {
                ticksRemaining = i,
                countdownDuration = ticks,
            };

            yield return (state, step);
        }
    }
}
