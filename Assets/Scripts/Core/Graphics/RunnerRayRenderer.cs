using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(LineRenderer))]
public class RunnerRayRenderer : MonoBehaviour
{
    [SerializeField]
    private Vector3 direction;

    [SerializeField]
    private float drawOffset;

    [SerializeField]
    private LayerMask rayLayermask;

    [SerializeField]
    private float maxRayLength = 1000f;

    [SerializeField]
    private DecalProjector decalProjector;

    private LineRenderer rayRenderer;

    private void Awake()
    {
        direction.Normalize();
        rayRenderer = GetComponent<LineRenderer>();
        rayRenderer.useWorldSpace = false;
        rayRenderer.SetPositions(new Vector3[] {
            Vector3.zero, Vector3.zero,
        });
    }

    private void LateUpdate()
    {
        UpdateRay();
    }

    private void UpdateRay()
    {
        var ray = new Ray(transform.position, direction);
        var eventualHitPoint = ray.origin + ray.direction * maxRayLength;
        var collision = Physics.Raycast(ray, out var hit, maxRayLength, rayLayermask, QueryTriggerInteraction.Ignore);
        if (collision)
        {
            eventualHitPoint = hit.point;

            if (!decalProjector.enabled)
                decalProjector.enabled = true;

            UpdateDecal(hit.point + hit.normal * 0.001f, hit.normal);
        }
        else
        {
            decalProjector.enabled = false;
        }

        rayRenderer.SetPosition(0, transform.InverseTransformPoint(ray.origin) + direction * drawOffset);
        rayRenderer.SetPosition(1, transform.InverseTransformPoint(eventualHitPoint) - direction * drawOffset);
    }

    private void UpdateDecal(Vector3 position, Vector3 surfaceNormal)
    {
        decalProjector.transform.position = position;
        decalProjector.transform.forward = -surfaceNormal;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 1f, 1f);
        Gizmos.DrawRay(transform.position, direction);
    }
}
