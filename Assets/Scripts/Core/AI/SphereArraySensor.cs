using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereArraySensor : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int depth = 5;

    [SerializeField, Min(1)]
    private int rowCount = 2;

    [SerializeField, Min(0f)]
    private float rowSpacing = 2;

    [SerializeField, Range(0f, 180f)]
    private float theta = 45;

    [SerializeField]
    private float castRadius = 12;

    [SerializeField]
    private float sphereRadius = 0.5f;

    [SerializeField]
    private Vector3 origin = Vector3.zero;

    [SerializeField, ReadOnly]
    private int numPoints = 0;

    private List<Vector3> points;

    private void RecalculatePoints()
    {
        if (points == null)
            points = new List<Vector3>();
        else
            points.Clear();

        float totalHeight = (rowCount - 1) * rowSpacing;

        for (int r = 0; r < rowCount; r++)
        {
            float yOffset = (-totalHeight / 2f) + r * rowSpacing;
            for (int i = 0; i < depth; i++)
            {
                int sphereCount = i + 1;
                for (int j = 0; j < sphereCount; j++)
                {
                    float angle = sphereCount <= 1 ? 0 : Mathf.Lerp(-theta / 2f, theta / 2f, (float)j / (sphereCount - 1));
                    Vector3 direction = transform.rotation * Quaternion.Euler(new Vector3(0f, angle, 0f)) * Vector3.forward;
                    Vector3 position = origin + direction * ((float)i / depth) * castRadius + transform.up * yOffset;

                    points.Add(position);
                }
            }
        }

        numPoints = points.Count;
    }

    private void OnValidate()
    {
        RecalculatePoints();
    }

    private void OnDrawGizmos()
    {
        if (points == null)
            return;

        Gizmos.color = Color.red;
        foreach (var point in points)
        {
            Gizmos.DrawWireSphere(transform.position + point, sphereRadius);
        }
    }
}
