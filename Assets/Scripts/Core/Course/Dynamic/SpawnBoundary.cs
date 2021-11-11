using UnityEngine;
using System.Collections;

public class SpawnBoundary : MonoBehaviour
{
    [SerializeField]
    private Bounds bounds;

    public Bounds WorldBounds => new Bounds(bounds.center + transform.position, bounds.size);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(WorldBounds.center, WorldBounds.size);
    }
}
