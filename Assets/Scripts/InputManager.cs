using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputManager
{
    // Returns the move input as a normalized Vector2
    public static Vector2 GetMoveInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        return new Vector2(horizontalInput, verticalInput).normalized;
    }

    // Returns the mouse input as a Vector2
    public static Vector2 GetMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        return new Vector2(mouseX, mouseY);
    }

    // Returns true if the crouch key is pressed
    public static bool GetCrouchInput()
    {
        return Input.GetKeyDown(KeyCode.LeftControl);
    }

    // Returns true if the specified mouse button is pressed
    public static bool GetInteractInput(int value)
    {
        return Input.GetMouseButtonDown(value);
    }

    // Returns the scroll wheel input value
    public static float GetScrollInput()
    {
        return Input.GetAxis("Mouse ScrollWheel");
    }
}
