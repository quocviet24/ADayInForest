using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AutoQuizTrigger : MonoBehaviour
{
    [Header("Mã câu hỏi (Ví dụ: Q11, Q12...)")]
    public string questionID = "Q11";

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi Player bước vào vùng Trigger và câu hỏi chưa từng được kích hoạt
        if (!isTriggered && collision.CompareTag("Player"))
        {
            isTriggered = true;

            // Gọi QuizManager mở câu hỏi
            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.OpenQuestion(questionID, this.gameObject);
            }
            else
            {
                Debug.LogWarning("Chưa tìm thấy QuizManager trong Scene!");
            }
        }
    }
}