using System.Collections;
using UnityEngine;

public class Cutscene_Picnic : MonoBehaviour
{
    [Header("Vật phẩm trên thảm (Dành cho Lựa Chọn C)")]
    public GameObject milkBoxObject;        // Kéo Sprite hộp sữa trên thảm vào đây (để ẩn đi khi bị trộm)
    public AudioSource drinkAudioSource;    // Âm thanh uống nước / nuốt (nếu có, không có để trống)

    [Header("Thoại Dẫn Dắt Ban Đầu")]
    [TextArea(2, 4)]
    public string introDialogue = "Ồ, một góc dã ngoại được bày biện trông chill ghê! Nhưng lạ thật, chung quanh chẳng thấy bóng dáng chủ nhân đâu cả. Bạn tính làm gì đây?";

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
            StartCoroutine(PlayPicnicCutscene());
        }
    }

    private IEnumerator PlayPicnicCutscene()
    {
        // 1. Phanh nhân vật lại
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.ForceStopAndLook(Vector2.zero);
        }
        yield return new WaitForSeconds(0.2f);

        // 2. Mascot tự động hiện thoại dẫn dắt
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
    /// Lựa chọn A (100đ): Tôn trọng, đi tiếp
    /// </summary>
    public void ChonA_DiTiep()
    {
        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn B (50đ): Ngó nghiêng tò mò
    /// </summary>
    public void ChonB_NgoNghieng()
    {
        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn C (0đ): Trộm hộp sữa
    /// </summary>
    public void ChonC_TromSua()
    {
        if (drinkAudioSource != null)
        {
            drinkAudioSource.Play();
        }

        if (milkBoxObject != null)
        {
            milkBoxObject.SetActive(false); // Ẩn hộp sữa
        }

        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn D (80đ): Quan sát xem có ai cần giúp đỡ rồi đi tiếp
    /// </summary>
    public void ChonD_QuanSatGiupDo()
    {
        CowAI.UnfreezePlayer();
    }
}