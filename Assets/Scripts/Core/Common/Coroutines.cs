using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class Coroutines
{
    public static IEnumerator Next(System.Action action)
    {
        yield return null;
        action?.Invoke();
    }

    public static IEnumerator OnFrameEnd(System.Action action)
    {
        yield return new WaitForEndOfFrame();
        action?.Invoke();
    }

    public static IEnumerator OnFixedUpdate(System.Action action)
    {
        yield return new WaitForFixedUpdate();
        action?.Invoke();
    }

    public static IEnumerator After(float seconds, System.Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    public static IEnumerator After(Coroutine coroutine, System.Action action)
    {
        yield return coroutine;
        action?.Invoke();
    }

    public static IEnumerator After(IEnumerator coroutine, System.Action action)
    {
        yield return coroutine;
        action?.Invoke();
    }

    public static IEnumerator Until(System.Func<bool> predicate)
    {
        yield return new WaitUntil(predicate);
    }

    public static IEnumerator While(System.Func<bool> predicate)
    {
        yield return new WaitWhile(predicate);
    }

    public static IEnumerator All(ICollection<IEnumerator> routines)
    {
        foreach (var routine in routines)
            yield return routine;
    }

    public static IEnumerator All(params IEnumerator[] routines)
    {
        foreach (var routine in routines)
            yield return routine;
    }

    public static IEnumerator All(ICollection<Coroutine> routines)
    {
        foreach (var routine in routines)
            yield return routine;
    }

    public static IEnumerator All(params Coroutine[] routines)
    {
        foreach (var routine in routines)
            yield return routine;
    }

    public static IEnumerator Immediate(System.Action action)
    {
        action?.Invoke();
        yield break;
    }

    public static Task<T> Promisify<T>(Action<Action<T>> cb)
    {
        var tcs = new TaskCompletionSource<T>();

        try
        {
            cb.Invoke(val => tcs.TrySetResult(val));
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }

        return tcs.Task;
    }

    public static Task Promisify(Action<Action> cb)
    {
        var tcs = new TaskCompletionSource<bool>();

        try
        {
            cb.Invoke(() => tcs.TrySetResult(true));
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }

        return tcs.Task;
    }
}
