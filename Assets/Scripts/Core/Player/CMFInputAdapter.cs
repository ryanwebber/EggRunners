using UnityEngine;
using System.Collections;

public class CMFInputAdapter : CMF.CharacterInput
{
    public Vector2 MovementInput;
    public bool IsJumping;

    public override float GetHorizontalMovementInput() => MovementInput.x;
    public override float GetVerticalMovementInput() => MovementInput.y;
    public override bool IsJumpKeyPressed() => IsJumping;
}
