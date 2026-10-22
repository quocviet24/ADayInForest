using System.Collections;
using UnityEngine;
using TMPro; // Dùng cho TextMeshPro

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string sentence;
}

public class RoomCutsceneManager : MonoBehaviour
{
    [Header("Nhân vật & Di chuyển")]
    public Transform momTransform;      // Kéo NPC_Me vào đây
    public Transform momTargetPosition; // Kéo Target_Mom vào đây
    public float momMoveSpeed = 3f;     // Tốc độ mẹ đi

    [Header("UI Hội thoại")]
    public GameObject dialoguePanel;   // Kéo DialoguePanel vào đây
    public TextMeshProUGUI txtName;    // Kéo chữ Tên vào đây
    public TextMeshProUGUI txtMessage; // Kéo chữ Nội dung vào đây
    public float typingSpeed = 0.04f;  // Tốc độ gõ chữ

    [Header("Kịch bản thoại")]
    public DialogueLine[] dialogues;   // Chỗ để bạn gõ thoại của Mẹ và Con

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool isCutsceneActive = false;

    void Start()
    {
        dialoguePanel.SetActive(false);
        StartCoroutine(StartCutsceneSequence());
    }

    void Update()
    {
        // Khi đang thoại, click chuột trái hoặc Space để Next
        if (isCutsceneActive && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (isTyping)
            {
                // Nếu chữ đang chạy mà bấm chuột -> Hiện full câu luôn
                StopAllCoroutines();
                txtMessage.text = dialogues[currentLineIndex].sentence;
                isTyping = false;
            }
            else
            {
                // Chuyển sang câu tiếp theo
                NextDialogue();
            }
        }
    }

    IEnumerator StartCutsceneSequence()
    {
        // 1. Khoảng lặng ban đầu (Con đang ngồi chơi)
        yield return new WaitForSeconds(1f);

        // 2. Tiếng gõ cửa (Hiện tại in ra Log, sau này ghép âm thanh sau)
        Debug.Log("*Cốc cốc cốc* Mẹ vào nhé!");
        yield return new WaitForSeconds(1f);

        // 3. Mẹ đi từ ngoài vào điểm Target
        if (momTransform != null && momTargetPosition != null)
        {
            while (Vector3.Distance(momTransform.position, momTargetPosition.position) > 0.1f)
            {
                momTransform.position = Vector3.MoveTowards(momTransform.position, momTargetPosition.position, momMoveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        // 4. Đến nơi thì bắt đầu mở bảng chat
        dialoguePanel.SetActive(true);
        isCutsceneActive = true;
        StartDialogue();
    }

    void StartDialogue()
    {
        currentLineIndex = 0;
        StartCoroutine(TypeSentence(dialogues[currentLineIndex]));
    }

    void NextDialogue()
    {
        currentLineIndex++;
        if (currentLineIndex < dialogues.Length)
        {
            StartCoroutine(TypeSentence(dialogues[currentLineIndex]));
        }
        else
        {
            EndCutscene();
        }
    }

    IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true;
        txtName.text = line.speakerName;
        txtMessage.text = "";

        // Hiệu ứng chữ chạy lạch cạch
        foreach (char letter in line.sentence.ToCharArray())
        {
            txtMessage.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void EndCutscene()
    {
        dialoguePanel.SetActive(false);
        isCutsceneActive = false;
        Debug.Log("Hết thoại! Bắt đầu lệnh Load Scene sang Map 1...");
        // Code chuyển Scene sẽ để ở đây sau
    }
}