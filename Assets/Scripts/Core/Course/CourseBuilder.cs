using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CourseBuilder : MonoBehaviour
{
    [SerializeField]
    private Vector3 offset;

    public void LayoutCourse(IEnumerable<CourseChunk> chunks)
    {
        float accumulatedDistance = 0;
        Vector3 origin = transform.position + offset;

        foreach (var instance in chunks)
        {
            int lengthInWorldUnits = instance.Length;
            float zOffset = accumulatedDistance + lengthInWorldUnits / 2f;
            instance.transform.position = origin + Vector3.forward * zOffset;

            // Squish on the z-axis very slightly. Prevent the back face of adjacent
            // meshes from rendering over eachother
            instance.transform.localScale = Vector3.one - (Vector3.forward * 0.0001f);

            accumulatedDistance += lengthInWorldUnits;
        }
    }
}
