using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class HomeManager : MonoBehaviour
{
    [Header("Cài đặt thành phần")]
    public VideoPlayer introVideo; 
    public GameObject homePanel;   

    [Header("Cài đặt Hiệu ứng (Giây)")]
    public float videoFadeOutTime = 2f; // Thời gian video từ từ mờ đi thành màn hình đen
    public float homeFadeInTime = 2f;   // Thời gian màn hình Home từ từ sáng lên

    private CanvasGroup homeCanvasGroup;

    void Start()
    {
        // 1. Cài đặt cho Home Panel tàng hình lúc mới vào
        if (homePanel != null)
        {
            homeCanvasGroup = homePanel.GetComponent<CanvasGroup>();
            if (homeCanvasGroup == null)
            {
                homeCanvasGroup = homePanel.AddComponent<CanvasGroup>();
            }
            homeCanvasGroup.alpha = 0f; 
            homePanel.SetActive(false);
        }

        // 2. Cài đặt cho Video
        if (introVideo != null)
        {
            // Đảm bảo video rõ nét 100% lúc bắt đầu
            introVideo.targetCameraAlpha = 1f; 
            // Lắng nghe sự kiện chạy xong video
            introVideo.loopPointReached += OnVideoEnd;
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // TẠM DỪNG video để giữ lại khung hình cuối cùng (không tắt phụt nữa)
        introVideo.Pause(); 
        
        // Bắt đầu chuỗi hiệu ứng làm mờ
        StartCoroutine(FadeVideoThenShowHome());
    }

    private IEnumerator FadeVideoThenShowHome()
    {
        // === GIAI ĐOẠN 1: LÀM MỜ VIDEO ===
        if (introVideo != null)
        {
            float time = 0;
            while (time < videoFadeOutTime)
            {
                time += Time.deltaTime;
                // Giảm dần độ mờ của video từ 1 về 0 theo thời gian
                introVideo.targetCameraAlpha = Mathf.Lerp(1f, 0f, time / videoFadeOutTime);
                yield return null; // Chờ frame tiếp theo
            }
            // Đảm bảo video đen hoàn toàn, sau đó mới tắt nó đi để giải phóng bộ nhớ
            introVideo.targetCameraAlpha = 0f;
            introVideo.gameObject.SetActive(false); 
        }

        // === GIAI ĐOẠN 2: LÀM SÁNG HOME PANEL ===
        if (homePanel != null && homeCanvasGroup != null)
        {
            homePanel.SetActive(true);
            homeCanvasGroup.alpha = 0f;
            
            float time = 0;
            while (time < homeFadeInTime)
            {
                time += Time.deltaTime;
                // Tăng dần độ rõ nét của Home từ 0 lên 1 theo thời gian
                homeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / homeFadeInTime);
                yield return null;
            }
            // Đảm bảo Home sáng bừng 100%
            homeCanvasGroup.alpha = 1f;
        }
    }

    // Hàm này sẽ dùng cho nút Bắt Đầu
    public void ClickStartGame()
    {
        Debug.Log("Chuyển sang Scene Phòng của Mẹ!");
    }
}