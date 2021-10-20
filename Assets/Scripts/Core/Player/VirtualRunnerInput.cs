using UnityEngine;
using System.Collections;

public class VirtualRunnerInput : MonoBehaviour
{
    public struct Input
    {
        public Vector2 movementValue;
        public bool isJumping;
    }

    public delegate void InputUpdate(ref Input input);

    [SerializeField]
    private CMFInputAdapter inputAdapter;

    private bool isInputLocked = false;
    public bool IsInputLocked
    {
        get => isInputLocked;
        set
        {
            isInputLocked = value;
            UpdateInput(null);
        }
    }

    private Input input;
    public Input CurrentInput => input;

    public bool UpdateInput(InputUpdate block)
    {
        var i = CurrentInput;
        if (IsInputLocked)
            i = new Input();
        else if (block != null)
            block.Invoke(ref i);

        input = i;
        inputAdapter.MovementInput = i.movementValue;
        inputAdapter.IsJumping = i.isJumping;

        return true;
    }

    public void ResetInput() { input = new Input(); }
}
