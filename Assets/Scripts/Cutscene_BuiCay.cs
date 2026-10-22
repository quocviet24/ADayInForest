using System.Collections;
using UnityEngine;

public class Cutscene_BuiCay : MonoBehaviour
{
    [Header("Cài đặt Âm thanh")]
    public AudioSource rustleAudioSource;   // Kéo AudioSource chứa tiếng xào xạc vào đây
    public AudioSource squirrelAudioSource; // Kéo AudioSource chứa tiếng sóc vào đây

    [Header("Hiệu ứng Bụi cây & Chú sóc")]
    public Transform bushTransform;         // Kéo bụi cây rung lắc vào đây
    public GameObject squirrelObject;       // Sprite chú sóc thò ra (mặc định ẩn)

    [Header("Thoại Dẫn Dắt (Mascot)")]
    [TextArea(2, 4)]
    public string introDialogue = "Khoan đã... Bạn có nghe thấy tiếng sột soạt gì trong bụi rậm đằng kia không? Hình như có sinh vật nào đó đang ở rất gần!";

    private Interactable interactableData;
    private bool hasTriggered = false;

    void Start()
    {
        interactableData = GetComponent<Interactable>();

        // Ẩn chú sóc lúc đầu nếu có gán vào
        if (squirrelObject != null)
        {
            squirrelObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi người chơi chạm vào vạch trigger cạnh biển báo
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlayBuiCayCutscene());
        }
    }

    private IEnumerator PlayBuiCayCutscene()
    {
        // 1. Trigger lần 1: Kêu xào xạc lần 1 (Người chơi vẫn đi lại bình thường)
        PlayRustle();

        // 2. Chờ 1.5 giây (Người chơi vẫn tự do di chuyển)
        yield return new WaitForSeconds(1.5f);

        // 3. Kêu xào xạc lần 2
        PlayRustle();
        yield return new WaitForSeconds(0.4f);

        // 4. Lập tức phanh gấp nhân vật lại
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.ForceStopAndLook(Vector2.zero);
        }

        // 5. Hiển thị bảng trò chuyện của Mascot
        if (DialogueManager.instance != null && interactableData != null)
        {
            DialogueManager.instance.ShowDialogue(
                interactableData.characterName,
                introDialogue,
                interactableData.avatarIcon
            );

            // Chờ người chơi đọc và bấm chuột đóng thoại
            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        // 6. Ngay sau đó 1 giây: Lại phát tiếng xào xạc lần 3
        yield return new WaitForSeconds(1f);
        PlayRustle();

        // 7. Sau đó 1 giây: Bật bảng câu hỏi để người chơi chọn
        yield return new WaitForSeconds(1f);
        if (QuestionManager.instance != null && interactableData != null)
        {
            QuestionManager.instance.SetupQuestion(interactableData);
        }
    }

    // Hàm phát âm thanh xào xạc kèm hiệu ứng rung lắc nhẹ bụi cây
    private void PlayRustle()
    {
        if (rustleAudioSource != null)
        {
            rustleAudioSource.Play();
        }

        if (bushTransform != null)
        {
            StartCoroutine(ShakeBush(0.35f, 0.08f));
        }
    }

    // Hiệu ứng rung lắc bụi cây bằng code (không cần vẽ animation)
    private IEnumerator ShakeBush(float duration, float magnitude)
    {
        Vector3 originalPos = bushTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * (magnitude * 0.4f);

            bushTransform.localPosition = new Vector3(originalPos.x + offsetX, originalPos.y + offsetY, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        bushTransform.localPosition = originalPos;
    }

    // ==========================================
    // CÁC HÀNH ĐỘNG KHI CHỌN ĐÁP ÁN (A, B, C, D)
    // ==========================================

    /// <summary>
    /// Đáp án A (100đ): Quan sát từ xa -> Tiếng sóc kêu chít chít và chú sóc xuất hiện nhảy lên cây
    /// </summary>
    public void ChonA_QuanSat()
    {
        StartCoroutine(Routine_SocXuatHien());
    }

    private IEnumerator Routine_SocXuatHien()
    {
        if (squirrelAudioSource != null)
        {
            squirrelAudioSource.Play();
        }

        if (squirrelObject != null)
        {
            squirrelObject.SetActive(true);
            Vector3 startPos = squirrelObject.transform.position;
            Vector3 targetPos = startPos + new Vector3(0.3f, 1.2f, 0f);

            float time = 0f;
            while (time < 1f)
            {
                time += Time.deltaTime * 2.2f;
                squirrelObject.transform.position = Vector3.Lerp(startPos, targetPos, time);
                yield return null;
            }

            yield return new WaitForSeconds(1f);
            squirrelObject.SetActive(false);
        }
    }

    /// <summary>
    /// Đáp án B (50đ): Gõ gậy xuống đất xua đuổi -> Tiếng động tắt lịm
    /// </summary>
    public void ChonB_GoGay()
    {
        if (rustleAudioSource != null && rustleAudioSource.isPlaying)
        {
            rustleAudioSource.Stop();
        }
    }

    /// <summary>
    /// Đáp án C (25đ): Vạch lá xem -> Bụi cây rung giật mạnh cảnh cáo
    /// </summary>
    public void ChonC_VachLa()
    {
        if (rustleAudioSource != null)
        {
            rustleAudioSource.Play();
        }
        if (bushTransform != null)
        {
            StartCoroutine(ShakeBush(0.7f, 0.16f));
        }
    }

    /// <summary>
    /// Đáp án D (0đ): Bỏ chạy -> Bụi cây im bặt
    /// </summary>
    public void ChonD_BoChay()
    {
        if (rustleAudioSource != null && rustleAudioSource.isPlaying)
        {
            rustleAudioSource.Stop();
        }
    }
}