using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// Chuyển tới Scene_Room (Dùng cho nút Bắt đầu ở màn hình Home)
    /// </summary>
    public void VaoPhongNgu()
    {
        SceneManager.LoadScene("Scene_Room");
    }

    /// <summary>
    /// Hàm chuyển Scene theo tên linh hoạt
    /// </summary>
    public void ChuyenScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}