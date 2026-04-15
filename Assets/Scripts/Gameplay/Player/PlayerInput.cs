using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public float VerticalInput { get; private set; }

    [SerializeField] private float inputResponse = 8f;

    private float rawVerticalInput;

    private void Update()
    {
        ReadInput();

        VerticalInput = Mathf.MoveTowards(
            VerticalInput,
            rawVerticalInput,
            inputResponse * Time.deltaTime
        );
    }

    private void ReadInput()
    {
        rawVerticalInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                rawVerticalInput += 1f;

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                rawVerticalInput -= 1f;
        }
    }
}