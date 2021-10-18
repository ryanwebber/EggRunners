using System;

public delegate void Event();
public delegate void Event<T1>(T1 arg1);
public delegate void Event<T1, T2>(T1 arg1, T2 arg2);
public delegate void Event<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
public delegate void Event<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

public static class Events
{
    public static void On(ref Event e1, ref Event e2, System.Action action)
    {
        e1 += () => action?.Invoke();
        e2 += () => action?.Invoke();
    }

    public static void On(ref Event e1, ref Event e2, Event e3, System.Action action)
    {
        e1 += () => action?.Invoke();
        e2 += () => action?.Invoke();
        e3 += () => action?.Invoke();
    }

    public static void On<T1>(ref Event<T1> e1, ref Event<T1> e2, System.Action<T1> action)
    {
        e1 += (arg1) => action?.Invoke(arg1);
        e2 += (arg1) => action?.Invoke(arg1);
    }

    public static void On<T1>(ref Event<T1> e1, ref Event<T1> e2, ref Event<T1> e3, System.Action<T1> action)
    {
        e1 += (arg1) => action?.Invoke(arg1);
        e2 += (arg1) => action?.Invoke(arg1);
        e3 += (arg1) => action?.Invoke(arg1);
    }
}