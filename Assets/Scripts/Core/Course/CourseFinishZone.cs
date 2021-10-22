using UnityEngine;
using System.Collections;

public class CourseFinishZone : MonoBehaviour
{
    [SerializeField]
    private Bounds bounds;

    private BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    public Bounds FinishBounds
    {
        get
        {
            var center = transform.TransformPoint(bounds.center);
            var size = bounds.size;
            return new Bounds(center, size);
        }
    }

    private void OnDrawGizmos()
    {
        var rectBounds = FinishBounds;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(rectBounds.center, rectBounds.size);
    }
}
