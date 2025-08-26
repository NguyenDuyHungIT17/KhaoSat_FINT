using KhaoSat.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net.NetworkInformation;

namespace KhaoSat.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Home/Login
        public IActionResult Login()
        {
            if (HttpContext.Items["LoginError"] != null)
                ViewBag.Error = HttpContext.Items["LoginError"];
            return View();
        }
        public IActionResult AdminIndex() { 
            var role = HttpContext.Session.GetString("RoleName"); 
            if (role != "Admin") { 
                return RedirectToAction("Login", "Home"); 
            } 
            return View(); }

        // GET: /Home/UserIndex
        public async Task<IActionResult> UserIndex()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");   

            var employee = await _context.Employees
                .Include(e => e.Employeeroles).ThenInclude(er => er.Role)
                .Include(e => e.Department)
                .Include(e => e.Level)
                .FirstOrDefaultAsync(e => e.EmployeeId == empId.Value);

            if (employee == null) return RedirectToAction("Login");

            // ----- Thông tin cá nhân -----
            ViewBag.FullName = employee.FullName;
            ViewBag.Email = employee.Email;
            ViewBag.Roles = employee.Employeeroles.Select(er => er.Role.Name).ToList();
            ViewBag.DateOfBirth = employee.DateOfBirth;
            ViewBag.Department = employee.Department?.Name;
            ViewBag.Level = employee.Level?.Name;

            var newemp = await _context.Employees
                        .FirstOrDefaultAsync(e => e.Email == employee.Email);
            if (newemp == null)
            {
                return RedirectToAction("login");
            }
            int newempId = newemp.EmployeeId;

            Console.WriteLine("==============="+ newempId);

            // ----- Dropdown phòng ban -----
            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.DepartmentId = employee.DepartmentId;

            // ----- Lịch sử làm test -----
            var tests = await _context.Employeetests
                 .Include(et => et.Test)
                 .Where(et => et.EmployeeId == newempId)
                 .OrderByDescending(et => et.EndTime)
                 .ToListAsync();

            ViewBag.Tests = tests;

            Console.WriteLine("=== Name: " + tests.Count);

            var roleNames = employee.Employeeroles.Select(er => er.Role.Name).ToList();
            var availableTests = await _context.Tests
                .Where(t => roleNames.Contains(t.Type) &&
                            !_context.Employeetests.Any(et => et.EmployeeId == empId.Value && et.TestId == t.TestId))
                .OrderByDescending(t => t.TestId)
                .ToListAsync();
            ViewBag.AvailableTests = availableTests;

            return View();
        }

        // POST: /Home/UpdateProfile
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullName, string email, DateTime? dateOfBirth, int departmentId)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login");

            var employee = await _context.Employees.FindAsync(empId.Value);
            if (employee != null)
            {
                employee.FullName = fullName;
                employee.Email = email;
                if (dateOfBirth.HasValue)
                    employee.DateOfBirth = dateOfBirth.Value;
                employee.DepartmentId = departmentId;

                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("UserIndex");
        }

        // POST: /Home/ChangePassword
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login");

            var employee = await _context.Employees.FindAsync(empId.Value);
            if (employee == null) return RedirectToAction("Login");

            if (employee.Password != CurrentPassword)
            {
                TempData["Error"] = "Mật khẩu hiện tại không đúng.";
                return RedirectToAction("UserIndex");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "Xác nhận mật khẩu mới không khớp.";
                return RedirectToAction("UserIndex");
            }

            employee.Password = NewPassword;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("UserIndex");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
