
using System.ComponentModel.DataAnnotations;

namespace KhaoSat.Models.ViewModel
{
    public class TestEditViewModel
    {
        public int TestId { get; set; }

        [Required(ErrorMessage = "Tên bài test không được để trống")]
        [Display(Name = "Tên bài test")]
        public string TestName { get; set; }

        [Required(ErrorMessage = "Loại bài test không được để trống")]
        [Display(Name = "Đối tượng")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Thời gian không được để trống")]
        [Range(1, 500, ErrorMessage = "Thời gian phải lớn hơn 0")]
        [Display(Name = "Thời gian (phút)")]
        public int? DurationMinutes { get; set; }

        [Required(ErrorMessage = "Điểm đạt không được để trống")]
        [Range(0, 100, ErrorMessage = "Điểm đạt phải từ 0 đến 100")]
        [Display(Name = "Điểm đạt")]
        public double? PassingScore { get; set; }

        [Required(ErrorMessage = "Người tạo không được để trống")]
        [Display(Name = "Người tạo")]
        public int? CreatedBy { get; set; }

        // Danh sách QuestionId được chọn
        public List<int> SelectedQuestions { get; set; }
    }
}
