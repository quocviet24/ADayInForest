using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Cutscene_Rac : MonoBehaviour
{
    [Header("GameObject Túi Rác & Điểm Đá Vào Bụi")]
    public GameObject racObject;           // Kéo GameObject túi rác vào (để trống sẽ tự lấy object này)
    public Transform kickTarget;           // Điểm rơi trong bụi cây (nếu để trống sẽ tự văng sang bên phải 2.5m)

    [Header("Hiệu ứng Chụp Ảnh (Lựa chọn B)")]
    public Image flashOverlay;             // Kéo FlashOverlay từ Canvas vào
    public AudioClip shutterClip;          // Kéo file âm thanh chupanh vào

    [Header("Cài đặt Âm thanh Khác")]
    public AudioSource audioSource;        // AudioSource trên object sự kiện
    public AudioClip pickupClip;           // Âm thanh nhặt rác / sột soạt (nếu có, không có để trống)
    public AudioClip kickClip;             // Âm thanh đá / sút bóng / va chạm (hoặc dùng boHuc)

    [Header("Thoại Dẫn Dắt Ban Đầu")]
    [TextArea(2, 4)]
    public string introDialogue = "Ủa kìa, hình như ai đó đi qua đã vô ý vứt lại một túi rác ngay cạnh đường mòn. Bạn tính xử lý sao đây?";

    private Interactable interactableData;
    private bool hasTriggered = false;

    void Start()
    {
        interactableData = GetComponent<Interactable>();

        // Nếu chưa gán racObject thì mặc định chính là GameObject mang script này
        if (racObject == null)
        {
            racObject = gameObject;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlayRacCutscene());
        }
    }

    private IEnumerator PlayRacCutscene()
    {
        // 1. Phanh cứng bước chân người chơi
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.ForceStopAndLook(Vector2.zero);
        }
        yield return new WaitForSeconds(0.2f);

        // 2. Mascot nhảy ra thoại dẫn dắt
        if (DialogueManager.instance != null && interactableData != null)
        {
            DialogueManager.instance.ShowDialogue(
                interactableData.characterName,
                introDialogue,
                interactableData.avatarIcon
            );

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        // 3. Mở bảng 4 lựa chọn xử lý rác
        if (QuestionManager.instance != null && interactableData != null)
        {
            QuestionManager.instance.SetupQuestion(interactableData);
        }
    }

    // ==========================================
    // 4 HÀNH ĐỘNG KHI CHỌN ĐÁP ÁN
    // ==========================================

    /// <summary>
    /// Lựa chọn A (25đ): Bỏ qua không nhặt
    /// </summary>
    public void ChonA_BoQua()
    {
        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn B (50đ): Chụp ảnh nhắc nhở -> Nháy Flash trắng + tiếng máy ảnh
    /// </summary>
    public void ChonB_ChupAnh()
    {
        StartCoroutine(Routine_ChupAnh());
    }

    private IEnumerator Routine_ChupAnh()
    {
        if (audioSource != null && shutterClip != null)
        {
            audioSource.PlayOneShot(shutterClip);
        }

        if (flashOverlay != null)
        {
            flashOverlay.transform.SetAsLastSibling();
            flashOverlay.gameObject.SetActive(true);
            Color c = Color.white;
            c.a = 1f;
            flashOverlay.color = c;

            float fadeTime = 0f;
            while (fadeTime < 0.25f)
            {
                fadeTime += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, fadeTime / 0.25f);
                flashOverlay.color = c;
                yield return null;
            }
            c.a = 0f;
            flashOverlay.color = c;
            flashOverlay.gameObject.SetActive(false);
        }

        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn C (100đ): Nhặt lên -> Túi rác biến mất ngay lập tức
    /// </summary>
    public void ChonC_NhatRac()
    {
        if (audioSource != null && pickupClip != null)
        {
            audioSource.PlayOneShot(pickupClip);
        }

        if (racObject != null)
        {
            racObject.SetActive(false);
        }

        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn D (25đ): Đá túi rác vào sâu trong bụi cây
    /// Túi rác văng một đường parabol vào bụi rậm rồi biến mất
    /// </summary>
    public void ChonD_DaRac()
    {
        StartCoroutine(Routine_DaRacVaoBui());
    }

    private IEnumerator Routine_DaRacVaoBui()
    {
        // 1. Âm thanh đá / sút
        if (audioSource != null && kickClip != null)
        {
            audioSource.PlayOneShot(kickClip);
        }

        // 2. Hiệu ứng túi rác bay đường cong parabol vào lùm cây
        if (racObject != null)
        {
            Vector3 startPos = racObject.transform.position;
            Vector3 endPos = (kickTarget != null) 
                ? kickTarget.position 
                : (startPos + new Vector3(2.2f, 0.6f, 0f)); // Điểm mặc định văng chếch sang phải vào bãi cỏ

            float duration = 0.45f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                Vector3 current = Vector3.Lerp(startPos, endPos, progress);
                current.y += Mathf.Sin(progress * Mathf.PI) * 1.2f; // Bay bổng lên rồi hạ xuống bụi
                racObject.transform.position = current;

                // Xoay tròn túi rác khi bị đá
                racObject.transform.Rotate(0, 0, 480f * Time.deltaTime);

                yield return null;
            }

            racObject.transform.position = endPos;
            yield return new WaitForSeconds(0.15f);

            // Rác khuất tầm mắt trong bụi cây
            racObject.SetActive(false);
        }

        CowAI.UnfreezePlayer();
    }
}