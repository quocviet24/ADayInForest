using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Cutscene_TroiMua : MonoBehaviour
{
    [Header("Cài đặt Hiệu ứng Hình ảnh")]
    public GameObject lightningParticles; 
    public GameObject rainOverlay;        
    public GameObject rainParticles;      

    [Header("Cài đặt Âm thanh")]
    public AudioSource thunderAudioSource; 
    public AudioSource rainAudioSource;    

    [Header("Cài đặt Tạnh Mưa")]
    public float rainStopDelay = 5.0f;     // Thời gian mưa thêm 5 giây cho A, B, D
    public float fadeOutDuration = 2.0f;   // Thời gian bầu trời sáng dần và tiếng mưa tắt hẳn

    [Header("Hội thoại")]
    [TextArea] public string warningText1 = "Cẩn thận, thời tiết đang có vẻ chuyển hướng xấu đi... Có vẻ sắp có một trận mưa to.";
    [TextArea] public string warningText2 = "Thời tiết chuyển xấu có mưa và sấm sét rồi, ta cần làm gì đây?";

    private Interactable interactableData;
    private bool hasTriggered = false;

    private float targetAlpha;
    private Image overlayImage;

    void Start()
    {
        interactableData = GetComponent<Interactable>();
        
        if (lightningParticles != null) lightningParticles.SetActive(false);
        if (rainParticles != null) rainParticles.SetActive(false);

        if (rainOverlay != null)
        {
            overlayImage = rainOverlay.GetComponent<Image>();
            if (overlayImage != null)
            {
                targetAlpha = overlayImage.color.a; 
                
                Color startColor = overlayImage.color;
                startColor.a = 0f;
                overlayImage.color = startColor;
            }
            rainOverlay.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlayRainCutscene());
        }
    }

    private IEnumerator PlayRainCutscene()
    {
        // 1. Phanh gấp
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.ForceStopAndLook(Vector2.zero);
        }
        yield return new WaitForSeconds(0.2f); 

        // 2. Hiện thoại cảnh báo lần 1
        if (DialogueManager.instance != null && interactableData != null)
        {
            DialogueManager.instance.ShowDialogue(interactableData.characterName, warningText1, interactableData.avatarIcon);
            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        // 3. Nổi Sấm Sét đánh ĐOÀNG
        if (lightningParticles != null) lightningParticles.SetActive(true);
        if (thunderAudioSource != null) thunderAudioSource.Play();
        
        yield return new WaitForSeconds(1.5f); 

        // Tắt hiệu ứng sấm sét để không lặp lại
        if (lightningParticles != null) lightningParticles.SetActive(false);

        // 4. Bầu trời TỐI DẦN
        if (rainOverlay != null && overlayImage != null) 
        {
            rainOverlay.SetActive(true);
            yield return StartCoroutine(FadeOverlay(1.0f)); 
        }

        yield return new WaitForSeconds(2.5f); 

        // 5. Bắt đầu đổ mưa rào rào
        if (rainParticles != null) rainParticles.SetActive(true);
        if (rainAudioSource != null) rainAudioSource.Play();

        yield return new WaitForSeconds(1.5f); 

        // 6. Hiện thoại lần 2
        if (DialogueManager.instance != null && interactableData != null)
        {
            DialogueManager.instance.ShowDialogue(interactableData.characterName, warningText2, interactableData.avatarIcon);
            yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);
        }

        // 7. Bật Bảng Câu Hỏi
        if (QuestionManager.instance != null && interactableData != null)
        {
            QuestionManager.instance.SetupQuestion(interactableData);
        }
    }

    private IEnumerator FadeOverlay(float duration)
    {
        float time = 0;
        Color color = overlayImage.color;
        
        while (time < duration)
        {
            color.a = Mathf.Lerp(0, targetAlpha, time / duration);
            overlayImage.color = color;
            time += Time.deltaTime;
            yield return null; 
        }
        
        color.a = targetAlpha;
        overlayImage.color = color;
    }

    // ==============================================================
    // XỬ LÝ KHI CHỌN ĐÁP ÁN
    // ==============================================================

    /// <summary>
    /// Các lựa chọn A, B, D: Mưa tiếp tục rơi thêm 5 giây rồi mới ngớt
    /// </summary>
    public void ChonDapAn_NgungMuaSau5Giay()
    {
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.enabled = true;
        }

        StartCoroutine(Routine_NgungMuaDan(rainStopDelay));
    }

    /// <summary>
    /// Lựa chọn C (Trú mưa): Hết mưa ngay lập tức (delay = 0)
    /// </summary>
    public void ChonC()
    {
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.enabled = true;
        }

        StartCoroutine(Routine_NgungMuaDan(0f));
    }

    public void ChonA() => ChonDapAn_NgungMuaSau5Giay();
    public void ChonB() => ChonDapAn_NgungMuaSau5Giay();
    public void ChonD() => ChonDapAn_NgungMuaSau5Giay();

    private IEnumerator Routine_NgungMuaDan(float delay)
    {
        // Chờ đúng số giây được chỉ định (5s với A, B, D và 0s với C)
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        // 1. Dừng sinh hạt mưa mới
        if (rainParticles != null)
        {
            ParticleSystem ps = rainParticles.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop();
            else rainParticles.SetActive(false);
        }

        // 2. Giảm âm lượng mưa và làm bầu trời sáng lại (Fade Out)
        float elapsed = 0f;
        float startVolume = (rainAudioSource != null) ? rainAudioSource.volume : 1f;
        float startAlpha = (overlayImage != null) ? overlayImage.color.a : targetAlpha;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeOutDuration;

            if (rainAudioSource != null)
            {
                rainAudioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            }

            if (overlayImage != null)
            {
                Color c = overlayImage.color;
                c.a = Mathf.Lerp(startAlpha, 0f, progress);
                overlayImage.color = c;
            }

            yield return null;
        }

        // 3. Tắt hẳn hiệu ứng
        if (rainAudioSource != null)
        {
            rainAudioSource.Stop();
            rainAudioSource.volume = startVolume;
        }

        if (rainParticles != null) rainParticles.SetActive(false);
        if (rainOverlay != null) rainOverlay.SetActive(false);
    }
}