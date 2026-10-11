using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("Kết nối Giao diện")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtMessage;
    public Image avatarImage;

    [Header("Cài đặt")]
    public float typingSpeed = 0.03f; // Tốc độ gõ chữ (càng nhỏ càng nhanh)

    private bool isTyping = false;
    private string fullText = "";
    private Action onDialogueComplete; // Lưu trữ hành động tiếp theo sau khi nói xong
    private Coroutine typingCoroutine;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    // Hàm này được gọi từ bên ngoài để bắt đầu nói chuyện
    public void ShowDialogue(string characterName, string message, Sprite avatar, Action onComplete = null)
    {
        dialoguePanel.SetActive(true);
        txtName.text = characterName;
        
        if (avatar != null && avatarImage != null) 
            avatarImage.sprite = avatar;

        fullText = message;
        onDialogueComplete = onComplete;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        isTyping = true;
        txtMessage.text = "";
        
        // Vòng lặp in ra từng chữ cái
        foreach (char letter in fullText.ToCharArray())
        {
            txtMessage.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
    }

    void Update()
    {
        // Kiểm tra người chơi bấm chuột trái (hoặc chạm màn hình điện thoại)
        if (Input.GetMouseButtonDown(0) && dialoguePanel.activeSelf)
        {
            if (isTyping)
            {
                // Đang gõ chữ dở -> Bấm để hiện đầy đủ câu luôn
                StopCoroutine(typingCoroutine);
                txtMessage.text = fullText;
                isTyping = false;
            }
            else
            {
                // Đã nói xong -> Bấm để đóng hộp thoại và làm việc tiếp theo
                dialoguePanel.SetActive(false);
                
                // Gọi hành động tiếp theo (Mở bảng câu hỏi hoặc Chạy sự kiện)
                onDialogueComplete?.Invoke();
            }
        }
    }
}