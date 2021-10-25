using UnityEngine;
using System.Collections;

public class CMFInputAdapter : CMF.CharacterInput
{
    public Vector2 MovementInput = Vector2.zero;
    public bool IsJumping = false;

    public override float GetHorizontalMovementInput() => MovementInput.x;
    public override float GetVerticalMovementInput() => MovementInput.y;
    public override bool IsJumpKeyPressed() => IsJumping;
}
