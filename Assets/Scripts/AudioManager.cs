using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Nguồn phát (Kéo AudioSource vào)")]
    public AudioSource bgmSource; // Kênh phát nhạc nền
    public AudioSource sfxSource; // Kênh phát hiệu ứng (click UI)

    [Header("File âm thanh")]
    public AudioClip bgmClip;
    public AudioClip clickClip;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // Tự động phát nhạc nền lặp lại khi vào game
        if (bgmClip != null && bgmSource != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    // Hàm này sẽ gắn vào các nút bấm UI
    public void PlayClick()
    {
        if (clickClip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clickClip);
        }
    }
}