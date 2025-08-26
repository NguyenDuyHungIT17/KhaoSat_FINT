using System.ComponentModel.DataAnnotations;

namespace KhaoSat.Models.Dtos.Department
{
    public class DepartmentEditDto
    {
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}