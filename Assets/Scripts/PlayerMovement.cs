using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public float paddingX = 0.5f;
    public float paddingY = 0.5f;

    public FloatingJoystick joystick;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator anim;
    private Camera mainCam;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        mainCam = Camera.main;
    }

    void Update()
    {
        movement = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (joystick != null &&
            joystick.inputVector.sqrMagnitude > 0.01f)
        {
            movement = joystick.inputVector;
        }

        if (movement.sqrMagnitude > 0.01f)
        {
            anim.SetFloat("X", movement.x);
            anim.SetFloat("Y", movement.y);
        }

        anim.SetFloat("Speed", movement.sqrMagnitude);
    }

    void FixedUpdate()
    {
        Vector2 nextPosition =
            rb.position +
            movement.normalized *
            moveSpeed *
            Time.fixedDeltaTime;

        Vector2 minBounds =
            mainCam.ViewportToWorldPoint(new Vector2(0, 0));

        Vector2 maxBounds =
            mainCam.ViewportToWorldPoint(new Vector2(1, 1));

        nextPosition.x = Mathf.Clamp(
            nextPosition.x,
            minBounds.x + paddingX,
            maxBounds.x - paddingX
        );

        nextPosition.y = Mathf.Clamp(
            nextPosition.y,
            minBounds.y + paddingY,
            maxBounds.y - paddingY
        );

        rb.MovePosition(nextPosition);
    }
}