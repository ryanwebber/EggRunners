using UnityEngine;
using System.Collections;

public class GroundTaxi : MonoBehaviour
{
    [SerializeField]
    private Transform parent;

    public GroundContact CreateContact(Vector3 position, GameObject reference)
    {
        var instance = new GameObject($"Ground Contact ({reference.name})");
        instance.transform.parent = parent;
        instance.transform.position = position;

        instance.AddComponent<GroundContact>();
        instance.GetComponent<GroundContact>().LastPosition = position;

        return instance.GetComponent<GroundContact>();
    }
}
