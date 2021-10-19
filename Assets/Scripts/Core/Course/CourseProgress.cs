using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourseProgress : MonoBehaviour
{
    [SerializeField]
    private PlayerService playersService;

    private float bestDistance = float.NegativeInfinity;

    public float Distance => transform.position.z;

    private void Start()
    {
        bestDistance = transform.position.z;
    }

    private void Update()
    {
        if (playersService.Runners.Count == 0)
            return;

        foreach (var player in playersService.Runners)
        {
            bestDistance = Mathf.Max(bestDistance, player.transform.position.z);
        }

        var currentPosition = transform.position;
        currentPosition.z = bestDistance;
        transform.position = currentPosition;
    }
}
