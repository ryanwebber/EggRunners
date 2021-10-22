using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourseProgress : MonoBehaviour
{
    [SerializeField]
    private GameState gameState;

    private float bestDistance = float.NegativeInfinity;
    private float startProgress = 0f;
    public float Distance => transform.position.z;

    private void Awake()
    {
        startProgress = transform.position.z;
        gameState.Events.OnCourseShouldReset += () => ResetProgress();
    }

    private void Start()
    {
        ResetProgress();
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
        bestDistance = Mathf.Max(bestDistance, position.z);
        UpdateProgress();
    }
}
