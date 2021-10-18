using UnityEngine;
using System.Collections;

using InputSystem = UnityEngine.InputSystem;

[RequireComponent(typeof(InputSystem.PlayerInput))]
public class AttachableInputSource : MonoBehaviour
{
    private InputSystem.PlayerInput rawInput;
    private RelayInputSource relayedSource;
    private RelayInputFeedback relayedFeedback;

    public IInputSource MainSource => relayedSource;
    public IInputFeedback MainFeedback => relayedFeedback;

    private bool IsJoystick => rawInput.currentControlScheme != InputScemeUtils.KeyboardAndMouseScheme;

    private void Awake()
    {
        rawInput = GetComponent<InputSystem.PlayerInput>();

        relayedSource = new RelayInputSource();
        relayedSource.InputIdentifier = new PlayerIdentifier(rawInput.playerIndex);

        relayedFeedback = new RelayInputFeedback();
        relayedFeedback.OnTriggerHapticFeedback += () =>
        {
            Debug.Log("Haptics!");
            foreach (var device in rawInput.devices)
            {
                if (device is InputSystem.Gamepad gamepad)
                {
                    // TODO: rumble for a sec
                }
            }
        };
    }

    public void OnPlayerMove(InputSystem.InputAction.CallbackContext ctx)
    {
        relayedSource.MovementValue = Vector2.ClampMagnitude(ctx.ReadValue<Vector2>(), 1f);
    }

    public void OnPlayerMovementSpecialAction(InputSystem.InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            relayedSource.OnMovementSpecialAction?.Invoke();
    }

    private class RelayInputSource : IInputSource
    {
        public Event OnMovementSpecialAction { get; set; }
        public Vector2 MovementValue { get; set; } = Vector2.zero;
        public PlayerIdentifier InputIdentifier { get; set; }

        public void Reset()
        {
            MovementValue = Vector2.zero;
        }
    }

    private class RelayInputFeedback : IInputFeedback
    {
        public Event OnTriggerHapticFeedback { get; set; }
    }
}
