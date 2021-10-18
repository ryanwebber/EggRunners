using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputBinder))]
public class PlayerInputRouter : MonoBehaviour
{
    [SerializeField]
    private PlayerMovementController movementController;

    private PlayerInputBinder inputBinder;
    private IInputSource currentSource;

    private void Awake()
    {
        inputBinder = GetComponent<PlayerInputBinder>();
    }

    private void OnEnable()
    {
        inputBinder.OnAttachToInput += (newSource) =>
        {
            if (currentSource != null)
                DetachFromInput(currentSource);
            if (newSource != null)
                AttachToInput(newSource);

            currentSource = newSource;
        };

        AttachToInput(InputSourceUtils.DetachedSource);
    }

    private void AttachToInput(IInputSource source)
    {
    }

    private void DetachFromInput(IInputSource source)
    {
    }

    private void Update()
    {
        if (currentSource != null)
        {
            movementController.IsJumping = false;
            movementController.MovementInput = currentSource.MovementValue;
        }
        else
        {
            movementController.IsJumping = false;
            movementController.MovementInput = Vector2.zero;
        }
    }
}
