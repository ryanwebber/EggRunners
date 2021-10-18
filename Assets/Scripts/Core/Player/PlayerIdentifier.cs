using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdentifier
{
    public int PlayerIndex { get; private set; }

    public string Key => $"{PlayerIndex}";

    public PlayerIdentifier(int playerIndex)
    {
        PlayerIndex = playerIndex;
    }

    public static bool TryParse(string key, out PlayerIdentifier identifier)
    {
        var segments = key.Split(';');
        if (segments.Length >= 1 && int.TryParse(segments[0], out var playerIndex))
        {
            identifier = new PlayerIdentifier(playerIndex);
            return true;
        }

        identifier = default;
        return false;
    }

    public static PlayerIdentifier Default => new PlayerIdentifier(0);
}
