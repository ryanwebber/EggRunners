using UnityEngine;
using System.Collections;

public class MeshGizmo : MonoBehaviour
{
    [SerializeField]
    private Color color;

    [SerializeField]
    private Mesh mesh;

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawMesh(mesh, transform.position, transform.rotation, transform.localScale);
    }
}
