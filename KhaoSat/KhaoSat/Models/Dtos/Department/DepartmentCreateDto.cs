using System.ComponentModel.DataAnnotations;

namespace KhaoSat.Models.Dtos.Department
{
    public class DepartmentCreateDto
    {
        [Required, Display(Name = "Tên phòng ban")]
        public string DepartmentName { get; set; }
    }
}
