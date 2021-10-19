using UnityEngine;
using System.Collections;

public class ClippingPlane : MonoBehaviour
{
    public Plane Plane
    {
        get
        {
            return new Plane(transform.forward, transform.position);
        }
    }

    public Vector4 PlanarData
    {
        get
        {
            var plane = Plane;
            return new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, Plane.normal);
    }
}
