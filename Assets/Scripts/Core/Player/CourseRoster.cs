using System;
using System.Collections.Generic;
using UnityEngine;

public class CourseRoster
{
    public IReadOnlyList<PlayerRegistration> players;

    public CourseRoster(IEnumerable<PlayerRegistration> players)
    {
        this.players = new List<PlayerRegistration>(players);
    }
}
