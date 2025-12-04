// InputHandler.cs
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public Vector2 GetMoveInput()
    {
        float x = 0f, z = 0f;
        if (Input.GetKey(InputSettings.Keys[Action.MoveForward])) z += 1f;
        if (Input.GetKey(InputSettings.Keys[Action.MoveBackward])) z -= 1f;
        if (Input.GetKey(InputSettings.Keys[Action.MoveRight])) x += 1f;
        if (Input.GetKey(InputSettings.Keys[Action.MoveLeft])) x -= 1f;
        return new Vector2(x, z);
    }

    public Vector2 GetMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * InputSettings.MouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * InputSettings.MouseSensitivity;

        if (InputSettings.InvertY)
            mouseY = -mouseY;

        return new Vector2(mouseX, mouseY);
    }

    public bool IsRunning() => Input.GetKey(InputSettings.Keys[Action.Run]);
    public bool IsJumpPressed() => Input.GetKeyDown(InputSettings.Keys[Action.Jump]);
    public bool IsCrouchPressed() => Input.GetKey(InputSettings.Keys[Action.Crouch]);
    public bool IsInteractPressed() => Input.GetKeyDown(InputSettings.Keys[Action.Interact]);
    public bool IsThrowPressed() => Input.GetKeyDown(InputSettings.Keys[Action.Throw]);
    public bool IsGrabPressed() => Input.GetKeyDown(InputSettings.Keys[Action.Grab]);
    public bool IsInventory1Pressed() => Input.GetKeyDown(InputSettings.Keys[Action.Inventory1]);
    public bool IsInventory2Pressed() => Input.GetKeyDown(InputSettings.Keys[Action.Inventory2]);
    public bool IsInventory3Pressed() => Input.GetKeyDown(InputSettings.Keys[Action.Inventory3]);
    public bool IsInventory4Pressed() => Input.GetKeyDown(InputSettings.Keys[Action.Inventory4]);
    public bool IsListTogglePressed() => Input.GetKeyDown(InputSettings.Keys[Action.ListToggle]);
    public bool IsPausePressed() => Input.GetKeyDown(InputSettings.Keys[Action.Pause]);
}