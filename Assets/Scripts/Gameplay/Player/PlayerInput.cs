using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float VerticalInput { get; private set; }

    private void Update()
    {
        ReadInput();
    }

    private void ReadInput()
    {
        VerticalInput = Input.GetAxisRaw("Vertical");
    }
}