using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;

public struct BehaviorRequest
{
    public Type[] types;

    public BehaviorRequest(params Type[] types)
    {
        this.types = types;
    }

    public static BehaviorRequest Of<T1>()
    {
        return new BehaviorRequest(typeof(T1));
    }

    public static BehaviorRequest Of<T1, T2>()
    {
        return new BehaviorRequest(typeof(T1), typeof(T2));
    }

    public static BehaviorRequest Of<T1, T2, T3>()
    {
        return new BehaviorRequest(typeof(T1), typeof(T2), typeof(T3));
    }

    public static BehaviorRequest Of<T1, T2, T3, T4>()
    {
        return new BehaviorRequest(typeof(T1), typeof(T2));
    }
}

public struct BehaviorPair
{
    public object behavior;
    public BehaviorController controller;
}

public class BehaviorSet
{
    private Dictionary<Type, BehaviorPair> behaviors;

    public BehaviorSet()
    {
        this.behaviors = new Dictionary<Type, BehaviorPair>();
    }

    public BehaviorSet(Dictionary<Type, BehaviorPair> behaviors)
    {
        this.behaviors = behaviors;
    }

    public T Get<T>() where T : class
    {
        var type = typeof(T);
        if (!behaviors.ContainsKey(type))
            throw new System.Exception($"Behavior not registered: { type.Name }");

        return (T)behaviors[type].behavior;
    }
}

public class BehaviorMixer : MonoBehaviour
{
    [SerializeField]
    private List<BehaviorController> controllers;

    private HashSet<BehaviorController> enabledControllers;

    private void Awake()
    {
        enabledControllers = new HashSet<BehaviorController>();
    }

    public BehaviorSet RequestAll(params BehaviorRequest[] requests)
    {
        bool TryGetBehavior(Type t, out BehaviorPair pair)
        {
            foreach (var controller in controllers)
            {
                if (t.IsInstanceOfType(controller.MainBehavior))
                {
                    pair = new BehaviorPair
                    {
                        behavior = controller.MainBehavior,
                        controller = controller,
                    };

                    return true;
                }
            }

            pair = default;
            return false;
        }

        Dictionary<Type, BehaviorPair> foundTypes = new Dictionary<Type, BehaviorPair>();
        foreach (var request in requests)
        {
            foreach (var type in request.types)
            {
                if (foundTypes.ContainsKey(type))
                    continue;

                if (TryGetBehavior(type, out var pair))
                {
                    foundTypes[type] = pair;
                }
                else
                {
                    throw new System.Exception($"Unable to find behavor '{type.Name}' in controller list");
                }
            }
        }

        foreach (var pair in foundTypes.Values)
        {
            if (enabledControllers.Contains(pair.controller))
                enabledControllers.Remove(pair.controller);
            else
                pair.controller.OnBehaviorEnabled?.Invoke();
        }

        foreach (var controller in enabledControllers)
            controller.OnBehaviorDisabled?.Invoke();

        enabledControllers.Clear();
        foreach (var pair in foundTypes.Values)
        {
            enabledControllers.Add(pair.controller);
        }

        return new BehaviorSet(foundTypes);
    }
}
