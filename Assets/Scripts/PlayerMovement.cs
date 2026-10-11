using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public float paddingX = 0.5f;
    public float paddingY = 0.5f;

    public static PlayerMovement instance;

    public bool canMove = true;
    public FloatingJoystick joystick;

    [Header("Âm thanh bước chân")]
    public AudioSource walkAudioSource; // Kéo AudioSource của Player vào đây

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

    void Awake() { instance = this; }

    void Update()
    {
        if (!canMove)
        {
            // Tắt tiếng bước chân khi bị khóa di chuyển (VD: đang xem cắt cảnh)
            if (walkAudioSource != null && walkAudioSource.isPlaying) walkAudioSource.Stop();
            return;
        }
        
        movement = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (joystick != null && joystick.inputVector.sqrMagnitude > 0.01f)
        {
            movement = joystick.inputVector;
        }

        if (movement.sqrMagnitude > 0.01f)
        {
            anim.SetFloat("X", movement.x);
            anim.SetFloat("Y", movement.y);
            
            // Bật tiếng bước chân nếu đang di chuyển mà âm thanh chưa phát
            if (walkAudioSource != null && !walkAudioSource.isPlaying) walkAudioSource.Play();
        }
        else
        {
            // Tắt tiếng bước chân khi nhân vật đứng im
            if (walkAudioSource != null && walkAudioSource.isPlaying) walkAudioSource.Stop();
        }

        anim.SetFloat("Speed", movement.sqrMagnitude);
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero; 
            return;
        }
        
        Vector2 nextPosition = rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime;

        Vector2 minBounds = mainCam.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 maxBounds = mainCam.ViewportToWorldPoint(new Vector2(1, 1));

        nextPosition.x = Mathf.Clamp(nextPosition.x, minBounds.x + paddingX, maxBounds.x - paddingX);
        nextPosition.y = Mathf.Clamp(nextPosition.y, minBounds.y + paddingY, maxBounds.y - paddingY);

        rb.MovePosition(nextPosition);
    }

    public void ForceStopAndLook(Vector2 lookDirection)
    {
        canMove = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero; 

        if (walkAudioSource != null) walkAudioSource.Stop(); // Đảm bảo tắt tiếng khi phanh gấp

        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetFloat("Speed", 0f); 
            if (lookDirection != Vector2.zero) 
            {
                anim.SetFloat("X", lookDirection.x);
                anim.SetFloat("Y", lookDirection.y);
            }
        }
    }
}