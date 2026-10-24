using System.Collections;
using UnityEngine;

public class Cutscene_NgaBa : MonoBehaviour
{
    [Header("Mục tiêu Camera Lia tới (Đáp án C)")]
    public Transform targetPathTransform;   // Điểm trên con đường an toàn cần lia tới
    public float panSpeed = 6.0f;           // Tốc độ lia camera
    public float pauseAtTarget = 1.0f;      // Thời gian dừng lại xem lối đi

    [Header("Lời khuyên của Mascot (Đáp án C)")]
    [TextArea(2, 4)]
    public string mascotAdvice = "Đường bên phải trông có vẻ bằng phẳng và có biển chỉ dẫn rõ ràng hơn kìa, chúng ta nên đi hướng đó nhé!";

    [Header("Thoại Dẫn Dắt Ban Đầu")]
    [TextArea(2, 4)]
    public string introDialogue = "Phía trước là ngã ba rừng rậm rạp... Ta nên rẽ hướng nào để tiếp tục cuộc hành trình đây?";

    private Interactable interactableData;
    private bool hasTriggered = false;
    private CameraFollow camFollow;

    void Start()
    {
        interactableData = GetComponent<Interactable>();

        if (Camera.main != null)
        {
            camFollow = Camera.main.GetComponent<CameraFollow>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlayNgaBaCutscene());
        }
    }

    private IEnumerator PlayNgaBaCutscene()
    {
        // 1. Phanh cứng bước chân
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.ForceStopAndLook(Vector2.zero);
        }
        yield return new WaitForSeconds(0.2f);

        // 2. Mascot dẫn dắt ban đầu
        if (DialogueManager.instance != null && interactableData != null)
        {
            DialogueManager.instance.ShowDialogue(
                interactableData.characterName,
                introDialogue,
                interactableData.avatarIcon
            );

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        // 3. Mở bảng câu hỏi lựa chọn hướng đi
        if (QuestionManager.instance != null && interactableData != null)
        {
            QuestionManager.instance.SetupQuestion(interactableData);
        }
    }

    // ==========================================
    // CÁC HÀNH ĐỘNG KHI CHỌN ĐÁP ÁN
    // ==========================================

    public void ChonA_ReTrai()
    {
        CowAI.UnfreezePlayer();
    }

    public void ChonB_RePhai()
    {
        CowAI.UnfreezePlayer();
    }

    /// <summary>
    /// Lựa chọn C: Lia camera xem đường -> Mascot khuyên -> Lia camera về -> Mở di chuyển
    /// </summary>
    public void ChonC_HoiBanDongHanh()
    {
        StartCoroutine(Routine_LiaCameraVaGoiY());
    }

    public void ChonD_DungNghi()
    {
        CowAI.UnfreezePlayer();
    }

    private IEnumerator Routine_LiaCameraVaGoiY()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            CowAI.UnfreezePlayer();
            yield break;
        }

        // 1. Tạm dừng CameraFollow
        if (camFollow != null) camFollow.enabled = false;

        float camZ = mainCam.transform.position.z; // Luôn giữ nguyên Z của Camera (-10)

        // 2. Lia camera đến điểm chỉ định (targetPathTransform)
        if (targetPathTransform != null)
        {
            Vector3 targetCamPos = new Vector3(targetPathTransform.position.x, targetPathTransform.position.y, camZ);

            while (Vector2.Distance(mainCam.transform.position, targetCamPos) > 0.08f)
            {
                mainCam.transform.position = Vector3.MoveTowards(
                    mainCam.transform.position,
                    targetCamPos,
                    panSpeed * Time.deltaTime
                );
                yield return null;
            }
            mainCam.transform.position = targetCamPos;
        }

        yield return new WaitForSeconds(pauseAtTarget);

        // 3. Mascot hiện bảng thoại đưa ra lời khuyên
        if (DialogueManager.instance != null && interactableData != null)
        {
            DialogueManager.instance.ShowDialogue(
                interactableData.characterName,
                mascotAdvice,
                interactableData.avatarIcon
            );

            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        yield return new WaitForSeconds(0.2f);

        // 4. Lia camera quay trở lại vị trí Player (Khắc phục triệt để lỗi kẹt trục Z)
        Transform player = (PlayerMovement.instance != null) ? PlayerMovement.instance.transform : null;
        if (player != null)
        {
            Vector3 playerCamPos = new Vector3(player.position.x, player.position.y, camZ);

            while (Vector2.Distance(mainCam.transform.position, playerCamPos) > 0.08f)
            {
                playerCamPos = new Vector3(player.position.x, player.position.y, camZ); // Cập nhật theo Player
                mainCam.transform.position = Vector3.MoveTowards(
                    mainCam.transform.position,
                    playerCamPos,
                    panSpeed * Time.deltaTime
                );
                yield return null;
            }
            mainCam.transform.position = playerCamPos;
        }

        // 5. Bật lại CameraFollow bám theo người chơi
        if (camFollow != null)
        {
            camFollow.enabled = true;
        }

        yield return new WaitForSeconds(0.1f);

        // 6. GIẢI PHÓNG ĐIỀU KHIỂN CHO NGƯỜI CHƠI
        CowAI.UnfreezePlayer();
    }
}