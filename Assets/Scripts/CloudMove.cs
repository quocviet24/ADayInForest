using UnityEngine;

public class CloudMove : MonoBehaviour
{
    [Header("Cài đặt Tốc độ")]
    public float minSpeed = 0.88f;
    public float maxSpeed = 2f;

    [Header("Cài đặt Kích thước")]
    public float minScale = 20f; 
    public float maxScale = 26f;

    [Header("Cài đặt Vị trí")]
    public float leftBounds = -20f;
    public float startPositionX = 20f;
    public float minY = -11f;
    public float maxY = 11f;

    [Header("Giãn cách Mây (Mới)")]
    public float minGap = 30f;  // Khoảng cách bắt buộc tối thiểu giữa 2 mây
    public float maxGap = 100f; // Khoảng cách tối đa (Càng to mây ra càng thưa)

    [Header("Kiểu dáng Mây")]
    public Sprite[] cloudSprites;
    
    private float currentSpeed;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ResetCloud(true); 
    }

    void Update()
    {
        transform.Translate(Vector3.left * currentSpeed * Time.deltaTime);

        if (transform.position.x < leftBounds)
        {
            ResetCloud(false);
        }
    }

    void ResetCloud(bool isStarting)
    {
        // 1. Random Tốc độ & Kích thước
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        float randomScale = Random.Range(minScale, maxScale);
        transform.localScale = new Vector3(randomScale, randomScale, 1f);

        // 2. Random Kiểu dáng
        if (cloudSprites != null && cloudSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, cloudSprites.Length);
            spriteRenderer.sprite = cloudSprites[randomIndex];
        }

        // 3. Random Vị trí
        float randomY = Random.Range(minY, maxY);
        float randomX;

        if (isStarting)
        {
            // Mới vào game: Kéo giãn đám mây từ trong màn hình ra tít ngoài xa
            randomX = Random.Range(leftBounds, startPositionX + maxGap);
        }
        else
        {
            // Vòng lặp: Đám mây mới bắt buộc phải đợi cách đám trước ít nhất là minGap
            randomX = startPositionX + Random.Range(minGap, maxGap);
        }
        
        transform.position = new Vector3(randomX, randomY, transform.position.z);
    }
}