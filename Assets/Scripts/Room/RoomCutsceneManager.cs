using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;         // Tên người nói (Mẹ, Bạn, v.v.)
    public Sprite avatar;              // Ảnh avatar hiển thị
    [TextArea(2, 5)]
    public string sentence;            // Nội dung câu thoại
}

public class RoomCutsceneManager : MonoBehaviour
{
    [Header("Nhân vật & Di chuyển")]
    public Transform momTransform;      // Kéo NPC_Me vào đây
    public Transform momTargetPosition; // Kéo Target_Mom vào đây
    public float momMoveSpeed = 2.5f;   // Tốc độ mẹ bước vào

    [Header("UI Hội thoại")]
    public GameObject dialoguePanel;   // Kéo DialoguePanel vào đây
    public TextMeshProUGUI txtName;    // Kéo chữ Tên vào đây
    public TextMeshProUGUI txtMessage; // Kéo chữ Nội dung vào đây
    public Image imgAvatar;            // Kéo Image Avatar vào đây (nếu có, không có để trống)
    public float typingSpeed = 0.035f; // Tốc độ gõ chữ

    [Header("Chuyển Scene & Chuyển Cảnh")]
    public string nextSceneName = "Map1";      // Tên Scene nạp tiếp theo
    public Image blackOverlay;                 // Kéo BlackOverlay từ Canvas vào
    public float delayBeforeLoadScene = 0.5f;  // Thời gian chờ màn hình đen trước khi load map

    [Header("Âm thanh (Tùy chọn)")]
    public AudioSource audioSource;    // AudioSource trong Scene
    public AudioClip knockDoorClip;    // Tiếng gõ cửa "cốc cốc cốc"

    [Header("Kịch bản thoại Mẹ & Con")]
    public DialogueLine[] dialogues;   // Danh sách thoại

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool isCutsceneActive = false;
    private Coroutine typingCoroutine;
    private Animator momAnimator;

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (momTransform != null)
        {
            momAnimator = momTransform.GetComponent<Animator>();
        }

        if (blackOverlay != null)
        {
            Color c = blackOverlay.color;
            c.a = 0f;
            blackOverlay.color = c;
            blackOverlay.gameObject.SetActive(false);
        }

        StartCoroutine(StartCutsceneSequence());
    }

    void Update()
    {
        // Khi đang thoại, click chuột trái hoặc Space để Next
        if (isCutsceneActive && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (isTyping)
            {
                // Nếu chữ đang chạy mà bấm chuột -> Hiện trọn vẹn câu ngay
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
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
        // 1. Khoảng lặng ban đầu (Con đang ngồi trong phòng)
        yield return new WaitForSeconds(0.8f);

        // 2. Tiếng gõ cửa "Cốc cốc cốc"
        if (audioSource != null && knockDoorClip != null)
        {
            audioSource.PlayOneShot(knockDoorClip);
        }
        else
        {
            Debug.Log("*Cốc cốc cốc* Mẹ vào nhé!");
        }
        yield return new WaitForSeconds(1.2f);

        // 3. Mẹ đi từ ngoài cửa vào điểm Target
        if (momTransform != null && momTargetPosition != null)
        {
            Vector2 moveDir = (momTargetPosition.position - momTransform.position).normalized;
            SetMomAnimation(moveDir, true);

            while (Vector2.Distance(momTransform.position, momTargetPosition.position) > 0.08f)
            {
                momTransform.position = Vector3.MoveTowards(
                    momTransform.position,
                    new Vector3(momTargetPosition.position.x, momTargetPosition.position.y, momTransform.position.z),
                    momMoveSpeed * Time.deltaTime
                );
                yield return null;
            }

            momTransform.position = new Vector3(momTargetPosition.position.x, momTargetPosition.position.y, momTransform.position.z);
            SetMomAnimation(Vector2.down, false); // Dừng chân, quay mặt nhìn xuống
        }

        yield return new WaitForSeconds(0.4f);

        // 4. Đến nơi thì bắt đầu mở bảng chat
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        isCutsceneActive = true;
        StartDialogue();
    }

    void StartDialogue()
    {
        currentLineIndex = 0;
        if (dialogues != null && dialogues.Length > 0)
        {
            ShowCurrentLine();
        }
        else
        {
            EndCutscene();
        }
    }

    void NextDialogue()
    {
        currentLineIndex++;
        if (currentLineIndex < dialogues.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            EndCutscene();
        }
    }

    void ShowCurrentLine()
    {
        DialogueLine line = dialogues[currentLineIndex];
        txtName.text = line.speakerName;

        // Cập nhật Avatar nếu có
        if (imgAvatar != null)
        {
            if (line.avatar != null)
            {
                imgAvatar.gameObject.SetActive(true);
                imgAvatar.sprite = line.avatar;
            }
            else
            {
                imgAvatar.gameObject.SetActive(false);
            }
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(line.sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        txtMessage.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            txtMessage.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void EndCutscene()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        isCutsceneActive = false;

        StartCoroutine(Routine_ChuyenSangMap1());
    }

    private IEnumerator Routine_ChuyenSangMap1()
    {
        // 1. Màn hình tối dần sang đen (Fade Out)
        if (blackOverlay != null)
        {
            blackOverlay.transform.SetAsLastSibling();
            blackOverlay.gameObject.SetActive(true);
            float t = 0f;
            Color c = blackOverlay.color;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, t / 0.5f);
                blackOverlay.color = c;
                yield return null;
            }
            c.a = 1f;
            blackOverlay.color = c;
        }

        yield return new WaitForSeconds(delayBeforeLoadScene);

        // 2. Chuyển thẳng sang Map1
        Debug.Log("Nạp Scene: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }

    private void SetMomAnimation(Vector2 dir, bool isMoving)
    {
        if (momAnimator == null) return;
        momAnimator.SetFloat("MoveX", dir.x);
        momAnimator.SetFloat("MoveY", dir.y);
        momAnimator.SetBool("IsMoving", isMoving);
    }
}