using System.Collections;
using UnityEngine;

public class Cutscene_BangChiDan : MonoBehaviour
{
    [Header("Hội thoại Mở đầu")]
    [TextArea] public string introDialogue; 

    private Interactable interactableData;
    private bool hasTriggered = false;

    void Start()
    {
        interactableData = GetComponent<Interactable>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            
            if (interactableData != null && interactableData.indicatorIcon != null) 
                interactableData.indicatorIcon.SetActive(false);

            StartCoroutine(PlayCutsceneSequence());
        }
    }

    private IEnumerator PlayCutsceneSequence()
    {
        // 1. Phanh gấp, khựng lại một nhịp
        PlayerMovement.instance.ForceStopAndLook(Vector2.zero);
        yield return new WaitForSeconds(0.5f); 

        // 2. Zoom Camera tiến lại gần (Tăng thời gian zoom cho êm hơn)
        float zoomDuration = 1.0f; // Zoom chậm rãi trong 1 giây
        if (CameraController.instance != null && PlayerMovement.instance != null)
        {
            CameraController.instance.ZoomIn(3f, zoomDuration, PlayerMovement.instance.transform);
        }
        
        // Đợi Camera zoom xong hoàn toàn
        yield return new WaitForSeconds(zoomDuration);

        // THÊM DELAY: Camera dừng hẳn, để im khung cảnh 0.3 giây
        yield return new WaitForSeconds(0.8f); 

        // 3. Xoay mặt nhìn lên bảng chỉ dẫn
        PlayerMovement.instance.ForceStopAndLook(Vector2.up);
        
        // THÊM DELAY: Đứng tĩnh nhìn bảng 0.8 giây cho chân thực rồi mới hiện chữ
        yield return new WaitForSeconds(1f); 

        // 4. Mở hộp thoại hội thoại
        DialogueManager.instance.ShowDialogue(
            interactableData.characterName, 
            introDialogue, 
            interactableData.avatarIcon, 
            () => {
                if (QuestionManager.instance != null && interactableData != null)
                {
                    QuestionManager.instance.SetupQuestion(interactableData);
                }
            }
        );
    }
}