using System;
using System.Collections.Generic;

public class GameServices
{
    private Dictionary<Type, object> services;

    public GameServices()
    {
        services = new Dictionary<Type, object>();
    }

    public void RegisterService<T>(T service) where T: class
    {
        var type = typeof(T);
        if (services.ContainsKey(type))
            throw new System.Exception($"Service already registered: { type.Name }");

        services[type] = service;
    }

    public T GetService<T>() where T: class
    {
        var type = typeof(T);
        if (!services.ContainsKey(type))
            throw new System.Exception($"Service not registered: { type.Name }");

        return (T)services[type];
    }
}
