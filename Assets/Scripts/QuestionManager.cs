using UnityEngine;
using TMPro;

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager instance;

    [Header("Giao diện Câu hỏi")]
    public GameObject questionPanel;
    public TextMeshProUGUI txtSituation;
    public TextMeshProUGUI txtQuestion;
    public TextMeshProUGUI[] txtOptions;

    [Header("Giao diện Điểm số")]
    public TextMeshProUGUI txtScore; // Đã bổ sung biến này để hết lỗi

    [Header("Dữ liệu Người chơi")]
    public int totalScore = 0;

    private Interactable currentInteractable;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    public void SetupQuestion(Interactable interactableObject)
    {
        currentInteractable = interactableObject;

        txtSituation.text = interactableObject.situation;
        txtQuestion.text = interactableObject.question;

        for (int i = 0; i < 4; i++)
        {
            txtOptions[i].text = interactableObject.options[i].answerText;
        }

        questionPanel.SetActive(true);
    }

    public void SelectOption(int index)
    {
        Interactable.AnswerOption chosenOption = currentInteractable.options[index];

        // 1. Cộng điểm và Cập nhật UI
        totalScore += chosenOption.score;
        if (txtScore != null) txtScore.text = "Điểm: " + totalScore;

        // 2. Chạy sự kiện riêng (Xóa đồ vật, dời chỗ...)
        chosenOption.onChooseEvent.Invoke();

        // 3. Ẩn bảng câu hỏi đi
        questionPanel.SetActive(false);

        // 4. Gọi bảng Dialogue để hiện Lời khuyên (Feedback)
        DialogueManager.instance.ShowDialogue(currentInteractable.characterName, chosenOption.feedbackText, currentInteractable.avatarIcon, () => {
            
            // Khi người chơi bấm qua đoạn lời khuyên -> Zoom Out và Mở khóa di chuyển
            if (CameraController.instance != null) CameraController.instance.ZoomOut(0.5f);
            if (PlayerMovement.instance != null) PlayerMovement.instance.canMove = true;

            // Khóa tương tác của đồ vật để không bị trả lời lặp lại
            currentInteractable.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            if (currentInteractable.indicatorIcon != null) 
                currentInteractable.indicatorIcon.SetActive(false);
        });
    }
}