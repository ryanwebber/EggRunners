using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CourseBuilder : MonoBehaviour
{
    [SerializeField]
    private Vector3 offset;

    public void LayoutCourse(IEnumerable<CourseChunk> chunks)
    {
        Vector3 origin = transform.position + offset;
        float zAccumulator = 0;
        float yAccumulator = 0f;

        foreach (var instance in chunks)
        {
            int lengthInWorldUnits = instance.Length;
            float zOffset = zAccumulator + lengthInWorldUnits / 2f;
            float yOffset = instance.EndHeight;
            instance.transform.position = origin + Vector3.forward * zOffset + Vector3.up * yAccumulator;

            // Squish on the z-axis very slightly. Prevent the back face of adjacent
            // meshes from rendering over eachother
            instance.transform.localScale = Vector3.one - (Vector3.forward * 0.0001f);

            zAccumulator += lengthInWorldUnits;
            yAccumulator += instance.EndHeight;
        }
    }
}
