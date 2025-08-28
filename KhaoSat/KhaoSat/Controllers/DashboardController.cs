using KhaoSat.Models;
using KhaoSat.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using System.Linq;
using System.Threading.Tasks;

namespace KhaoSat.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ===== 1. Tổng quan cơ bản =====
            ViewBag.TotalEmployees = await _context.Employees.CountAsync();
            ViewBag.TotalDepartments = await _context.Departments.CountAsync();
            ViewBag.TotalQuestions = await _context.Questions.CountAsync();
            ViewBag.TotalTests = await _context.Tests.CountAsync();

            // ===== 2. Thống kê theo Role + Level =====
            var roleLevelSummary = await _context.Employees
                .Include(e => e.Level)
                .Include(e => e.Employeeroles)
                    .ThenInclude(er => er.Role)
                .ToListAsync();

            var grouped = roleLevelSummary
                .SelectMany(e => e.Employeeroles, (e, er) => new { LevelName = e.Level != null ? e.Level.Name : "Unknown", RoleName = er.Role.Name })
                .GroupBy(x => x.RoleName)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(x => x.LevelName)
                          .ToDictionary(lg => lg.Key, lg => lg.Count())
                );

            ViewBag.RoleLevelSummary = grouped;

            // ===== 3. Thống kê kỹ năng (Skill -> danh sách nhân viên) =====
            // Projection để EF chỉ lấy dữ liệu cần thiết
            var model = await _context.Skills
                .Select(s => new SkillDashboardViewModel
                {
                    SkillId = s.SkillId,
                    SkillName = s.Name,
                    EmployeeNames = s.Employeeskills
                                     .Select(es => es.Employee.FullName)
                                     .OrderBy(name => name)
                                     .ToList()
                })
                .ToListAsync();

            // Truyền model sang View
            return View(model);
        }
        public IActionResult ExportPdf()
        {
            var skills = _context.Skills
                .Select(s => new SkillDashboardViewModel
                {
                    SkillId = s.SkillId,
                    SkillName = s.Name,
                    EmployeeNames = s.Employeeskills
                        .Select(es => es.Employee.FullName)
                        .ToList()
                }).ToList();

            return new ViewAsPdf("ExportPdf", skills)
            {
                FileName = "SkillReport.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
        }
    }
}
