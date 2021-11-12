using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class GroundContact : MonoBehaviour
{
    public Vector3 LastPosition { get; set; }
    public Vector3 CurrentPosition => transform.position;
    public Vector3 Displacement => CurrentPosition - LastPosition;

    private Collider attachedCollider;
    public Collider GroundCollider => attachedCollider;

    private void Awake()
    {
        attachedCollider = GetComponentInParent<Collider>();
        Assert.IsNotNull(attachedCollider);
    }

    private void FixedUpdate()
    {
        LastPosition = transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }

    public void Detach()
    {
        Destroy(gameObject);
    }
}
