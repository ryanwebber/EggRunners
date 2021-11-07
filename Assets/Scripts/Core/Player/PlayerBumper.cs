using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerBumper : MonoBehaviour
{
    [SerializeField, Range(-1f, 1f)]
    private float interpolation;

    [SerializeField, Min(0f)]
    private float radius;

    [SerializeField, Min(0f)]
    private float dynamicBodyForceScale;

    [SerializeField, Min(0f)]
    private float playerBodyForceScale;

    [SerializeField, Min(0f)]
    private float duration;

    [SerializeField]
    private Vector3 extent;

    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private bool usingLocalRotation = false;

    private Coroutine currentAction;

    public bool IsIdle => currentAction == null;

    private bool isHoldingBump = false;

    public void UpdateBumpState(bool isKeyPressed, Vector2 movementDirection)
    {
        if (isKeyPressed && !isHoldingBump)
        {
            isHoldingBump = true;
        }
        else if (!isKeyPressed && isHoldingBump)
        {
            isHoldingBump = false;
            TriggerBumper(movementDirection.x);
        }
    }

    private void TriggerBumper(float direction)
    {
        IEnumerator DoBump()
        {
            float endTime = Time.fixedTime + duration;
            HashSet<GameObject> bumpedObjects = new HashSet<GameObject>();

            Debug.Log("Player starting bump attempt", this);

            while (Time.fixedTime < endTime)
            {
                var origin = LerpExtent(0f);
                var target = LerpExtent(Mathf.Sign(direction));
                var deltaPosition = target - origin;
                var ray = new Ray(origin, deltaPosition);

                if (Physics.SphereCast(ray, radius, out var hit, maxDistance: deltaPosition.magnitude, layerMask, QueryTriggerInteraction.Ignore) && !bumpedObjects.Contains(hit.collider.gameObject))
                {
                    var resultForce = Vector3.Project(deltaPosition, hit.normal).normalized;
                    Debug.DrawRay(hit.point, hit.normal, Color.gray, 0.5f);
                    Debug.DrawRay(hit.point, resultForce, Color.magenta, 0.5f);

                    if (ApplyForce(hit.collider, resultForce))
                    {
                        bumpedObjects.Add(hit.collider.gameObject);
                    }
                }

                yield return new WaitForFixedUpdate();
            }

            Debug.Log($"End bump. Hit { bumpedObjects.Count } objects");

            currentAction = null;
        }

        if (currentAction != null || Mathf.Abs(direction) < 0.1f)
            return;

        currentAction = StartCoroutine(DoBump());
    }

    private bool ApplyForce(Collider collider, Vector3 normalizedForce)
    {
        if (collider.TryGetComponent<CMFMovementController>(out var controller))
        {
            controller.AddMomentum(normalizedForce * playerBodyForceScale);
            return true;
        }
        else if (collider.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(normalizedForce * dynamicBodyForceScale, ForceMode.Impulse);
            return true;
        }
        else
        {
            return true;
        }
    }

    private Vector3 LerpExtent(float t)
    {
        var flippedExtent = new Vector3(Mathf.Sign(t) * extent.x, extent.y, extent.z);
        var rotation = usingLocalRotation ? transform.localRotation : transform.rotation;
        return Vector3.Lerp(transform.position, transform.position + rotation * flippedExtent, Mathf.Abs(t));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        var origin = LerpExtent(0f);
        Gizmos.DrawLine(origin, LerpExtent(1f));
        Gizmos.DrawLine(origin, LerpExtent(-1f));

        Gizmos.DrawWireSphere(LerpExtent(interpolation), radius);
    }
}
