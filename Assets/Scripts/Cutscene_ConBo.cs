using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Cutscene_ConBo : MonoBehaviour
{
    [Header("Hiệu ứng Chụp Ảnh (Option B)")]
    public Image flashOverlay;             // Kéo FlashOverlay ở Canvas vào
    public AudioClip shutterClip;          // Kéo file âm thanh chupanh vào

    [Header("Âm thanh & Rượt đuổi (Option D)")]
    public AudioSource audioSource;        // AudioSource trên SuKien_ConBo
    public AudioClip cowMooClip;           // File âm thanh boHuc
    public float playerRunHeadstart = 0.8f;// Thời gian chạy trước

    [Header("Thoại Dẫn Dắt Ban Đầu")]
    [TextArea(2, 4)]
    public string introDialogue = "Phía trước có đàn bò đang thong dong gặm cỏ kìa. Trông hiền lành thế thôi chứ đừng dại mà làm phiền chúng nó nha!";

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
            StartCoroutine(PlayConBoCutscene());
        }
    }

    private IEnumerator PlayConBoCutscene()
    {
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.ForceStopAndLook(Vector2.zero);
        }
        yield return new WaitForSeconds(0.2f);

        if (DialogueManager.instance != null && interactableData != null)
        {
            DialogueManager.instance.ShowDialogue(
                interactableData.characterName,
                introDialogue,
                interactableData.avatarIcon
            );

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        if (QuestionManager.instance != null && interactableData != null)
        {
            QuestionManager.instance.SetupQuestion(interactableData);
        }
    }

    public void ChonA_DiVongQua()
    {
        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn B: Tự động đưa Flash lên trên cùng Canvas để chớp sáng rõ rệt
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
            flashOverlay.transform.SetAsLastSibling(); // Đưa flash đè lên mọi UI khác
            flashOverlay.gameObject.SetActive(true);
            Color c = Color.white;
            c.a = 1f;
            flashOverlay.color = c;

            float fadeTime = 0f;
            while (fadeTime < 0.35f)
            {
                fadeTime += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, fadeTime / 0.35f);
                flashOverlay.color = c;
                yield return null;
            }
            c.a = 0f;
            flashOverlay.color = c;
            flashOverlay.gameObject.SetActive(false);
        }

        CowAI.UnfreezePlayer();
    }

    public void ChonC_DuCo()
    {
        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn D: Bò rống -> Cảnh báo -> Đóng thoại -> Bò rống 2 -> Chạy -> Rượt
    /// </summary>
    public void ChonD_ChocBo()
    {
        StartCoroutine(Routine_TrinhTuChocBo());
    }

    private IEnumerator Routine_TrinhTuChocBo()
    {
        if (audioSource != null && cowMooClip != null)
        {
            audioSource.PlayOneShot(cowMooClip);
        }

        yield return new WaitForSeconds(0.2f);
        if (DialogueManager.instance != null && DialogueManager.instance.dialoguePanel != null)
        {
            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        CowAI.UnfreezePlayer();

        if (audioSource != null && cowMooClip != null)
        {
            audioSource.PlayOneShot(cowMooClip);
        }

        yield return new WaitForSeconds(playerRunHeadstart);

        if (CowAI.instance != null)
        {
            CowAI.instance.StartChasing();
        }
    }
}