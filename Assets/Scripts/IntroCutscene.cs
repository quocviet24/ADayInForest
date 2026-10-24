using System.Collections;
using UnityEngine;

public class IntroCutscene : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName = "Người đồng hành";
        public Sprite speakerAvatar;                  
        [TextArea(2, 4)]
        public string sentence = "Chúng ta bắt đầu hành trình thôi! Phía trước sẽ có rất nhiều chuyện thú vị cho xem...";
    }

    [Header("Mốc Vị Trí Di Chuyển")]
    public Transform spawnPoint;         // Diem_XuatPhat
    public Transform centerPoint;        // Diem_GiuaManHinh
    public float walkSpeed = 2.5f;       

    [Header("Danh Sách Hội Thoại Mở Đầu")]
    public DialogueLine[] dialogueLines;

    private Animator playerAnimator;
    private CameraFollow camFollow;

    void Start()
    {
        StartCoroutine(PlayIntroRoutine());
    }

    private IEnumerator PlayIntroRoutine()
    {
        // 1. Tạm dừng Camera bám theo Player
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            camFollow = mainCam.GetComponent<CameraFollow>();
            if (camFollow != null) camFollow.enabled = false;
        }

        // 2. Khóa bước chân người chơi
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.ForceStopAndLook(Vector2.zero);
            PlayerMovement.instance.enabled = false;
            playerAnimator = PlayerMovement.instance.GetComponent<Animator>();
        }

        // 3. Đưa Player về điểm xuất phát bên trái màn hình
        Transform player = (PlayerMovement.instance != null) ? PlayerMovement.instance.transform : null;
        if (player != null && spawnPoint != null)
        {
            player.position = new Vector3(spawnPoint.position.x, spawnPoint.position.y, 0f);
        }

        yield return new WaitForSeconds(0.3f);

        // 4. Cho nhân vật tự động bước sang phải về phía giữa màn hình
        if (player != null && centerPoint != null)
        {
            SetPlayerWalkAnimation(Vector2.right, true);

            Vector2 targetPos2D = new Vector2(centerPoint.position.x, centerPoint.position.y);

            // Sử dụng Vector2.Distance để triệt tiêu hoàn toàn lỗi lệch trục Z
            while (Vector2.Distance(new Vector2(player.position.x, player.position.y), targetPos2D) > 0.08f)
            {
                Vector3 newPos = Vector3.MoveTowards(
                    player.position,
                    new Vector3(targetPos2D.x, targetPos2D.y, 0f),
                    walkSpeed * Time.deltaTime
                );
                newPos.z = 0f;
                player.position = newPos;
                yield return null;
            }

            // Đặt chính xác vị trí và quay mặt nhìn xuống màn hình
            player.position = new Vector3(targetPos2D.x, targetPos2D.y, 0f);
            SetPlayerWalkAnimation(Vector2.down, false);

            if (PlayerMovement.instance != null)
            {
                PlayerMovement.instance.ForceStopAndLook(Vector2.down);
            }
        }

        yield return new WaitForSeconds(0.4f);

        // 5. Hiển thị các câu thoại mở đầu
        if (DialogueManager.instance != null && dialogueLines != null && dialogueLines.Length > 0)
        {
            for (int i = 0; i < dialogueLines.Length; i++)
            {
                DialogueLine line = dialogueLines[i];
                DialogueManager.instance.ShowDialogue(line.speakerName, line.sentence, line.speakerAvatar);

                // Chờ 0.2s để UI kịp kích hoạt
                yield return new WaitForSeconds(0.2f);

                // Chờ người chơi đọc và bấm đóng bảng thoại
                if (DialogueManager.instance.dialoguePanel != null)
                {
                    yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
                }
                yield return new WaitForSeconds(0.15f);
            }
        }

        // 6. Kích hoạt lại Camera bám theo Player
        if (camFollow != null)
        {
            camFollow.enabled = true;
        }

        // 7. Mở khóa hoàn toàn bước chân cho người chơi
        CowAI.UnfreezePlayer();
    }

    private void SetPlayerWalkAnimation(Vector2 dir, bool isMoving)
    {
        if (playerAnimator == null) return;

        playerAnimator.SetFloat("MoveX", dir.x);
        playerAnimator.SetFloat("MoveY", dir.y);
        playerAnimator.SetBool("IsMoving", isMoving);
    }
}