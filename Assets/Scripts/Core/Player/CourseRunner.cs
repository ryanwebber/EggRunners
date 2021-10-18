using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInputBinder))]
public class CourseRunner : MonoBehaviour
{
    public PlayerInputBinder MainInput { get; private set; }

    private void Awake()
    {
        MainInput = GetComponent<PlayerInputBinder>();
    }
}
