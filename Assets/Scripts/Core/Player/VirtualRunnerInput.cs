using UnityEngine;
using System.Collections;

public class VirtualRunnerInput : MonoBehaviour
{
    [System.Serializable]
    public struct Input
    {
        public Vector2 movementValue;
        public bool isJumping;
        public bool isDashing;

        public override string ToString()
        {
            return $"Input(movementInput={movementValue}, isJumping={isJumping}, isDashing={isDashing})";
        }
    }

    public delegate void InputUpdate(ref Input input);

    [SerializeField]
    private CMFInputAdapter inputAdapter;

    [SerializeField]
    private PlayerBumper bumpController;

    public bool IsInputLocked { get; set; }

    [ReadOnly]
    [SerializeField]
    private Input input;
    public Input CurrentInput => input;

    public bool UpdateInput(InputUpdate block)
    {
        if (IsInputLocked)
            return false;

        ForceUpdateInput(block);
        
        return true;
    }

    public void ForceUpdateInput(InputUpdate block)
    {
        block?.Invoke(ref input);
        inputAdapter.MovementInput = input.movementValue;
        inputAdapter.IsJumping = input.isJumping;
        bumpController.UpdateBumpState(input.isDashing, input.movementValue);
    }

    public void ResetInputAndLock()
    {
        ResetInput();
        IsInputLocked = true;
    }

    public void ResetInput() {
        ForceUpdateInput((ref Input i) => i = default);
    }
}
