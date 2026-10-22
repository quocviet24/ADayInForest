using System.Collections;
using UnityEngine;

public class Cutscene_CanhCay : MonoBehaviour
{
    [Header("Cài đặt Cành cây")]
    public GameObject[] branchObjects;   // 4 cành cây chắn đường
    public Transform cleanupPosition;    // Điểm dọn rác ven đường (DiemVutRac)
    public float moveSpeed = 4f;         // Tốc độ gom / bay của cành cây
    public float scatterDistance = 2.5f; // Khoảng cách văng ra khi sút mạnh

    [Header("Thoại Dẫn Dắt (Mascot)")]
    [TextArea(2, 4)]
    public string introDialogue = "Ui, cơn mưa bão vừa rồi có vẻ đã làm gãy một số cành cây ra giữa đường, bạn sẽ làm gì đây?";

    private Interactable interactableData;
    private bool hasTriggered = false;

    void Start()
    {
        interactableData = GetComponent<Interactable>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Vừa bước vào vùng BoxCollider2D là tự động kích hoạt
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlayCanhCayCutscene());
        }
    }

    private IEnumerator PlayCanhCayCutscene()
    {
        // 1. Phanh gấp nhân vật lại
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.ForceStopAndLook(Vector2.zero);
        }
        yield return new WaitForSeconds(0.2f);

        // 2. Mascot tự động nhảy ra thoại dẫn dắt
        if (DialogueManager.instance != null && interactableData != null)
        {
            DialogueManager.instance.ShowDialogue(
                interactableData.characterName, 
                introDialogue, 
                interactableData.avatarIcon
            );

            // Đợi người chơi đọc và click đóng thoại
            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        // 3. Tự động mở Bảng câu hỏi & 4 lựa chọn
        if (QuestionManager.instance != null && interactableData != null)
        {
            QuestionManager.instance.SetupQuestion(interactableData);
        }
    }

    // ==========================================
    // CÁC HÀNH ĐỘNG CUSTOM CHO 4 LỰA CHỌN
    // ==========================================

    /// <summary>
    /// Lựa chọn A: Gom tất cả các cành cây gọn gàng vào điểm dọn rác ven đường
    /// </summary>
    public void ChonA_GomGon()
    {
        StartCoroutine(Routine_GomGon());
    }

    private IEnumerator Routine_GomGon()
    {
        if (cleanupPosition == null || branchObjects == null) yield break;

        float time = 0f;
        Vector3[] startPositions = new Vector3[branchObjects.Length];
        for (int i = 0; i < branchObjects.Length; i++)
        {
            if (branchObjects[i] != null) 
                startPositions[i] = branchObjects[i].transform.position;
        }

        while (time < 1f)
        {
            time += Time.deltaTime * moveSpeed;
            for (int i = 0; i < branchObjects.Length; i++)
            {
                if (branchObjects[i] != null)
                {
                    // Di chuyển mượt về điểm DiemVutRac, xếp lệch nhẹ để tự nhiên
                    Vector3 targetPos = cleanupPosition.position + new Vector3((i - 1.5f) * 0.2f, (i % 2) * 0.15f, 0);
                    branchObjects[i].transform.position = Vector3.Lerp(startPositions[i], targetPos, time);
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Lựa chọn B: Nhặt mất 1 cành làm gậy leo núi, các cành khác giữ nguyên
    /// </summary>
    public void ChonB_NhatMotCanh()
    {
        StartCoroutine(Routine_NhatMotCanh());
    }

    private IEnumerator Routine_NhatMotCanh()
    {
        if (branchObjects == null || branchObjects.Length == 0) yield break;

        // Tìm cành cây đầu tiên để nhặt
        GameObject branchToPick = null;
        foreach (var branch in branchObjects)
        {
            if (branch != null && branch.activeSelf)
            {
                branchToPick = branch;
                break;
            }
        }

        if (branchToPick != null)
        {
            // Hiệu ứng thu nhỏ dần rồi biến mất (tượng trưng cho việc nhặt cất đi)
            float elapsed = 0f;
            Vector3 initialScale = branchToPick.transform.localScale;
            while (elapsed < 0.25f)
            {
                elapsed += Time.deltaTime;
                branchToPick.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, elapsed / 0.25f);
                yield return null;
            }
            branchToPick.SetActive(false);
        }
    }

    /// <summary>
    /// Lựa chọn C: Mặc kệ, giữ nguyên hiện trường
    /// </summary>
    public void ChonC_DeNguyen()
    {
        Debug.Log("[Cutscene_CanhCay] Lựa chọn C: Giữ nguyên cành cây chắn đường.");
    }

    /// <summary>
    /// Lựa chọn D: Sút thật mạnh khiến mỗi cành cây tõe/văng ra một góc khác nhau
    /// </summary>
    public void ChonD_ToeCacHuong()
    {
        StartCoroutine(Routine_ToeCacHuong());
    }

    private IEnumerator Routine_ToeCacHuong()
    {
        if (branchObjects == null || branchObjects.Length == 0) yield break;

        Vector3[] startPositions = new Vector3[branchObjects.Length];
        Vector3[] targetPositions = new Vector3[branchObjects.Length];

        // Tính hướng văng tỏa đều 360 độ cho từng cành cây
        for (int i = 0; i < branchObjects.Length; i++)
        {
            if (branchObjects[i] != null)
            {
                startPositions[i] = branchObjects[i].transform.position;
                
                float angle = (360f / branchObjects.Length) * i + Random.Range(-20f, 20f);
                float rad = angle * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f).normalized;
                
                targetPositions[i] = startPositions[i] + dir * scatterDistance;
            }
        }

        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime * (moveSpeed * 1.5f);
            for (int i = 0; i < branchObjects.Length; i++)
            {
                if (branchObjects[i] != null)
                {
                    branchObjects[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], time);
                    // Xoay tròn khi văng
                    branchObjects[i].transform.Rotate(0, 0, 360f * Time.deltaTime);
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Hàm dự phòng nhận index trực tiếp (0 = A, 1 = B, 2 = C, 3 = D)
    /// </summary>
    public void ExecuteActionBasedOnChoice(int choiceIndex)
    {
        switch (choiceIndex)
        {
            case 0: ChonA_GomGon(); break;
            case 1: ChonB_NhatMotCanh(); break;
            case 2: ChonC_DeNguyen(); break;
            case 3: ChonD_ToeCacHuong(); break;
        }
    }
}