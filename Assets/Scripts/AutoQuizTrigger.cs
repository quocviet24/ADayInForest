using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AutoQuizTrigger : MonoBehaviour
{
    public string questionID = "Q11";

    [HideInInspector]
    public SpriteRenderer iconSpriteRenderer; // Biến nhận diện dấu chấm than từ AutoMapGenerator

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isTriggered && collision.CompareTag("Player"))
        {
            isTriggered = true;

            // Ẩn dấu chấm than ngay khi bước vào vùng hỏi
            if (iconSpriteRenderer != null)
            {
                iconSpriteRenderer.enabled = false;
            }

            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.OpenQuestion(questionID, this.gameObject);
            }
        }
    }
}