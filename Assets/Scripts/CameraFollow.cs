using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Mục tiêu theo dõi")]
    public Transform target; // Kéo Player vào đây

    [Header("Cài đặt độ mượt")]
    [Range(1f, 10f)]
    public float smoothSpeed = 5f; // Tốc độ di chuyển của camera (càng cao càng bám sát)

    [Header("Khoảng cách (Offset)")]
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Trục Z luôn phải là số âm trong game 2D (ví dụ -10)

    // Dùng LateUpdate thay vì Update để tránh hiện tượng giật lag (jitter) khi nhân vật di chuyển
    void LateUpdate()
    {
        // Kiểm tra xem đã gán nhân vật chưa
        if (target != null)
        {
            // Tính toán vị trí mong muốn mà camera cần tới (vị trí nhân vật + khoảng cách)
            Vector3 desiredPosition = target.position + offset;

            // Dùng Lerp để làm mượt chuyển động từ vị trí hiện tại đến vị trí mong muốn
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Cập nhật vị trí mới cho camera
            transform.position = smoothedPosition;
        }
    }
}