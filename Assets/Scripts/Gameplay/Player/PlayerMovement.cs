using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 12f;

    private PlayerInput playerInput;
    private float currentVelocity;

    public void Initialize(PlayerInput input)
    {
        playerInput = input;
    }

    private void Update()
    {
        if (playerInput == null)
            return;

        float input = playerInput.VerticalInput;
        float targetVelocity = input * maxSpeed;

        float speedChange = (Mathf.Abs(input) > 0.01f ? acceleration : deceleration) * Time.deltaTime;
        currentVelocity = Mathf.MoveTowards(currentVelocity, targetVelocity, speedChange);

        if (Mathf.Abs(currentVelocity) < 0.01f)
            currentVelocity = 0f;

        transform.position += Vector3.up * currentVelocity * Time.deltaTime;
    }
}