using System.Collections;
using UnityEngine;

public class Cutscene_QuaLa : MonoBehaviour
{
    [Header("Cài đặt Âm thanh Bụng Réo")]
    public AudioSource audioSource;        // Kéo AudioSource vào đây
    public AudioClip stomachGrowlClip;     // Kéo file tiếng bụng réo ọc ọc vào đây

    [Header("Thông tin Người chơi (Độc thoại)")]
    public string playerName = "Bạn";      // Tên hiển thị người chơi
    public Sprite playerAvatar;            // Kéo Sprite mặt người chơi vào

    private Interactable interactableData;
    private bool hasTriggered = false; 

    void Start()
    {
        interactableData = GetComponent<Interactable>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlayQuaLaCutscene());
        }
    }

    private IEnumerator PlayQuaLaCutscene()
    {
        // 1. Phanh cứng nhân vật
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.ForceStopAndLook(Vector2.zero);
        }
        yield return new WaitForSeconds(0.2f);

        // 2. Tiếng bụng đói réo lên ọc ọc...
        if (audioSource != null && stomachGrowlClip != null)
        {
            audioSource.PlayOneShot(stomachGrowlClip);
        }
        yield return new WaitForSeconds(0.6f);

        // 3. Người chơi độc thoại than đói
        if (DialogueManager.instance != null)
        {
            string line1 = "Ọc ọc... Đi nãy giờ mệt lả cả người, bụng đói meo mốc rồi...";
            DialogueManager.instance.ShowDialogue(playerName, line1, playerAvatar);

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        yield return new WaitForSeconds(0.2f);

        // 4. Người chơi tự động QUAY MẶT SANG PHẢI nhìn các bụi quả
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.ForceStopAndLook(Vector2.right);
        }
        yield return new WaitForSeconds(0.4f);

        // 5. Người chơi thắc mắc có nên ăn không
        if (DialogueManager.instance != null)
        {
            string line2 = "Ủa, mấy bụi cây bên đường mọc đầy chùm quả mọng đỏ kìa... Trông ngon mắt ghê, không biết có ăn được không nhỉ?";
            DialogueManager.instance.ShowDialogue(playerName, line2, playerAvatar);

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        // 6. Hiển thị bảng 4 lựa chọn
        if (QuestionManager.instance != null && interactableData != null)
        {
            QuestionManager.instance.SetupQuestion(interactableData);
        }
    }

    // ==========================================
    // CÁC HÀNH ĐỘNG KHI CHỌN ĐÁP ÁN
    // ==========================================

    /// <summary>
    /// Lựa chọn A (100đ): Thôi không ăn
    /// </summary>
    public void ChonA_KhongAn()
    {
        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn B (0đ): Ăn thử -> Bụng lại réo ọc ọc đau quặn
    /// </summary>
    public void ChonB_AnThu()
    {
        StartCoroutine(Routine_BungDau());
    }

    private IEnumerator Routine_BungDau()
    {
        yield return new WaitForSeconds(0.3f);
        if (audioSource != null && stomachGrowlClip != null)
        {
            audioSource.PlayOneShot(stomachGrowlClip);
        }
        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn C (80đ): Hỏi Mascot
    /// </summary>
    public void ChonC_HoiMascot()
    {
        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn D (10đ): Quay lại bãi picnic
    /// </summary>
    public void ChonD_QuayLaiPicnic()
    {
        CowAI.UnfreezePlayer();
    }
}