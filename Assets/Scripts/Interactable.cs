using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Thông tin Nhân vật (Cho Hội thoại/Feedback)")]
    public string characterName = "Người đồng hành";
    public Sprite avatarIcon;

    [Header("Câu hỏi & Đáp án")]
    [TextArea] public string situation; 
    [TextArea] public string question;  

    [System.Serializable]
    public class AnswerOption
    {
        public string answerText;
        public int score;
        [TextArea] public string feedbackText; 
        public UnityEvent onChooseEvent;
    }

    public AnswerOption[] options = new AnswerOption[4];

    [Header("Hiệu ứng báo hiệu (Chỉ dùng cho Tương tác thủ công)")]
    public GameObject indicatorIcon; 
    
    private void Start() 
    { 
        if (indicatorIcon != null) indicatorIcon.SetActive(false); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (indicatorIcon != null) indicatorIcon.SetActive(true);
            PlayerInteraction player = collision.GetComponent<PlayerInteraction>();
            if (player != null) player.SetTarget(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (indicatorIcon != null) indicatorIcon.SetActive(false);
            PlayerInteraction player = collision.GetComponent<PlayerInteraction>();
            if (player != null) player.SetTarget(null);
        }
    }

    // Dành riêng cho các vật thể ấn phím/UI thủ công (VD: Túi rác)
    public void TriggerAction()
    {
        if (indicatorIcon != null) indicatorIcon.SetActive(false);

        if (QuestionManager.instance != null)
        {
            QuestionManager.instance.SetupQuestion(this);
        }
    }
}