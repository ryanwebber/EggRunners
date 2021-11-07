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
        relayedSource.IsJumpPressed = ctx.ReadValueAsButton();
    }

    public void OnPlayerDashAction(InputSystem.InputAction.CallbackContext ctx)
    {
        relayedSource.IsDashPressed = ctx.ReadValueAsButton();
    }

    private class RelayInputSource : IInputSource
    {
        public bool IsJumpPressed { get; set; } = false;
        public bool IsDashPressed { get; set; } = false;
        public Vector2 MovementValue { get; set; } = Vector2.zero;
        public PlayerIdentifier InputIdentifier { get; set; }

        public void Reset()
        {
            MovementValue = Vector2.zero;
            IsJumpPressed = false;
        }
    }

    private class RelayInputFeedback : IInputFeedback
    {
        public Event OnTriggerHapticFeedback { get; set; }
    }
}
