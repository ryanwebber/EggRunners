using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputBinder))]
public class PlayerInputRouter : MonoBehaviour
{
    [SerializeField]
    private VirtualRunnerInput forwardingInput;

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
            forwardingInput.UpdateInput((ref VirtualRunnerInput.Input i) => {
                i.isJumping = currentSource.IsJumpPressed;
                i.isDashing = currentSource.IsDashPressed;
                i.movementValue = currentSource.MovementValue;
            });
        }
        else
        {
            forwardingInput.ResetInput();
        }
    }
}
