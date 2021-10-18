using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSceneOrganizable : MonoBehaviour
{
    [SerializeField]
    private StringValue group;
    public string Group => group.Value;

    private void Start()
    {
        SceneOrganizer.TryOrganize(this);
        Destroy(this);
    }
}
