using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [SerializeField]
    private float tmpMovementSpeed;

    public Vector2 MovementInput;
    public bool IsJumping;

    private void Update()
    {
        transform.Translate(Vector3.forward * tmpMovementSpeed * Time.deltaTime * MovementInput.y);
    }
}
