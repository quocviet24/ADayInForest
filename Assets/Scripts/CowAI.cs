using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class CowAI : MonoBehaviour
{
    public static CowAI instance;

    public enum CowState { Idle, Wandering, Chasing, Returning }
    [Header("Trạng thái hiện tại")]
    public CowState currentState = CowState.Idle;

    [Header("Cài đặt Di chuyển")]
    public float walkSpeed = 1.2f;
    public float wanderRadius = 2.0f;
    public float minIdleTime = 2f;
    public float maxIdleTime = 4f;

    [Header("Cài đặt Rượt Đuổi")]
    public float chaseSpeed = 4.3f;        // Tốc độ bò rượt
    public Transform bridgeSafeTarget;     // Kéo DiemQuaCauAnToan vào đây

    [Header("Tọa độ Rơi Xuống Suối & Hồi Sinh")]
    public Transform riverTarget;          // DiemDuoiSong
    public Transform safeLandTarget;       // DiemTrenBo

    [Header("Màn hình Tối")]
    public Image blackOverlay;             // BlackOverlay ở Canvas

    [Header("Âm thanh Con Bò")]
    public AudioSource audioSource;        // AudioSource trên con bò
    public AudioClip hitAudioClip;         // File âm thanh boHuc

    [Header("Thoại Mascot khi hồi sinh")]
    public string mascotName = "Người đồng hành";
    public Sprite mascotAvatar;

    private Animator animator;
    private Vector3 startPosition;
    private Vector3 currentDestination;
    private float stateTimer;
    private string currentPlayingAnim = "";
    private bool isChasingActive = false;
    private bool isHandlingHit = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        stateTimer = Random.Range(minIdleTime, maxIdleTime);

        if (blackOverlay != null)
        {
            Color c = blackOverlay.color;
            c.a = 0f;
            blackOverlay.color = c;
            blackOverlay.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isHandlingHit) return;

        switch (currentState)
        {
            case CowState.Idle:
                UpdateIdle();
                break;
            case CowState.Wandering:
                UpdateWandering();
                break;
            case CowState.Chasing:
                UpdateChasing();
                break;
            case CowState.Returning:
                UpdateReturning();
                break;
        }
    }

    void UpdateIdle()
    {
        animator.speed = 0f;
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            Vector2 randomPoint = (Vector2)startPosition + Random.insideUnitCircle * wanderRadius;
            currentDestination = new Vector3(randomPoint.x, randomPoint.y, transform.position.z);
            currentState = CowState.Wandering;
        }
    }

    void UpdateWandering()
    {
        animator.speed = 1f;
        Vector3 moveDir = (currentDestination - transform.position).normalized;
        PlayDirectionAnimation(moveDir);

        transform.position = Vector3.MoveTowards(transform.position, currentDestination, walkSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentDestination) < 0.1f)
        {
            stateTimer = Random.Range(minIdleTime, maxIdleTime);
            currentState = CowState.Idle;
        }
    }

    void UpdateChasing()
    {
        if (!isChasingActive || PlayerMovement.instance == null) return;

        Transform playerTransform = PlayerMovement.instance.transform;

        // Thoát nạn khi chạy qua cầu
        if (bridgeSafeTarget != null && playerTransform.position.x >= bridgeSafeTarget.position.x)
        {
            isChasingActive = false;
            currentState = CowState.Returning;
            return;
        }

        // Rượt theo Player
        animator.speed = 1.4f;
        Vector3 chaseDir = (playerTransform.position - transform.position).normalized;
        PlayDirectionAnimation(chaseDir);

        transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, chaseSpeed * Time.deltaTime);
    }

    // ==============================================================
    // BẮT VA CHẠM VẬT LÝ: CHỈ CẦN CHẠM VÀO LÀ HÚC BAY NGAY LẬP TỨC
    // ==============================================================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isChasingActive && !isHandlingHit && collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(Routine_HucVangPlayer());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isChasingActive && !isHandlingHit && collision.CompareTag("Player"))
        {
            StartCoroutine(Routine_HucVangPlayer());
        }
    }

    void UpdateReturning()
    {
        animator.speed = 1f;
        Vector3 returnDir = (startPosition - transform.position).normalized;
        PlayDirectionAnimation(returnDir);

        transform.position = Vector3.MoveTowards(transform.position, startPosition, walkSpeed * 1.5f * Time.deltaTime);

        if (Vector3.Distance(transform.position, startPosition) < 0.1f)
        {
            stateTimer = Random.Range(minIdleTime, maxIdleTime);
            currentState = CowState.Idle;
        }
    }

    void PlayDirectionAnimation(Vector2 dir)
    {
        string targetAnim = "";
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            targetAnim = (dir.x > 0) ? "Cow_WalkRight" : "Cow_WalkLeft";
        }
        else
        {
            targetAnim = (dir.y > 0) ? "Cow_WalkUp" : "Cow_WalkDown";
        }

        if (currentPlayingAnim != targetAnim)
        {
            currentPlayingAnim = targetAnim;
            animator.Play(targetAnim);
        }
    }

    public void StartChasing()
    {
        isChasingActive = true;
        currentState = CowState.Chasing;
    }

    private IEnumerator Routine_HucVangPlayer()
    {
        isHandlingHit = true;
        isChasingActive = false;
        currentState = CowState.Idle;
        animator.speed = 0f;

        // Phanh nhân vật
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.ForceStopAndLook(Vector2.zero);
        }

        // Tiếng bò húc
        if (audioSource != null && hitAudioClip != null)
        {
            audioSource.PlayOneShot(hitAudioClip);
        }

        // Bay hình cầu vồng xuống suối
        Transform player = PlayerMovement.instance != null ? PlayerMovement.instance.transform : null;
        if (player != null && riverTarget != null)
        {
            Vector3 startP = player.position;
            Vector3 endP = riverTarget.position;
            float elapsed = 0f;
            float flyDuration = 0.45f;

            while (elapsed < flyDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / flyDuration;
                Vector3 current = Vector3.Lerp(startP, endP, progress);
                current.y += Mathf.Sin(progress * Mathf.PI) * 1.8f;
                player.position = current;
                yield return null;
            }
            player.position = endP;
        }

        yield return new WaitForSeconds(0.2f);

        // Màn hình tối sầm
        if (blackOverlay != null)
        {
            blackOverlay.transform.SetAsLastSibling();
            blackOverlay.gameObject.SetActive(true);
            float t = 0f;
            Color c = blackOverlay.color;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, t / 0.35f);
                blackOverlay.color = c;
                yield return null;
            }
            c.a = 1f;
            blackOverlay.color = c;
        }

        yield return new WaitForSeconds(1.0f);

        // Đưa người chơi lên bờ và trả bò về vị trí ban đầu
        if (player != null && safeLandTarget != null)
        {
            player.position = safeLandTarget.position;
        }
        transform.position = startPosition;

        // Màn hình sáng dần lại
        if (blackOverlay != null)
        {
            float t = 0f;
            Color c = blackOverlay.color;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, t / 0.4f);
                blackOverlay.color = c;
                yield return null;
            }
            c.a = 0f;
            blackOverlay.color = c;
            blackOverlay.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.2f);

        // Mascot nhắc nhở
        if (DialogueManager.instance != null && DialogueManager.instance.dialoguePanel != null)
        {
            string scoldText = "Đã bảo đừng có nghịch dại rồi mà không nghe! :)) May mà suối mùa này nước cạn đó, ướt như chuột lột rồi kìa!";
            DialogueManager.instance.ShowDialogue(mascotName, scoldText, mascotAvatar);

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        UnfreezePlayer();
        isHandlingHit = false;
        currentState = CowState.Idle;
    }

    public static void UnfreezePlayer()
    {
        if (PlayerMovement.instance == null) return;
        PlayerMovement.instance.enabled = true;

        System.Type type = PlayerMovement.instance.GetType();
        string[] boolTrue = { "canMove", "isCanMove", "allowMove", "enableMove" };
        foreach (var name in boolTrue)
        {
            var f = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(bool)) f.SetValue(PlayerMovement.instance, true);

            var p = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(bool) && p.CanWrite) p.SetValue(PlayerMovement.instance, true);
        }

        string[] boolFalse = { "isLocked", "isFreeze", "isFrozen", "freeze" };
        foreach (var name in boolFalse)
        {
            var f = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(bool)) f.SetValue(PlayerMovement.instance, false);

            var p = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(bool) && p.CanWrite) p.SetValue(PlayerMovement.instance, false);
        }

        Rigidbody2D rb = PlayerMovement.instance.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = true;
        }
    }
}