using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Kéo nút UI Tương tác vào đây")]
    public GameObject interactButton; 
    
    private Interactable currentTarget;

    void Start()
    {
        if (interactButton != null)
        {
            interactButton.SetActive(false); // Ẩn nút lúc mới vào game
            // Tự động gắn sự kiện bấm nút UI
            interactButton.GetComponent<Button>().onClick.AddListener(Interact);
        }
    }

    void Update()
    {
        // Lắng nghe phím E trên bàn phím máy tính
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    // Hàm thực thi tương tác
    public void Interact()
    {
        if (currentTarget != null)
        {
            currentTarget.TriggerAction();
        }
    }

    // Hàm này để đồ vật tự động gọi khi Player bước vào vùng kích hoạt
    public void SetTarget(Interactable interactable)
    {
        currentTarget = interactable;
        // Bật nút UI lên nếu có đồ vật, tắt đi nếu không có
        if (interactButton != null) 
        {
            interactButton.SetActive(interactable != null);
        }
    }
}