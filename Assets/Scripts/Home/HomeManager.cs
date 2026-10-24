using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class HomeManager : MonoBehaviour
{
    [Header("UI Màn hình")]
    public GameObject clickToStartPanel;   // Màn hình đen "Click để bắt đầu"
    public GameObject homePanel;           // Giao diện chính chứa nút Bắt đầu

    [Header("Cài đặt Video")]
    public VideoPlayer introVideo; 
    public string videoFileName = "AiGiuRungDzay_logo_motion_v2.mp4"; 

    [Header("Thời gian hiệu ứng (Giây)")]
    public float videoFadeOutTime = 1.2f; 
    public float homeFadeInTime = 1.2f;   

    private CanvasGroup homeCanvasGroup;
    private bool isWaitingForClick = true;
    private bool isTransitioning = false;
    private float playDurationCounter = 0f;

    void Start()
    {
        // 1. Cài đặt Home Panel ẩn ban đầu
        if (homePanel != null)
        {
            homeCanvasGroup = homePanel.GetComponent<CanvasGroup>();
            if (homeCanvasGroup == null)
            {
                homeCanvasGroup = homePanel.AddComponent<CanvasGroup>();
            }
            homeCanvasGroup.alpha = 0f;
            homeCanvasGroup.interactable = false;
            homeCanvasGroup.blocksRaycasts = false;
            homePanel.SetActive(false);
        }

        // 2. Luôn bật màn hình Click to Start đầu tiên
        if (clickToStartPanel != null)
        {
            clickToStartPanel.SetActive(true);
        }

        // 3. Chuẩn bị nạp trước video vào bộ nhớ đệm
        if (introVideo != null)
        {
            introVideo.playOnAwake = false;
            introVideo.source = VideoSource.Url;
            string videoPath = Path.Combine(Application.streamingAssetsPath, videoFileName);
            introVideo.url = videoPath;
            introVideo.targetCameraAlpha = 1f;

            introVideo.loopPointReached += (vp) => { KetThucVideo(); };
            introVideo.errorReceived += (vp, msg) => {
                Debug.LogWarning("[HomeManager] Lỗi nạp video: " + msg);
                HienThiHomeNgay();
            };

            introVideo.Prepare();
        }
        else
        {
            HienThiHomeNgay();
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        // GIAI ĐOẠN 1: Chờ người chơi tương tác lần đầu để mở khóa quyền âm thanh cho trình duyệt
        if (isWaitingForClick)
        {
            if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
            {
                BatDauPhatVideo();
            }
            return;
        }

        // GIAI ĐOẠN 2: Video đang phát
        if (introVideo != null && introVideo.isPlaying)
        {
            playDurationCounter += Time.deltaTime;

            // Kiểm tra khi video phát gần chạm đích
            if (introVideo.length > 0 && introVideo.time >= (introVideo.length - 0.15f))
            {
                KetThucVideo();
                return;
            }

            // Cho phép Skip bằng chuột hoặc Space sau khi xem tối thiểu 1.5 giây
            if (playDurationCounter > 1.5f && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
            {
                KetThucVideo();
            }
        }
    }

    void BatDauPhatVideo()
    {
        isWaitingForClick = false;

        // Tắt màn hình Click to Start
        if (clickToStartPanel != null)
        {
            clickToStartPanel.SetActive(false);
        }

        // Phát video (lúc này trình duyệt đã có quyền âm thanh nên sẽ phát có tiếng 100%)
        if (introVideo != null)
        {
            introVideo.Play();
        }
        else
        {
            HienThiHomeNgay();
        }
    }

    private void KetThucVideo()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        StartCoroutine(Routine_ChuyenTuVideoSangHome());
    }

    private IEnumerator Routine_ChuyenTuVideoSangHome()
    {
        // 1. Làm mờ Video
        if (introVideo != null)
        {
            float elapsed = 0f;
            float startAlpha = introVideo.targetCameraAlpha;
            while (elapsed < videoFadeOutTime)
            {
                elapsed += Time.deltaTime;
                introVideo.targetCameraAlpha = Mathf.Lerp(startAlpha, 0f, elapsed / videoFadeOutTime);
                yield return null;
            }
            introVideo.targetCameraAlpha = 0f;
            introVideo.Stop();
            introVideo.gameObject.SetActive(false);
        }

        // 2. Làm sáng dần giao diện Home
        if (homePanel != null && homeCanvasGroup != null)
        {
            homePanel.SetActive(true);
            homeCanvasGroup.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < homeFadeInTime)
            {
                elapsed += Time.deltaTime;
                homeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / homeFadeInTime);
                yield return null;
            }
            homeCanvasGroup.alpha = 1f;
            homeCanvasGroup.interactable = true;
            homeCanvasGroup.blocksRaycasts = true;
        }
    }

    private void HienThiHomeNgay()
    {
        isTransitioning = true;
        isWaitingForClick = false;
        if (clickToStartPanel != null) clickToStartPanel.SetActive(false);
        if (introVideo != null) introVideo.gameObject.SetActive(false);
        if (homePanel != null)
        {
            homePanel.SetActive(true);
            if (homeCanvasGroup != null)
            {
                homeCanvasGroup.alpha = 1f;
                homeCanvasGroup.interactable = true;
                homeCanvasGroup.blocksRaycasts = true;
            }
        }
    }
}