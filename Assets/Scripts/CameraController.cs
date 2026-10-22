using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    private Camera cam;
    
    [Header("Kích thước mặc định")]
    public float defaultSize;

    void Awake()
    {
        instance = this;
        cam = GetComponent<Camera>();
        
        // Tự động ghi nhận Size hiện tại làm Size gốc nếu chưa điền ở Inspector
        if (defaultSize <= 0)
        {
            defaultSize = cam.orthographicSize;
        }
    }

    /// <summary>
    /// Thu nhỏ ống kính (Zoom In). Tham số target giữ lại để tương thích các script gọi từ trước.
    /// </summary>
    public void ZoomIn(float targetSize, float duration, Transform target = null)
    {
        StopAllCoroutines();
        StartCoroutine(ZoomSizeRoutine(targetSize, duration));
    }

    /// <summary>
    /// Trả ống kính về kích thước ban đầu (Zoom Out).
    /// </summary>
    public void ZoomOut(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ZoomSizeRoutine(defaultSize, duration));
    }

    /// <summary>
    /// Chỉ thay đổi góc nhìn (orthographicSize), để CameraFollow toàn quyền bám sát Player.
    /// </summary>
    IEnumerator ZoomSizeRoutine(float targetSize, float duration)
    {
        float startSize = cam.orthographicSize;
        float time = 0;
        
        while (time < duration)
        {
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        
        cam.orthographicSize = targetSize;
    }
}