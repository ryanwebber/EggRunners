using UnityEngine;
using System.Collections;

public class HarmonicSwingComponent : MonoBehaviour
{
    [System.Serializable]
    private enum Axis { X, Y, Z }

    [SerializeField]
    private float initialRotationDisplacement;

    [SerializeField]
    private float armLength;

    [SerializeField]
    private float gravitationalForce;

    [SerializeField]
    private Axis rotationAxis;

    [SerializeField]
    private DynamicCourseComponent dynamicController;

    private float rotationStartTime = 0f;

    private void Awake()
    {
        rotationStartTime = Time.fixedTime;
        SetAngularDisplacement(initialRotationDisplacement);

        dynamicController.OnDynamicComponentReset += () =>
        {
            rotationStartTime = Time.fixedTime;
            SetAngularDisplacement(initialRotationDisplacement);
        };
    }

    private void FixedUpdate()
    {
        SetAngularDisplacement(HarmonicUtils.GetRotation(Time.fixedTime - rotationStartTime, armLength, gravitationalForce, initialRotationDisplacement));
    }

    private void SetAngularDisplacement(float theta)
    {
        switch (rotationAxis)
        {
            case Axis.X:
                transform.localRotation = Quaternion.Euler(theta, 0f, 0f);
                break;
            case Axis.Y:
                transform.localRotation = Quaternion.Euler(0f, theta, 0f);
                break;
            case Axis.Z:
                transform.localRotation = Quaternion.Euler(0f, 0f, theta);
                break;
        }
    }
}
