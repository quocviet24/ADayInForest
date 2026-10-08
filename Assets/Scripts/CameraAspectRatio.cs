using UnityEngine;

public class CameraAspectRatio : MonoBehaviour
{
    public float targetAspectX = 16f;
    public float targetAspectY = 9f;
    
    private int lastScreenWidth = 0;
    private int lastScreenHeight = 0;

    void Update()
    {
        // Chỉ tính toán lại viền đen khi phát hiện kích thước cửa sổ bị thay đổi
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            UpdateAspectRatio();
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }

    void UpdateAspectRatio()
    {
        float targetaspect = targetAspectX / targetAspectY;
        float windowaspect = (float)Screen.width / (float)Screen.height;
        float scaleheight = windowaspect / targetaspect;

        Camera camera = GetComponent<Camera>();

        if (scaleheight < 1.0f)
        {
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;
            camera.rect = rect;
        }
        else
        {
            float scalewidth = 1.0f / scaleheight;
            Rect rect = camera.rect;
            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;
            camera.rect = rect;
        }
    }
}