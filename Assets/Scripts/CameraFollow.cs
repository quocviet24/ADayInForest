using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("1. Mục tiêu theo dõi")]
    public Transform target;

    [Header("2. Độ mượt")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10f);

    [Header("3. Giới hạn khung hình trong Map")]
    public bool enableBounds = true; // Bật giới hạn biên
    public int mapWidth = 70;        // Nhập đúng Width của MapGenerator
    public int mapHeight = 50;       // Nhập đúng Height của MapGenerator

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        SnapToTarget();
    }

    /// <summary>
    /// Tính toán vị trí bị chặn lại ở mép bản đồ
    /// </summary>
    private Vector3 GetClampedPosition(Vector3 rawPosition)
    {
        if (!enableBounds || cam == null) return rawPosition;

        // Tính bề ngang và bề dọc thực tế mà camera nhìn thấy
        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;

        float halfW = mapWidth / 2f;
        float halfH = mapHeight / 2f;

        // Tính giới hạn nhỏ nhất và lớn nhất mà tâm camera được phép chạm tới
        float minX = -halfW + camHalfWidth;
        float maxX = halfW - camHalfWidth;
        float minY = -halfH + camHalfHeight;
        float maxY = halfH - camHalfHeight;

        // Giữ camera luôn nằm bên trong map
        float clampedX = (minX < maxX) ? Mathf.Clamp(rawPosition.x, minX, maxX) : 0f;
        float clampedY = (minY < maxY) ? Mathf.Clamp(rawPosition.y, minY, maxY) : 0f;

        return new Vector3(clampedX, clampedY, rawPosition.z);
    }

    /// <summary>
    /// Dịch chuyển camera ngay lập tức tới nhân vật (đã tính chặn mép map)
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }

        if (target != null)
        {
            Vector3 rawPos = target.position + offset;
            transform.position = GetClampedPosition(rawPos);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 rawPos = target.position + offset;
        Vector3 targetPos = GetClampedPosition(rawPos);

        // Di chuyển mượt theo vị trí đã được giới hạn
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed);
    }
}