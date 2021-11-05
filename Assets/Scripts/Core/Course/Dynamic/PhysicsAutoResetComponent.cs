using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsAutoResetComponent: MonoBehaviour
{
    [SerializeField]
    private DynamicCourseComponent dynamicController;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 startScale;

    private void Awake() {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
        startScale = transform.localScale;

        dynamicController.OnDynamicComponentReset += () => 
        {
            transform.localPosition = startPosition;
            transform.localRotation = startRotation;
            transform.localScale = startScale;

            var rb = GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        };
    }
}
