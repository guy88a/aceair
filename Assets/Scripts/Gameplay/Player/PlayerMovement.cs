using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float fixedX;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 12f;

    private PlayerInput playerInput;

    private float currentVelocity;

    public void Initialize(PlayerInput input)
    {
        playerInput = input;
        fixedX = transform.position.x;
    }

    private void Update()
    {
        if (playerInput == null)
            return;

        HandleMovement();
    }

    private void HandleMovement()
    {
        float input = playerInput.VerticalInput;

        float targetSpeed = input * maxSpeed;

        if (Mathf.Abs(input) > 0.01f)
        {
            currentVelocity = Mathf.MoveTowards(
                currentVelocity,
                targetSpeed,
                acceleration * Time.deltaTime
            );
        }
        else
        {
            currentVelocity = Mathf.MoveTowards(
                currentVelocity,
                0f,
                deceleration * Time.deltaTime
            );
        }

        Vector3 newPosition = transform.position;
        newPosition.y += currentVelocity * Time.deltaTime;
        newPosition.x = fixedX;

        transform.position = newPosition;
    }
}