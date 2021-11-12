using UnityEngine;
using System.Collections;

public class ConstantRotationComponent : MonoBehaviour
{
    [System.Serializable]
    private enum Axis { X, Y, Z }

    [SerializeField]
    private Axis rotationAxis;

    [SerializeField]
    private float speed;

    private void FixedUpdate()
    {
        float degreeChange = Time.fixedDeltaTime * speed;
        switch (rotationAxis)
        {
            case Axis.X:
                transform.Rotate(degreeChange, 0f, 0f, Space.Self);
                break;
            case Axis.Y:
                transform.Rotate(0f, degreeChange, 0f, Space.Self);
                break;
            case Axis.Z:
                transform.Rotate(0f, 0f, degreeChange, Space.Self);
                break;
        }
    }
}
