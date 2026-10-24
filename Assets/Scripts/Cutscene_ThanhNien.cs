using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Cutscene_ThanhNien : MonoBehaviour
{
    [Header("GameObject & Hình ảnh Thanh Niên")]
    public GameObject thanhNienObject;      // Kéo GameObject thanh niên trên map vào (để ẩn khi biến mất)
    public Sprite thanhNienAvatar;          // Ảnh mặt thanh niên hiện trên khung thoại
    public string thanhNienName = "Thanh niên";

    [Header("Hiệu ứng Màn hình (Canvas)")]
    public Image flashOverlay;             // Kéo FlashOverlay từ Canvas vào
    public Image blackOverlay;             // Kéo BlackOverlay từ Canvas vào

    [Header("Cài đặt Âm thanh")]
    public AudioSource audioSource;        // AudioSource trên SuKien_ThanhNien
    public AudioClip shutterClip;          // Tiếng chụp ảnh
    public AudioClip fightAudioClip;       // Tiếng bụp bụp chát chát / đấm đá

    [Header("Thông tin Người chơi")]
    public Sprite playerAvatar;            // Kéo Avatar người chơi vào
    public string playerName = "Bạn";

    [Header("Thoại Dẫn Dắt Mở Đầu")]
    [TextArea(2, 4)]
    public string introDialogue = "Ủa đằng kia có ai kìa... Hình như cậu ta đang bước hẳn vào giữa thảm hoa để chụp ảnh thì phải? Bạn tính làm gì đây?";

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
            StartCoroutine(PlayThanhNienCutscene());
        }
    }

    private IEnumerator PlayThanhNienCutscene()
    {
        // 1. Phanh cứng bước chân
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.ForceStopAndLook(Vector2.zero);
        }
        yield return new WaitForSeconds(0.2f);

        // 2. Mascot mở đầu thoại
        if (DialogueManager.instance != null && interactableData != null)
        {
            DialogueManager.instance.ShowDialogue(
                interactableData.characterName,
                introDialogue,
                interactableData.avatarIcon
            );

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        // 3. Mở bảng câu hỏi
        if (QuestionManager.instance != null && interactableData != null)
        {
            QuestionManager.instance.SetupQuestion(interactableData);
        }
    }

    // ==========================================
    // CÁC HÀNH ĐỘNG KHI CHỌN ĐÁP ÁN
    // ==========================================

    /// <summary>
    /// Lựa chọn A (30đ): Mặc kệ đi tiếp
    /// </summary>
    public void ChonA_MacKe()
    {
        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn B (10đ): Chụp ảnh -> Thanh niên chửi -> Màn hình đen đấm đá -> Mascot thở dài
    /// </summary>
    public void ChonB_ChupAnh()
    {
        StartCoroutine(Routine_ChupAnhDanhNhau());
    }

    private IEnumerator Routine_ChupAnhDanhNhau()
    {
        // 1. Phát tiếng máy ảnh + Nháy Flash
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

        yield return new WaitForSeconds(0.4f);

        // 2. Thanh niên nổi đóa
        if (DialogueManager.instance != null)
        {
            string angryText = "Thằng chó, mày chụp cái gì đấy?!";
            DialogueManager.instance.ShowDialogue(thanhNienName, angryText, thanhNienAvatar);

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        // 3. Tối đen màn hình + Tiếng đấm đá
        yield return StartCoroutine(Routine_ManHinhDenVaDanhNhau());

        // 4. Mascot thở dài
        if (DialogueManager.instance != null && interactableData != null)
        {
            string mascotSigh = "Haizzzz... Đã bảo chụp lén người ta rất dễ ăn đòn mà! May mà mặt mũi chỉ hơi bầm tím tí thôi, lần sau đừng có manh động như vậy nữa nha!";
            DialogueManager.instance.ShowDialogue(interactableData.characterName, mascotSigh, interactableData.avatarIcon);

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn C (100đ): Khuyên ngăn lịch sự -> Thanh niên xin lỗi
    /// </summary>
    public void ChonC_KhuyenNgan()
    {
        StartCoroutine(Routine_KhuyenNganLichSu());
    }

    private IEnumerator Routine_KhuyenNganLichSu()
    {
        yield return new WaitForSeconds(0.2f);

        // Thanh niên lịch sự nhận lỗi
        if (DialogueManager.instance != null)
        {
            string politeText = "Ơ... Xin lỗi bạn nhiều nha, mải chụp hình nên tôi sơ ý quá. Tôi bước ra đường mòn ngay đây!";
            DialogueManager.instance.ShowDialogue(thanhNienName, politeText, thanhNienAvatar);

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        // Mascot khen ngợi
        if (DialogueManager.instance != null && interactableData != null)
        {
            string mascotPraise = "Ứng xử quá điểm 10! Góp ý văn minh, lịch sự vừa bảo vệ được hoa cỏ vừa tránh được xô xát không đáng có.";
            DialogueManager.instance.ShowDialogue(interactableData.characterName, mascotPraise, interactableData.avatarIcon);

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn D (0đ): Chửi bới thô lỗ -> Đấm nhau màn hình đen -> Mascot cằn nhằn
    /// </summary>
    public void ChonD_ChuiBoi()
    {
        StartCoroutine(Routine_ChuiBoiDanhNhau());
    }

    private IEnumerator Routine_ChuiBoiDanhNhau()
    {
        yield return new WaitForSeconds(0.2f);

        // 1. Người chơi chửi thẳng mặt
        if (DialogueManager.instance != null)
        {
            string playerYell = "Thằng chóa kia, ai cho mày dẫm lên hoa?!";
            DialogueManager.instance.ShowDialogue(playerName, playerYell, playerAvatar);

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        yield return new WaitForSeconds(0.2f);

        // 2. Thanh niên sừng cồ
        if (DialogueManager.instance != null)
        {
            string thanhNienReply = "Mày chửi ai đấy? Thích va chạm à?!";
            DialogueManager.instance.ShowDialogue(thanhNienName, thanhNienReply, thanhNienAvatar);

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        // 3. Tối đen màn hình + Tiếng bụp chát
        yield return StartCoroutine(Routine_ManHinhDenVaDanhNhau());

        // 4. Mascot lắc đầu cằn nhằn
        if (DialogueManager.instance != null && interactableData != null)
        {
            string mascotScold = "Haizzzz... Đi bảo vệ môi trường mà máu giang hồ nổi lên chi vậy bạn? Lời nói chẳng mất tiền mua, nóng giận mất khôn chưa kìa!";
            DialogueManager.instance.ShowDialogue(interactableData.characterName, mascotScold, interactableData.avatarIcon);

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        CowAI.UnfreezePlayer();
    }

    // ==========================================
    // HÀM DÙNG CHUNG: MÀN HÌNH ĐEN & TIẾNG ĐẤM NHAU
    // ==========================================
    private IEnumerator Routine_ManHinhDenVaDanhNhau()
    {
        // 1. Tối dần màn hình
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

        // 2. Phát âm thanh đấm đá bụp bụp chát chát
        if (audioSource != null && fightAudioClip != null)
        {
            audioSource.PlayOneShot(fightAudioClip);
        }

        // Chờ 1.5 giây trong bóng tối để nghe tiếng ẩu đả
        yield return new WaitForSeconds(1.5f);

        // 3. Thanh niên biến mất khỏi hiện trường
        if (thanhNienObject != null)
        {
            thanhNienObject.SetActive(false);
        }

        // 4. Màn hình sáng dần trở lại
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

        yield return new WaitForSeconds(0.3f);
    }
}