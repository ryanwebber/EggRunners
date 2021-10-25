using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInputBinder))]
public class AutoInputBinding : MonoBehaviour
{
    [SerializeField]
    private AttachableInputSource inputSourcePrefab;

    private void Start()
    {
        var instance = Instantiate(inputSourcePrefab, transform);
        GetComponent<PlayerInputBinder>().Bind(instance);
    }
}
