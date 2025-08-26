namespace KhaoSat.Models.ViewModel
{
    public class TestCreateViewModel
    {
        public string TestName { get; set; }
        public string Type { get; set; }
        public int DurationMinutes { get; set; }
        public int PassingScore { get; set; }
        public int CreatedBy { get; set; }
        public int? SoCauHoi { get; set; }
        public string? Difficulty { get; set; }

        // Danh sách câu hỏi được chọn
        public List<int> SelectedQuestions { get; set; } = new List<int>();
    }
}
