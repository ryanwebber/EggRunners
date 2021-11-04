using UnityEngine;
using System.Collections;

public class PendulumCourseComponent : MonoBehaviour
{
    [SerializeField, Range(0f, 180f)]
    private float maximumSwingAngle = 120f;

    [SerializeField, Min(0f)]
    private float swingSpeed = 0.1f;

    [SerializeField]
    private DynamicCourseComponent dynamics;

    private float position = 0f;
    private bool isSwinging = true;

    private void Awake()
    {
        dynamics.OnDynamicComponentStart += () => { isSwinging = true; };
        dynamics.OnDynamicComponentReset += () =>
        {
            isSwinging = false;
            position = 0f;
        };
    }

    private void FixedUpdate()
    {
        if (!isSwinging)
            return;

        position += swingSpeed * Time.fixedDeltaTime;
        position = position % (Mathf.PI * 2f);
        
        var t = Mathf.Sin(position);
        var angle = Mathf.LerpUnclamped(0f, maximumSwingAngle, t);
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
