using UnityEngine;
using System.Collections;

public class CourseProgressDolly : MonoBehaviour
{
    [SerializeField]
    private CourseProgress progress;

    private float offset = 0f;

    private void Start()
    {
        offset = transform.position.z - progress.Distance;
    }

    private void Update()
    {
        var trackerPosition = transform.position;
        trackerPosition.z = progress.Distance + offset;
        transform.position = trackerPosition;
    }
}
