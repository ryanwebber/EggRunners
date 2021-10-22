using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourseChunk : MonoBehaviour
{
    public Event OnChunkDynamicsReset;
    public Event OnChunkDynamicsActivated;

    [SerializeField]
    [Min(1)]
    private int unitWidth = 5;
    public int Width => unitWidth * Globals.ChunkScale.x;

    [SerializeField]
    private int unitLength = 8;
    public int Length => unitLength * Globals.ChunkScale.z;

    public Bounds Bounds
    {
        get
        {
            var size = new Vector3(Width, Globals.ChunkScale.y, Length);
            return new Bounds(transform.position, size);
        }
    }

    private void OnDrawGizmos()
    {
        var bounds = this.Bounds;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
