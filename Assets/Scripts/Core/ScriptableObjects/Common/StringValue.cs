using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StringValue", menuName = "Custom/Primitive/String")]
public class StringValue : ScriptableObject
{
    [SerializeField]
    private string value;
    public string Value => value;

    private void OnValidate()
    {
        if (value == null || value == "")
            value = name;
    }

    public static implicit operator string(StringValue value)
    {
        return value.Value;
    }
}
