using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneOrganizer : MonoBehaviour
{
    private static SceneOrganizer instance;

    [System.Serializable]
    public struct GroupEntry
    {
        public StringValue group;
        public Transform parent;
    }

    [SerializeField]
    private List<GroupEntry> groups;

    private Dictionary<string, Transform> mapping;

    private void Awake()
    {
        instance = this;

        mapping = new Dictionary<string, Transform>();
        foreach (var entry in groups)
        {
            mapping[entry.group] = entry.parent;
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public static bool TryOrganize(AutoSceneOrganizable organizable)
    {
        if (instance == null)
            return false;

        if (instance.mapping.TryGetValue(organizable.Group, out var transform) && organizable.transform.parent == null)
        {
            organizable.transform.parent = transform;
            return true;
        }

        return false;
    }
}
