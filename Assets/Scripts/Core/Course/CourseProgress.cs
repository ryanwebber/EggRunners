using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourseProgress : MonoBehaviour
{
    [SerializeField]
    private GameState gameState;

    [SerializeField]
    private Transform maxProgressMarker;

    [SerializeField]
    private float autoProgressSpeed = 0f;

    private float bestDistance = float.NegativeInfinity;
    private float startProgress = 0f;
    public float Distance => transform.position.z;

    private float internalAutoProgressSpeed = 0f;

    private void Awake()
    {
        startProgress = transform.position.z;
        gameState.Events.OnCourseShouldReset += () =>
        {
            ResetProgress();
            internalAutoProgressSpeed = 0f;
        };

        gameState.Events.OnCountDownEnd += () =>
        {
            StartCoroutine(Coroutines.After(1f, () => internalAutoProgressSpeed = autoProgressSpeed));
        };
    }

    private void Start()
    {
        ResetProgress();
    }

    private void Update()
    {
        RecordProgress(new Vector3(0f, 0f, bestDistance + Time.deltaTime * internalAutoProgressSpeed));
    }

    private void ResetProgress()
    {
        bestDistance = startProgress;
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        var currentPosition = transform.position;
        currentPosition.z = bestDistance;
        transform.position = currentPosition;
    }

    public void RecordProgress(Vector3 position)
    {
        bestDistance = Mathf.Min(Mathf.Max(bestDistance, position.z), maxProgressMarker.position.z);
        UpdateProgress();
    }
}
