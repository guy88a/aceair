using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public float VerticalInput { get; private set; }

    private void Update()
    {
        ReadInput();
    }

    private void ReadInput()
    {
        VerticalInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                VerticalInput += 1f;

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                VerticalInput -= 1f;
        }
    }
}