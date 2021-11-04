using UnityEngine;
using System.Collections;

public class DynamicCourseComponent : MonoBehaviour
{
    public Event OnDynamicComponentStart;
    public Event OnDynamicComponentReset;

    private void Start()
    {
        var chunk = GetComponentInParent<CourseChunk>();
        if (chunk != null)
        {
            chunk.OnChunkDynamicsActivated += () => OnDynamicComponentStart?.Invoke();
            chunk.OnChunkDynamicsReset += () => OnDynamicComponentReset?.Invoke();
        }    
    }
}
