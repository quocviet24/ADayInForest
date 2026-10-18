using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class QuizOption
{
    public string text;
    public int score;
    [TextArea(2, 4)] public string feedback;
}

[System.Serializable]
public class QuestionData
{
    public string id;
    public string title;
    [TextArea(2, 4)] public string situation;
    [TextArea(1, 3)] public string question;
    public QuizOption[] options = new QuizOption[4];
    [TextArea(2, 4)] public string rangerInsight;
}

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;

    [Header("--- UI PANELS ---")]
    public GameObject quizPanel;          // Panel chứa câu hỏi & 4 nút đáp án
    public GameObject feedbackPanel;      // Panel hiện kết quả, feedback & Ranger Insight

    [Header("--- UI CÂU HỎI ---")]
    public TextMeshProUGUI txtTitle;
    public TextMeshProUGUI txtSituation;
    public TextMeshProUGUI txtQuestion;
    public Button[] btnOptions;           // 4 Button A, B, C, D
    public TextMeshProUGUI[] txtOptions;  // 4 Text tương ứng

    [Header("--- UI KẾT QUẢ / FEEDBACK ---")]
    public TextMeshProUGUI txtScoreGained;   // Có thể để trống hoặc dùng nếu có text bay riêng
    public TextMeshProUGUI txtFeedback;      // txt_Message bên trong DialoguePanel
    public TextMeshProUGUI txtRangerInsight; // Có thể để trống nếu đã gộp vào txtFeedback
    public Button btnContinue;

    [Header("--- THEO DÕI ĐIỂM SỐ ---")]
    public TextMeshProUGUI txtScoreHUD;   // txt_Score ở góc trên bên trái HUD
    public static int totalScore = 0;

    private QuestionData currentQuestion;
    private GameObject currentTriggerObject;
    private Dictionary<string, QuestionData> questionDatabase = new Dictionary<string, QuestionData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadMap2Questions();
    }

    private void Start()
    {
        if (quizPanel != null) quizPanel.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);

        // Gán sự kiện cho 4 nút lựa chọn
        if (btnOptions != null)
        {
            for (int i = 0; i < btnOptions.Length; i++)
            {
                int index = i;
                if (btnOptions[i] != null)
                {
                    btnOptions[i].onClick.AddListener(() => OnOptionSelected(index));
                }
            }
        }

        if (btnContinue != null)
        {
            btnContinue.onClick.AddListener(OnContinueClicked);
        }

        UpdateScoreHUD();
    }

    public void OpenQuestion(string qID, GameObject triggerObj)
    {
        if (!questionDatabase.ContainsKey(qID)) return;

        currentQuestion = questionDatabase[qID];
        currentTriggerObject = triggerObj;

        // Tạm dừng game để người chơi đọc tình huống
        Time.timeScale = 0f;

        if (txtTitle != null) txtTitle.text = currentQuestion.title;
        if (txtSituation != null) txtSituation.text = currentQuestion.situation;
        if (txtQuestion != null) txtQuestion.text = currentQuestion.question;

        if (btnOptions != null && txtOptions != null)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i < currentQuestion.options.Length && i < txtOptions.Length && txtOptions[i] != null)
                {
                    txtOptions[i].text = currentQuestion.options[i].text;
                }
                if (i < btnOptions.Length && btnOptions[i] != null)
                {
                    btnOptions[i].interactable = true;
                }
            }
        }

        if (quizPanel != null) quizPanel.SetActive(true);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    private void OnOptionSelected(int optionIndex)
    {
        QuizOption selectedOpt = currentQuestion.options[optionIndex];

        // Cộng dồn vào tổng điểm tích lũy
        totalScore += selectedOpt.score;
        UpdateScoreHUD();

        // Đóng panel câu hỏi, bật panel feedback
        if (quizPanel != null) quizPanel.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(true);

        // Cập nhật text tiêu đề của hộp thoại
        if (txtTitle != null)
        {
            txtTitle.text = currentQuestion.title;
        }

        // Chỉ cập nhật txtScoreGained nếu đây là GameObject riêng biệt, không phải là txtScoreHUD
        if (txtScoreGained != null && txtScoreGained != txtScoreHUD)
        {
            txtScoreGained.text = $"+{selectedOpt.score} ĐIỂM";
        }

        // Hiển thị nội dung nhận xét và Ranger Insight
        string fullMessage = $"{selectedOpt.feedback}";
        if (!string.IsNullOrEmpty(currentQuestion.rangerInsight))
        {
            fullMessage += $"\n\n<b>[Ranger Insight]:</b> {currentQuestion.rangerInsight}";
        }

        if (txtFeedback != null)
        {
            txtFeedback.text = fullMessage;
        }

        if (txtRangerInsight != null && txtRangerInsight != txtFeedback)
        {
            txtRangerInsight.text = $"<b>[Ranger Insight]:</b> {currentQuestion.rangerInsight}";
        }
    }

    private void OnContinueClicked()
    {
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        Time.timeScale = 1f;

        // Vô hiệu hóa Trigger vừa hoàn thành để không bị hỏi lại
        if (currentTriggerObject != null)
        {
            Collider2D col = currentTriggerObject.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    /// <summary>
    /// Luôn cập nhật tổng điểm tích lũy trên HUD
    /// </summary>
    private void UpdateScoreHUD()
    {
        if (txtScoreHUD != null)
        {
            txtScoreHUD.text = $"Point: {totalScore}";
        }
    }

    private void LoadMap2Questions()
    {
        questionDatabase.Clear();

        // Q11
        questionDatabase.Add("Q11", new QuestionData
        {
            id = "Q11",
            title = "Q11. TRỜI BẮT ĐẦU MƯA LỚN",
            situation = "Mây đen kéo tới rất nhanh. Gió mạnh hơn, mưa lớn bắt đầu đổ xuống và đường mòn trơn trượt.",
            question = "Bạn ưu tiên điều gì?",
            options = new QuizOption[] {
                new QuizOption { text = "A. Cố đi thật nhanh để hoàn thành hành trình", score = 50, feedback = "Bạn ưu tiên tiến độ, nhưng điều kiện môi trường rừng khi mưa lớn rất nguy hiểm." },
                new QuizOption { text = "B. Hỏi một khách khác xem nên đi tiếp không", score = 75, feedback = "Tham khảo thêm thông tin là tốt, nhưng cần ưu tiên hướng dẫn chính thức của trạm." },
                new QuizOption { text = "C. Kiểm tra thông tin và cân nhắc dừng/đổi tuyến an toàn", score = 100, feedback = "Biết thay đổi kế hoạch và tìm nơi trú ẩn an toàn khi thời tiết đổi thay là kỹ năng sống còn." },
                new QuizOption { text = "D. Bỏ qua vì nghĩ mưa sẽ tạnh nhanh", score = 25, feedback = "Dự đoán chủ quan trong rừng sâu có thể khiến bạn gặp nguy cơ lũ quét hoặc trượt ngã." }
            },
            rangerInsight = "Thời tiết trong rừng biến chuyển rất nhanh, giữ an toàn tính mạng luôn là ưu tiên số một."
        });

        // Q12
        questionDatabase.Add("Q12", new QuestionData
        {
            id = "Q12",
            title = "Q12. CON THÚ BỊ THƯƠNG",
            situation = "Bạn phát hiện một cá thể động vật hoang dã đang nằm bất động bên bìa rừng.",
            question = "Bạn nên làm gì?",
            options = new QuizOption[] {
                new QuizOption { text = "A. Lại gần ôm lấy để kiểm tra thật kỹ", score = 50, feedback = "Bạn có ý tốt, nhưng tiếp cận quá gần có thể khiến thú hoảng sợ tấn công tự vệ." },
                new QuizOption { text = "B. Để lại ít thức ăn của mình bên cạnh rồi đi", score = 50, feedback = "Thức ăn của người chưa chắc phù hợp với thú hoang dã và có thể thu hút thú săn mồi khác." },
                new QuizOption { text = "C. Giữ khoảng cách an toàn và báo lực lượng cứu hộ", score = 100, feedback = "Hỗ trợ đúng cách bắt đầu từ việc không tự ý can thiệp mà gọi người có chuyên môn." },
                new QuizOption { text = "D. Quay video đăng lên mạng xã hội nhờ trợ giúp", score = 25, feedback = "Việc này gây mất thời gian vàng cứu hộ và có thể làm lộ vị trí động vật quý." }
            },
            rangerInsight = "Cứu một con vật không phải lúc nào cũng bắt đầu bằng việc chạm vào nó."
        });

        // Q13
        questionDatabase.Add("Q13", new QuestionData
        {
            id = "Q13",
            title = "Q13. KHÓI TRONG RỪNG",
            situation = "Phía trước xuất hiện một cột khói bốc lên kèm mùi khét lẹt đặc trưng.",
            question = "Bạn sẽ xử lý thế nào?",
            options = new QuizOption[] {
                new QuizOption { text = "A. Chạy thẳng vào đám khói để dập lửa", score = 50, feedback = "Rất dũng cảm nhưng thiếu đồ bảo hộ sẽ khiến bạn bị ngạt khói hoặc kẹt trong đám cháy." },
                new QuizOption { text = "B. Đứng chờ xem khói có tự hết không", score = 25, feedback = "Cháy rừng lan với tốc độ rất nhanh, chần chừ sẽ khiến bạn mất lối thoát." },
                new QuizOption { text = "C. Báo ngay cho kiểm lâm và rút lui về hướng ngược gió", score = 100, feedback = "Thông báo vị trí chính xác và tự bảo vệ mình là phản ứng chuẩn xác nhất." },
                new QuizOption { text = "D. Đứng quay video lại làm tư liệu", score = 25, feedback = "Ghi hình không thể thay thế việc bảo vệ tính mạng và báo động khẩn cấp." }
            },
            rangerInsight = "Phát hiện sớm và báo động kịp thời giúp dập tắt đám cháy từ khi mới manh nha."
        });

        // Q14
        questionDatabase.Add("Q14", new QuestionData
        {
            id = "Q14",
            title = "Q14. NGƯỜI BỊ LẠC",
            situation = "Một người đi rừng hốt hoảng chạy lại: 'Tôi bị lạc khỏi đoàn rồi!'",
            question = "Hành động đúng đắn nhất là gì?",
            options = new QuizOption[] {
                new QuizOption { text = "A. Chỉ đại một hướng bảo họ cứ đi tiếp", score = 50, feedback = "Đoán hướng bừa bãi trong rừng có thể đẩy người lạc vào vùng nguy hiểm hơn." },
                new QuizOption { text = "B. Cùng người đó đi tìm đường theo trực giác", score = 50, feedback = "Thêm người lạc không giúp tìm được đường ra nếu không có bản đồ/la bàn." },
                new QuizOption { text = "C. Trấn an tâm lý, yêu cầu đứng yên và liên hệ cứu hộ", score = 100, feedback = "Bình tĩnh, ở nguyên vị trí an toàn và phát tín hiệu là nguyên tắc tìm kiếm cứu nạn." },
                new QuizOption { text = "D. Đăng bài tìm người thân lên mạng xã hội trước", score = 25, feedback = "Khu vực rừng sâu thường không có sóng mạng ổn định, cần ưu tiên báo trạm gác." }
            },
            rangerInsight = "Khi bị lạc, đứng yên tại chỗ luôn giúp lực lượng cứu hộ định vị bạn nhanh nhất."
        });

        // Q15
        questionDatabase.Add("Q15", new QuestionData
        {
            id = "Q15",
            title = "Q15. DẤU HIỆU VI PHẠM",
            situation = "Bạn nhìn thấy bóng người lén lút mang theo cưa và bao tải trong khu bảo tồn nghiêm ngặt.",
            question = "Bạn phản ứng ra sao?",
            options = new QuizOption[] {
                new QuizOption { text = "A. Lao tới giằng co và chất vấn đối tượng", score = 50, feedback = "Hành động này cực kỳ nguy hiểm vì các đối tượng phá rừng có thể mang theo hung khí." },
                new QuizOption { text = "B. Đứng gần huýt sáo xua đuổi", score = 75, feedback = "Có thể làm đối tượng bỏ chạy nhưng bạn sẽ gặp nguy hiểm nếu bị phát hiện." },
                new QuizOption { text = "C. Giữ khoảng cách bí mật, ghi nhớ đặc điểm và báo kiểm lâm", score = 100, feedback = "Đảm bảo an toàn bản thân và cung cấp thông tin hiện trường là cách hỗ trợ tốt nhất." },
                new QuizOption { text = "D. Bỏ đi coi như không thấy gì", score = 25, feedback = "Sự thờ ơ tạo điều kiện cho các hành vi xâm hại tài nguyên rừng tiếp diễn." }
            },
            rangerInsight = "Kiểm lâm có nghiệp vụ trấn áp tội phạm lâm nghiệp; người dân hỗ trợ qua tin báo mật."
        });

        // Q16
        questionDatabase.Add("Q16", new QuestionData
        {
            id = "Q16",
            title = "Q16. ÂM THANH BẤT THƯỜNG",
            situation = "Trời nhá nhem tối, có tiếng cành cây gãy răng rắc và tiếng động lạ phát ra sau bụi rậm.",
            question = "Bạn sẽ làm gì?",
            options = new QuizOption[] {
                new QuizOption { text = "A. Tò mò chui đầu vào bụi rậm kiểm tra", score = 25, feedback = "Rất nguy hiểm vì đó có thể là rắn độc hoặc thú săn mồi ban đêm." },
                new QuizOption { text = "B. Hét lớn đe dọa sinh vật", score = 50, feedback = "Tiếng thét có thể kích động bản năng tự vệ của động vật hoang dã." },
                new QuizOption { text = "C. Bật đèn pin, giữ cự ly, lùi lại từ từ về tuyến chính", score = 100, feedback = "Ánh sáng giúp nhận diện đường lui an toàn mà không khiêu khích mối nguy." },
                new QuizOption { text = "D. Bỏ chạy thục mạng trong bóng tối", score = 75, feedback = "Chạy tán loạn trong bóng tối dễ vấp ngã xuống vực hoặc mắc bẫy gai." }
            },
            rangerInsight = "Trong rừng đêm, không nhìn thấy không có nghĩa là không có thứ đang quan sát bạn."
        });

        // Q17
        questionDatabase.Add("Q17", new QuestionData
        {
            id = "Q17",
            title = "Q17. BỘ ĐÀM LIÊN LẠC",
            situation = "Bạn thấy một chiếc bộ đàm cầm tay của đội tuần tra đặt trên bàn tiếp nhận.",
            question = "Vì sao thông tin liên lạc là huyết mạch của nghề giữ rừng?",
            options = new QuizOption[] {
                new QuizOption { text = "A. Để các anh em nói chuyện đỡ buồn khi trực đêm", score = 25, feedback = "Liên lạc gắn kết đồng đội, nhưng sứ mệnh của nó trong công việc lớn hơn rất nhiều." },
                new QuizOption { text = "B. Để báo cáo giờ ăn uống nghỉ ngơi", score = 50, feedback = "Báo cáo sinh hoạt chỉ là phần nhỏ trong quy chế công tác." },
                new QuizOption { text = "C. Để phối hợp tác chiến, ứng cứu khẩn cấp giữa rừng sâu", score = 100, feedback = "Địa hình chia cắt, không có sóng điện thoại; bộ đàm là sợi dây cứu sinh kết nối đội ngũ." },
                new QuizOption { text = "D. Chỉ bật khi phát hiện lâm tặc tấn công", score = 50, feedback = "Hệ thống liên lạc phải duy trì liên tục từng giờ để nắm bắt tình hình toàn tuyến." }
            },
            rangerInsight = "Giữ rừng là công việc của tập thể, một cánh én không thể làm nên mùa xuân."
        });

        // Q18
        questionDatabase.Add("Q18", new QuestionData
        {
            id = "Q18",
            title = "Q18. BẢN ĐỒ TUẦN TRA",
            situation = "Tấm bản đồ địa hình với hàng chục tuyến đường tuần tra chằng chịt các ký hiệu màu.",
            question = "Điều gì khiến tuần tra rừng không chỉ là 'đi bộ ngắm cảnh'?",
            options = new QuizOption[] {
                new QuizOption { text = "A. Vì quãng đường đi bộ quá dài và dốc", score = 50, feedback = "Thể lực là thử thách lớn, nhưng chưa phải bản chất cốt lõi của nghề." },
                new QuizOption { text = "B. Vì phải mang vác quân trang nặng nề", score = 50, feedback = "Hành lý chỉ là công cụ hỗ trợ cho chuyến đi." },
                new QuizOption { text = "C. Vì phải kết hợp di chuyển, quan sát, xử lý sự cố và bảo tồn", score = 100, feedback = "Mỗi bước chân là một nhiệm vụ: rà soát bẫy, kiểm tra nguồn nước, ghi nhận sinh thái." },
                new QuizOption { text = "D. Vì trong rừng có nhiều muỗi và vắt", score = 25, feedback = "Côn trùng là chuyện thường nhật mà người giữ rừng nào cũng quen thuộc." }
            },
            rangerInsight = "Một tuyến đường tuần tra không chỉ đi qua, mà phải được thấu hiểu từng tấc đất."
        });

        // Q19
        questionDatabase.Add("Q19", new QuestionData
        {
            id = "Q19",
            title = "Q19. BA LÔ KIỂM LÂM",
            situation = "Chiếc ba lô hành quân với: đèn pin, sổ tay, la bàn, túi sơ cứu y tế, dao đi rừng, nước lọc.",
            question = "Nguyên tắc quan trọng nhất khi chuẩn bị hành trang vào rừng là gì?",
            options = new QuizOption[] {
                new QuizOption { text = "A. Nhét thật nhiều đồ đạc để đề phòng mọi thứ", score = 50, feedback = "Quá tải sẽ bào mòn thể lực và làm bạn kiệt sức trước khi hoàn thành nhiệm vụ." },
                new QuizOption { text = "B. Chỉ mang theo đồ ăn vặt và điện thoại chụp ảnh", score = 25, feedback = "Thiếu thấu hiểu về sự khắc nghiệt của rừng tự nhiên." },
                new QuizOption { text = "C. Chuẩn bị tinh gọn, đúng mục đích và điều kiện thời tiết", score = 100, feedback = "Hành trang chuẩn mực bảo đảm khả năng cơ động và an toàn sinh tồn cao nhất." },
                new QuizOption { text = "D. Không cần mang gì nhiều vì đi rừng quen rồi", score = 25, feedback = "Chủ quan là nguyên nhân hàng đầu dẫn đến các tai nạn đáng tiếc." }
            },
            rangerInsight = "Sự chuẩn bị kỹ lưỡng quyết định 90% thành công của một chuyến tuần tra."
        });

        // Q20
        questionDatabase.Add("Q20", new QuestionData
        {
            id = "Q20",
            title = "Q20. TRẠM KIỂM LÂM — NẾU LÀ BẠN?",
            situation = "Đứng trước Trạm Kiểm Lâm sau hành trình dài đối mặt với mưa gió, vết bẫy, khói lửa và cứu hộ...",
            question = "Điều gì thay đổi nhiều nhất trong suy nghĩ của bạn về rừng?",
            options = new QuizOption[] {
                new QuizOption { text = "A. Tôi sẽ chú ý và cẩn trọng hơn với từng hành động khi vào rừng", score = 100, feedback = "Ý thức của mỗi du khách chính là lá chắn xanh bảo vệ đại ngàn." },
                new QuizOption { text = "B. Tôi thấu hiểu sự hy sinh thầm lặng, gian lao của các chiến sĩ kiểm lâm", score = 100, feedback = "Sự đồng cảm và thấu hiểu của xã hội là nguồn động viên vô giá cho nghề giữ rừng." },
                new QuizOption { text = "C. Tôi nhận ra rừng là một hệ sinh thái gắn kết chặt chẽ cần được bảo vệ", score = 100, feedback = "Bảo vệ rừng chính là bảo vệ nguồn nước, không khí và tương lai của chính chúng ta." },
                new QuizOption { text = "D. Tôi muốn lan tỏa thông điệp của chiến dịch GÁC XANH tới bạn bè", score = 100, feedback = "Cảm ơn bạn đã sẵn sàng trở thành một đại sứ giữ rừng!" }
            },
            rangerInsight = "Rừng không thể tự lên tiếng. Cảm ơn bạn đã lắng nghe hơi thở của đại ngàn hôm nay!"
        });
    }
}