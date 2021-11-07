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

        public float quality;

        public SearchState(Vector3 currentPosition, float currentAngle, Collider surface)
        {
            this.stepCount = 0;
            this.currentAngle = currentAngle;
            this.currentPosition = currentPosition;
            this.previousPosition = currentPosition;
            this.surface = surface;
            this.firstInput = null;
            this.quality = 1f;
        }

        public SearchState Next(Vector3 newPosition, float newAngle, Collider collider, VirtualRunnerInput.Input input, float qualityMultiplier = 1f)
        {
            return new SearchState
            {
                stepCount = stepCount + 1,
                currentAngle = newAngle,
                currentPosition = newPosition,
                previousPosition = currentPosition,
                surface = collider,
                firstInput = firstInput == null ? input : firstInput,
                quality = quality * qualityMultiplier,
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

    [SerializeField, Min(1f)]
    private float minImprovementIncrement = 0.5f;

    [SerializeField]
    private float hoverDistance = 0.1f;

    [SerializeField, Min(0f)]
    private float movementSpeedTestMultiplier = 0.95f;

    [SerializeField, Min(0f)]
    private float minSlopeDetectionHeight = 0.5f;

    [Header("Jump Calculations")]

    [SerializeField, Min(0f)]
    private float jumpStepInterval = 0.1f;

    [SerializeField, Min(0)]
    private int jumpStepCount = 20;

    [SerializeField, Min(0f)]
    private float jumpSimplificationTolerance = 1f;

    [SerializeField, Min(0f)]
    private float jumpHeightTestMultiplier = 0.9f;

    [SerializeField]
    private CMFMovementController movementController;

    [Header("Input Parameters")]

    [SerializeField]
    private VirtualRunnerInput temporaryInputHook;

    [SerializeField, Min(0f)]
    private float movementDapening = 0.5f;

    [SerializeField]
    private float maxMovementVelocityChange = 2f;

    private DebugState debugInfo;
    private Vector2 inputDampening;

    // Caches
    private TrajectoryGenerator trajectoryGenerator;

    private void FixedUpdate()
    {
        CalculatePaths();
    }

    private void EnsureInitialized()
    {
        if (trajectoryGenerator == null)
            trajectoryGenerator = new TrajectoryGenerator(this);
    }

    private void CalculatePaths()
    {
        var wasJumping = temporaryInputHook.CurrentInput.isJumping;
        var oldMovementInput = temporaryInputHook.CurrentInput.movementValue;
        var searchState = InternalCalculatePaths();
        if (searchState.firstInput.HasValue)
        {
            Debug.DrawLine(transform.position, searchState.currentPosition, Color.green);
            temporaryInputHook?.UpdateInput((ref VirtualRunnerInput.Input i) =>
            {
                i.movementValue = searchState.firstInput.Value.movementValue;
                i.isJumping = !wasJumping && searchState.firstInput.Value.isJumping;
            });
        }
        else
        {
            temporaryInputHook?.UpdateInput((ref VirtualRunnerInput.Input i) =>
            {
                i.isJumping = false;
            });
        }

        temporaryInputHook?.UpdateInput((ref VirtualRunnerInput.Input i) =>
        {
            i.movementValue = Vector2.SmoothDamp(oldMovementInput, i.movementValue, ref inputDampening, movementDapening, maxMovementVelocityChange, Time.fixedDeltaTime);
        });
    }

    private SearchState InternalCalculatePaths()
    {
        EnsureInitialized();

        debugInfo = default;

        // 1. Find the surface plane
        if (!movementController.Mover.IsGrounded())
            return default;

        var currentSurface = movementController.Mover.GetGroundCollider();
        var surfacePoint = movementController.Mover.GetGroundPoint();
        var surfaceNormal = movementController.Mover.GetGroundNormal();
        var forwardsAngle = Vector3.SignedAngle(transform.forward, Vector3.forward, Vector3.up);

        debugInfo.originSurfacePosition = surfacePoint;

        int iterations = 0;
        Stack<SearchState> ongoingSearches = new Stack<SearchState>(32);

        for (int i = 0; i < extraPaths; i++)
        {
            ongoingSearches.Push(new SearchState(surfacePoint + surfaceNormal * hoverDistance, i * +extraPathsStepAngle, currentSurface));
            ongoingSearches.Push(new SearchState(surfacePoint + surfaceNormal * hoverDistance, i * -extraPathsStepAngle, currentSurface));
        }

        ongoingSearches.Push(new SearchState(surfacePoint + surfaceNormal * hoverDistance, 0f, currentSurface));

        float bestDistance = 0f;
        SearchState bestState = default;

        while (ongoingSearches.Count > 0)
        {
            // Escape hatch to prevent hangs
            if (iterations++ > 10000)
            {
                Debug.LogWarning("Maxed out on iterations during route calculations...", this);
                break;
            }

            var search = ongoingSearches.Pop();
            var currentAngle = search.currentAngle;
            var currentPosition = search.currentPosition;
            var previousPosition = search.previousPosition;
            var stepCount = search.stepCount;

            if (currentPosition.z > surfacePoint.z + maxLookaheadDistance)
            {
                // We've reached as far as we want to search, and this is the path which
                // deviates from the current path the least. Choose it
                return search;
            }
            else if ((currentPosition.z - surfacePoint.z) * search.quality > bestDistance + minImprovementIncrement)
            {
                bestDistance = (currentPosition.z - surfacePoint.z) * search.quality;
                bestState = search;
            }

            var nextAngle = Mathf.LerpAngle(currentAngle, forwardsAngle, Mathf.Clamp01(alignmentRate * stepCount));

            Vector3 stepDirection = transform.rotation * Quaternion.Euler(0f, currentAngle, 0f) * Vector3.forward;
            Vector3 nextPoint = currentPosition + stepDirection * stepSize;

            Debug.DrawLine(currentPosition, nextPoint, stepCount % 2 == 0 ? Color.magenta : Color.cyan);

            // Test for hill contact
            if (Physics.Raycast(currentPosition + transform.up * minSlopeDetectionHeight, stepDirection, out var slopeHit, stepSize + minSlopeDetectionHeight * (1f / minImprovementIncrement), groundLayermask, QueryTriggerInteraction.Ignore))
            {
                var input = new VirtualRunnerInput.Input
                {
                    movementValue = new Vector2(stepDirection.x, stepDirection.z),
                    isJumping = false,
                };

                ongoingSearches.Push(search.Next(slopeHit.point + slopeHit.normal * hoverDistance, nextAngle, slopeHit.collider, input));

                // Don't bother checking for jumps if we're walking uphill
                continue;
            }

            Vector3 down = transform.rotation * Quaternion.Euler(-5f, 0f, 0f) * Vector3.down;
            float groundCheckDistance = hoverDistance * 4;

            // Check for ground in front of us and check if we crest a hill
            bool isMoreGround = Physics.Raycast(nextPoint, down, out var groundHit, groundCheckDistance, groundLayermask, QueryTriggerInteraction.Ignore);
            isMoreGround = isMoreGround || Physics.Raycast(nextPoint + transform.up * minSlopeDetectionHeight, down, out groundHit, minSlopeDetectionHeight, groundLayermask, QueryTriggerInteraction.Ignore);

            Debug.DrawLine(nextPoint, nextPoint + down * groundCheckDistance, isMoreGround ? new Color(0.8f, 0.1f, 0.05f, 0.8f) : Color.gray);

            if (isMoreGround)
            {
                // Still ground in front of us, keep looking forward
                var input = new VirtualRunnerInput.Input
                {
                    movementValue = new Vector2(stepDirection.x, stepDirection.z),
                    isJumping = false,
                };

                ongoingSearches.Push(search.Next(nextPoint, nextAngle, groundHit.collider, input));
            }

            // Calculate some initial velocities for arial paths
            var dropVelocity = new Vector3(0f, 0f, movementController.movementSpeed * movementSpeedTestMultiplier);
            var jumpVelocity = new Vector3(0f, movementController.jumpSpeed * jumpHeightTestMultiplier, movementController.movementSpeed * movementSpeedTestMultiplier);

            if (!isMoreGround)
            {
                // Check if we can drop into this gap
                bool hasDrop = TryComputeAirialPath(nextPoint, nextAngle, dropVelocity, search, out var dropState);

                // Check if we can jump over this gap in front of us
                bool hasJump = TryComputeAirialPath(currentPosition, nextAngle, jumpVelocity, search, out var jumpState);

                if (hasDrop && hasJump && dropState.surface == jumpState.surface)
                {
                    // We can drop onto the same surface we can jump onto. Do the drop instead
                    ongoingSearches.Push(dropState);
                }
                else
                {
                    if (hasDrop)
                        ongoingSearches.Push(dropState);

                    if (hasJump)
                        ongoingSearches.Push(jumpState);
                }
            }
        }

        return bestState;
    }

    private bool TryComputeAirialPath(Vector3 startPosition, float nextAngle, Vector3 relativeVelocity, SearchState currentState, out SearchState resultState)
        => TryComputeAirialPath(startPosition, nextAngle, relativeVelocity, currentState, out resultState, Color.blue);

    private bool TryComputeAirialPath(Vector3 startPosition, float nextAngle, Vector3 relativeVelocity, SearchState currentState, out SearchState resultState, Color color)
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

                var nearestPointOnCurrentSurfaceToLanding = currentState.surface.ClosestPointOnBounds(jumpHit.point);
                var nearestPointOnNewSurfaceToCurrentPosition = jumpHit.collider.ClosestPointOnBounds(transform.position);
                var isSteppableTransition = (Mathf.Abs(nearestPointOnCurrentSurfaceToLanding.z - nearestPointOnNewSurfaceToCurrentPosition.z) < 0.1f) &&
                    (Mathf.Abs(nearestPointOnCurrentSurfaceToLanding.x - nearestPointOnNewSurfaceToCurrentPosition.x) < 0.1f);

                Debug.DrawLine(path[j - 1], jumpHit.point, color);
                Debug.DrawLine(nearestPointOnCurrentSurfaceToLanding, nearestPointOnNewSurfaceToCurrentPosition, Color.yellow);

                if (isMovingDownwards && isSurfaceLandable && isDifferentSurface && !isSteppableTransition)
                {
                    var targetDirection = transform.rotation * Quaternion.Euler(0f, nextAngle, 0f) * Vector3.forward;
                    var input = new VirtualRunnerInput.Input
                    {
                        movementValue = new Vector2(targetDirection.x, targetDirection.z),
                        isJumping = relativeVelocity.y > 0.1f,
                    };

                    resultState = currentState.Next(jumpHit.point + jumpHit.normal * hoverDistance, nextAngle, jumpHit.collider, input, input.isJumping ? 0.9f : 1f);
                    return true;
                }
                else
                {
                    break;
                }
            }
            else
            {
                Debug.DrawLine(path[j - 1], path[j], color);
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
