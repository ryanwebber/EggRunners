using UnityEngine;
using System.Collections;
using System;

public class GameState : MonoBehaviour
{
    public Event OnRegisterServices;
    public Event OnInitializeServices;

    public GameEvents Events { get; } = new GameEvents();
    public GameServices Services { get; } = new GameServices();

    public void BootGame()
    {
        OnRegisterServices?.Invoke();
        OnInitializeServices?.Invoke();
    }
}
