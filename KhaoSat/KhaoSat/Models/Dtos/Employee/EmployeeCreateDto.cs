using System.ComponentModel.DataAnnotations;

namespace KhaoSat.Models.Dtos.Employee
{
    public class EmployeeCreateDto
    {
        [Required, Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Date), Display(Name = "Ngày sinh")]
        public DateTime? DateOfBirth { get; set; }

        [DataType(DataType.Date), Display(Name = "Ngày vào làm")]
        public DateTime? HireDate { get; set; }

        [Required, Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Required, Display(Name = "Phòng ban")]
        public int DepartmentId { get; set; }

        [Display(Name = "Cấp bậc (Level)")]
        public int? LevelId { get; set; }

        [Display(Name = "Vai trò (có thể chọn nhiều)")]
        public List<int> RoleIds { get; set; } = new();  
    }
}
