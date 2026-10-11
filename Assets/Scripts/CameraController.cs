using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    private Camera cam;
    
    private float defaultSize;
    private Vector3 defaultPosition; // Lưu tọa độ gốc của Camera

    void Awake()
    {
        instance = this;
        cam = GetComponent<Camera>();
        
        defaultSize = cam.orthographicSize;
        defaultPosition = transform.position; // Ghi nhớ vị trí ban đầu
    }

    // Nhận thêm biến Transform target (vị trí của Player)
    public void ZoomIn(float targetSize, float duration, Transform target)
    {
        StopAllCoroutines();
        StartCoroutine(ZoomRoutine(targetSize, target.position, duration));
    }

    public void ZoomOut(float duration)
    {
        StopAllCoroutines();
        // Trả camera về kích thước và vị trí ban đầu
        StartCoroutine(ZoomRoutine(defaultSize, defaultPosition, duration));
    }

    IEnumerator ZoomRoutine(float targetSize, Vector3 targetPos, float duration)
    {
        float startSize = cam.orthographicSize;
        Vector3 startPos = transform.position;
        
        // Khóa trục Z ở -10 (hoặc vị trí hiện tại) để camera không đâm xuyên qua map 2D
        Vector3 finalPos = new Vector3(targetPos.x, targetPos.y, transform.position.z); 

        float time = 0;
        
        while (time < duration)
        {
            // Vừa thu phóng...
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, time / duration);
            // ...Vừa di chuyển Camera tới chỗ nhân vật
            transform.position = Vector3.Lerp(startPos, finalPos, time / duration);
            
            time += Time.deltaTime;
            yield return null;
        }
        
        cam.orthographicSize = targetSize;
        transform.position = finalPos;
    }
}