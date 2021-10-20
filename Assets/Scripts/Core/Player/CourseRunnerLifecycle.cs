using UnityEngine;
using System.Collections;

public class CourseRunnerLifecycle : MonoBehaviour
{
    public Event OnDidLeavePhysicsBounds;

    [SerializeField]
    private LayerMask physicsBoundsLayer;

    [SerializeField]
    private CMF.Mover movementController;

    private void Awake()
    {
        OnDidLeavePhysicsBounds += () =>
        {
            Debug.Log("Player left main physics bounds", this);
            movementController.BaseLayerMask = 0;
        };
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & physicsBoundsLayer.value) != 0)
        {
            OnDidLeavePhysicsBounds?.Invoke();
        }
    }
}
