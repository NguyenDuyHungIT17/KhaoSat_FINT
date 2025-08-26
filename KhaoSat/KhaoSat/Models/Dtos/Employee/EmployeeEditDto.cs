namespace KhaoSat.Models.Dtos.Employee
{
    public class EmployeeEditDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public int DepartmentId { get; set; }

        // nhiều role
        public List<int> RoleIds { get; set; } = new List<int>();

        public int? LevelId { get; set; }
    }
}
