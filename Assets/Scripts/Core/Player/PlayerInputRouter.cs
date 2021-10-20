using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputBinder))]
public class PlayerInputRouter : MonoBehaviour
{
    [SerializeField]
    private PlayerCMFInputAdapter mainMovementInput;

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
            mainMovementInput.IsJumping = currentSource.IsJumpPressed;
            mainMovementInput.MovementInput = currentSource.MovementValue;
        }
        else
        {
            mainMovementInput.IsJumping = false;
            mainMovementInput.MovementInput = Vector2.zero;
        }
    }
}
