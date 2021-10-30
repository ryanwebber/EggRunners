using System;
using System.Collections.Generic;
using System.Diagnostics;
using Extensions;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CoursePathFinder: MonoBehaviour
{
    [System.Serializable]
    private struct DebugState
    {
        public Nullable<Vector3> originSurfacePosition;

        public void DrawGizmos()
        {
            if (originSurfacePosition.TryGetValue(out var pos))
                Gizmos.DrawWireSphere(pos, 0.15f);
        }
    }

    private struct SearchState
    {
        public int stepCount;
        public float currentAngle;
        public Vector3 currentPosition;
        public Vector3 previousPosition;
        public Collider surface;
        public Nullable<VirtualRunnerInput.Input> firstInput;

        public SearchState(Vector3 currentPosition, float currentAngle, Collider surface)
        {
            this.stepCount = 0;
            this.currentAngle = currentAngle;
            this.currentPosition = currentPosition;
            this.previousPosition = currentPosition;
            this.surface = surface;
            this.firstInput = null;
        }

        public SearchState Next(Vector3 newPosition, float newAngle, Collider collider, VirtualRunnerInput.Input input)
        {
            return new SearchState
            {
                stepCount = stepCount + 1,
                currentAngle = newAngle,
                currentPosition = newPosition,
                previousPosition = currentPosition,
                surface = collider,
                firstInput = firstInput == null ? input : firstInput,
            };
        }
    }

    private class TrajectoryGenerator
    {
        private List<Vector3> allPoints;
        private List<Vector3> simplifiedPoints;

        public IReadOnlyList<Vector3> AllPoints => allPoints;
        public IReadOnlyList<Vector3> SimplifiedPoints => simplifiedPoints;

        private CoursePathFinder pathFinder;

        public TrajectoryGenerator(CoursePathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
            this.allPoints = new List<Vector3>();
            this.simplifiedPoints = new List<Vector3>();
        }

        public IReadOnlyList<Vector3> RecalculateTrajectory(Vector3 position, float angle, Vector3 relativeVelocity)
        {
            allPoints.Clear();
            simplifiedPoints.Clear();

            allPoints.Add(position);

            Vector3 previousPoint = position;
            Vector3 velocity = pathFinder.transform.rotation * Quaternion.Euler(0f, angle, 0f) * relativeVelocity;
            for (int i = 0; i < pathFinder.jumpStepCount; i++)
            {
                Vector3 nextPoint = previousPoint + velocity * pathFinder.jumpStepInterval;

                allPoints.Add(nextPoint);

                velocity.y -= pathFinder.movementController.gravity * pathFinder.jumpStepInterval;
                previousPoint = nextPoint;
            }

            LineUtility.Simplify(allPoints, pathFinder.jumpSimplificationTolerance, simplifiedPoints);
            return simplifiedPoints;
        }
    }

    [SerializeField]
    private LayerMask groundLayermask;

    [SerializeField, Range(0f, 1f)]
    private float alignmentRate = 0.1f;

    [SerializeField, Min(0f)]
    private float stepSize = 1f;

    [SerializeField, Min(0)]
    private float maxLookaheadDistance = 12f;

    [SerializeField, Min(0)]
    private int extraPaths = 4;

    [SerializeField, Min(0f)]
    private float extraPathsStepAngle = 10;

    [SerializeField, Range(0, 180f)]
    private float maxValidStartAngle = 90f;

    [SerializeField]
    private float hoverDistance = 0.1f;

    [Header("Jump Calculations")]

    [SerializeField, Min(0f)]
    private float jumpStepInterval = 0.1f;

    [SerializeField, Min(0)]
    private int jumpStepCount = 20;

    [SerializeField, Min(0f)]
    private float jumpSimplificationTolerance = 1f;

    [SerializeField]
    private CMFMovementController movementController;

    [SerializeField]
    private VirtualRunnerInput temporaryInputHook;

    private DebugState debugInfo;

    // Caches
    private TrajectoryGenerator trajectoryGenerator;

    private void FixedUpdate()
    {
        bool shouldTurnOffJump = temporaryInputHook.CurrentInput.isJumping;
        CalculatePaths();
        if (shouldTurnOffJump)
            temporaryInputHook?.UpdateInput((ref VirtualRunnerInput.Input i) => i.isJumping = false);
    }

    private void EnsureInitialized()
    {
        if (trajectoryGenerator == null)
            trajectoryGenerator = new TrajectoryGenerator(this);
    }

    private void CalculatePaths()
    {
        var firstInput = InternalCalculatePaths();
        if (firstInput.HasValue)
            temporaryInputHook?.UpdateInput((ref VirtualRunnerInput.Input i) => i = firstInput.Value);
        else
            temporaryInputHook.ResetInput();
    }

    private Nullable<VirtualRunnerInput.Input> InternalCalculatePaths()
    {
        EnsureInitialized();

        debugInfo = default;

        // 1. Find the surface plane
        if (!Physics.SphereCast(transform.position, 0.075f, -transform.up, out var hit, 1f, groundLayermask, QueryTriggerInteraction.Ignore))
            return null;

        var surfaceNormal = hit.normal;
        var surfacePoint = hit.point;
        var forwardsAngle = Vector3.SignedAngle(transform.forward, Vector3.forward, Vector3.up);

        debugInfo.originSurfacePosition = surfacePoint;

        int iterations = 0;
        Stack<SearchState> ongoingSearches = new Stack<SearchState>(32);

        for (int i = 0; i < extraPaths; i++)
        {
            ongoingSearches.Push(new SearchState(surfacePoint + surfaceNormal * hoverDistance, i * +extraPathsStepAngle, hit.collider));
            ongoingSearches.Push(new SearchState(surfacePoint + surfaceNormal * hoverDistance, i * -extraPathsStepAngle, hit.collider));
        }

        ongoingSearches.Push(new SearchState(surfacePoint + surfaceNormal * hoverDistance, 0f, hit.collider));

        float bestDistance = 0f;
        SearchState bestState = default;

        while (ongoingSearches.Count > 0)
        {
            // Escape hatch to prevent hangs
            if (iterations++ > 10000)
                break;

            var search = ongoingSearches.Pop();
            var currentAngle = search.currentAngle;
            var currentPosition = search.currentPosition;
            var previousPosition = search.previousPosition;
            var stepCount = search.stepCount;

            if (currentPosition.z > surfacePoint.z + maxLookaheadDistance)
            {
                // We've reached as far as we want to search, and this is the path which
                // deviates from the current path the least. Choose it
                Debug.DrawLine(surfacePoint, currentPosition, Color.black);
                return search.firstInput;
            }
            else if (currentPosition.z - surfacePoint.z > bestDistance)
            {
                bestDistance = currentPosition.z - surfacePoint.z;
                bestState = search;
            }

            var nextAngle = Mathf.LerpAngle(currentAngle, forwardsAngle, Mathf.Clamp01(alignmentRate * stepCount));

            Vector3 stepDirection = transform.rotation * Quaternion.Euler(0f, currentAngle, 0f) * Vector3.forward;
            Vector3 nextPoint = currentPosition + stepDirection * stepSize;

            Debug.DrawLine(currentPosition, nextPoint, stepCount % 2 == 0 ? Color.magenta : Color.green);
            // TODO: Handle slopes

            if (Physics.Raycast(nextPoint, -transform.up, out var groundHit, 1f, groundLayermask, QueryTriggerInteraction.Ignore))
            {
                // Still ground in front of us, keep looking forward
                var input = new VirtualRunnerInput.Input
                {
                    movementValue = new Vector2(stepDirection.x, stepDirection.z),
                    isJumping = false,
                };

                ongoingSearches.Push(search.Next(nextPoint, nextAngle, groundHit.collider, input));
            }
            else
            {
                // Check if we can jump over this gap in front of us
                var jumpVelocity = new Vector3(0f, movementController.jumpSpeed, movementController.movementSpeed);
                if (TryComputeAirialPath(previousPosition, nextAngle, jumpVelocity, search, out var jumpState))
                    ongoingSearches.Push(jumpState);

                // Check if we can drop into this gap
                var dropVelocity = new Vector3(0f, 0f, movementController.movementSpeed);
                if (TryComputeAirialPath(currentPosition, nextAngle, dropVelocity, search, out var dropState))
                    ongoingSearches.Push(dropState);
            }
        }

        return bestState.firstInput;
    }

    private bool TryComputeAirialPath(Vector3 startPosition, float nextAngle, Vector3 relativeVelocity, SearchState currentState, out SearchState resultState)
    {
        var path = trajectoryGenerator.RecalculateTrajectory(startPosition, currentState.currentAngle, relativeVelocity);
        for (int j = 1; j < path.Count; j++)
        {
            var origin = path[j - 1];
            var deltaPosition = path[j] - origin;
            var jumpRay = new Ray(origin, deltaPosition);

            if (Physics.Raycast(jumpRay, out var jumpHit, deltaPosition.magnitude, groundLayermask, QueryTriggerInteraction.Ignore))
            {
                // We hit something while jumping
                bool isMovingDownwards = Vector3.Dot(-transform.up, deltaPosition) > 0f;
                bool isSurfaceLandable = Vector3.Dot(jumpHit.normal, transform.up) > 0.6f;
                bool isDifferentSurface = jumpHit.collider != currentState.surface;

                Debug.DrawLine(path[j - 1], jumpHit.point, Color.blue);

                if (isMovingDownwards && isSurfaceLandable && isDifferentSurface)
                {
                    var movementInput = currentState.firstInput == null ? Vector2.up : currentState.firstInput.Value.movementValue;
                    var input = new VirtualRunnerInput.Input
                    {
                        movementValue = movementInput,
                        isJumping = relativeVelocity.y > 0.1f,
                    };

                    resultState = currentState.Next(jumpHit.point + jumpHit.normal * hoverDistance, nextAngle, jumpHit.collider, input);
                    return true;
                }
                else
                {
                    break;
                }
            }
            else
            {
                Debug.DrawLine(path[j - 1], path[j], Color.blue);
            }
        }

        resultState = default;
        return false;
    }

    private void OnDrawGizmos()
    {
        CalculatePaths();

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.25f);

        debugInfo.DrawGizmos();
    }
}
