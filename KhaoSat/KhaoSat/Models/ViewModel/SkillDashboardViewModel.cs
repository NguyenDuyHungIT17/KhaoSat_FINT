namespace KhaoSat.Models.ViewModel
{
    public class SkillDashboardViewModel
    {
        public int SkillId { get; set; }
        public string? SkillName { get; set; }
        public List<string>? EmployeeNames { get; set; } = new List<string>();
    }
}
