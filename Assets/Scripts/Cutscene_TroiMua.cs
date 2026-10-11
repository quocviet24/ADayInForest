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
        PlayerMovement.instance.ForceStopAndLook(Vector2.zero);
        yield return new WaitForSeconds(0.2f); 

        // 2. Hiện thoại cảnh báo lần 1
        DialogueManager.instance.ShowDialogue(interactableData.characterName, warningText1, interactableData.avatarIcon);
        
        yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);

        // 3. Nổi Sấm Sét đánh ĐOÀNG
        if (lightningParticles != null) lightningParticles.SetActive(true);
        if (thunderAudioSource != null) thunderAudioSource.Play();
        
        // Đợi 1.5 giây để hiệu ứng chớp và tiếng sấm diễn ra
        yield return new WaitForSeconds(1.5f); 

        // --- ĐIỂM MỚI ---
        // Tắt ngay hiệu ứng sấm sét để nó không bị lặp lại (loop)
        if (lightningParticles != null) lightningParticles.SetActive(false);
        // ----------------

        // 4. Bầu trời TỐI DẦN (thực hiện fade trong 1 giây)
        if (rainOverlay != null && overlayImage != null) 
        {
            rainOverlay.SetActive(true);
            yield return StartCoroutine(FadeOverlay(1.0f)); 
        }

        // Đứng im lìm trong bóng tối 2.5 giây
        yield return new WaitForSeconds(2.5f); 

        // 5. Bắt đầu đổ mưa rào rào (Mưa sẽ loop liên tục)
        if (rainParticles != null) rainParticles.SetActive(true);
        if (rainAudioSource != null) rainAudioSource.Play();

        // Đứng dưới mưa 1.5 giây
        yield return new WaitForSeconds(1.5f); 

        // 6. Hiện thoại lần 2
        DialogueManager.instance.ShowDialogue(interactableData.characterName, warningText2, interactableData.avatarIcon);
        yield return new WaitUntil(() => !DialogueManager.instance.dialoguePanel.activeSelf);

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
}