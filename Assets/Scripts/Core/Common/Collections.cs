using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Collections
{
    public static void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var fringeIndex = list.Count - i - 1;
            var swapIndex = Random.Range(0, fringeIndex + 1);
            if (fringeIndex != swapIndex)
            {
                var tmp = list[fringeIndex];
                list[fringeIndex] = list[swapIndex];
                list[swapIndex] = tmp;
            }
        }
    }
}
