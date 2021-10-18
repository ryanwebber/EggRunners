using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IntValue", menuName = "Custom/Primitive/Integer")]
public class IntValue : ScriptableObject
{
    [SerializeField]
    private int value;
    public int Value => value;

    public static implicit operator int(IntValue value)
    {
        return value.Value;
    }
}
