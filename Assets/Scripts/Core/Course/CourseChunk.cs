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

    [SerializeField]
    private int endHeight = 0;
    public int EndHeight => endHeight * Globals.ChunkScale.y;

    public Bounds Bounds
    {
        get
        {
            var size = new Vector3(Width, EndHeight, Length);
            return new Bounds(transform.position + Vector3.up * (EndHeight / 2f), size);
        }
    }

    private void OnDrawGizmos()
    {
        var bounds = this.Bounds;

        var leftStart = new Vector3(-bounds.extents.x, 0.5f * Globals.ChunkScale.y, -bounds.extents.z);
        var leftEnd = new Vector3(-bounds.extents.x, 0.5f * Globals.ChunkScale.y + bounds.extents.y * 2f, bounds.extents.z);
        var rightStart = new Vector3(bounds.extents.x, 0.5f * Globals.ChunkScale.y, -bounds.extents.z);
        var rightEnd = new Vector3(bounds.extents.x, 0.5f * Globals.ChunkScale.y + bounds.extents.y * 2f, bounds.extents.z);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(leftStart, leftEnd);
        Gizmos.DrawLine(rightStart, rightEnd);
        Gizmos.DrawLine(leftStart, rightStart);
        Gizmos.DrawLine(leftEnd, rightEnd);
    }
}
