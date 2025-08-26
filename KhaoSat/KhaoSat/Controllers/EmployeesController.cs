using KhaoSat.Models;
using KhaoSat.Models.Dtos.Employee;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KhaoSat.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index(int? departmentId, string searchString)
        {
            var employees = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Level)
                .Include(e => e.Employeeroles).ThenInclude(er => er.Role)
                .AsQueryable();

            // lọc theo phòng ban
            if (departmentId.HasValue)
            {
                employees = employees.Where(e => e.DepartmentId == departmentId.Value);
            }

            // tìm kiếm theo tên
            if (!string.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(e => e.FullName.Contains(searchString));
            }

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.SelectedDepartment = departmentId;
            ViewBag.SearchString = searchString;

            return View(await employees.ToListAsync());
        }


        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Level)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new EmployeeCreateDto
            {
                HireDate = DateTime.Today
            });
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model.DepartmentId, model.LevelId, model.RoleIds);
                return View(model);
            }

            // Tạo Employee
            var emp = new Employee
            {
                FullName = model.FullName,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth,
                HireDate = model.HireDate,
                Password = model.Password, // nếu sau này dùng hash thì thay bằng PasswordHash
                DepartmentId = model.DepartmentId,
                LevelId = model.LevelId
            };

            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            // Gán vai trò (many-to-many)
            if (model.RoleIds != null && model.RoleIds.Count > 0)
            {
                foreach (var rid in model.RoleIds.Distinct())
                {
                    _context.Employeeroles.Add(new Employeerole
                    {
                        EmployeeId = emp.EmployeeId,
                        RoleId = rid,
                        CreatedAt = DateTime.Now
                    });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns(int? departmentId = null, int? levelId = null, List<int> roleIds = null)
        {
            var departments = await _context.Departments
                .OrderBy(d => d.Name)
                .ToListAsync();

            var levels = await _context.Levels
                .OrderBy(l => l.Name)
                .ToListAsync();

            var roles = await _context.Roles
                .OrderBy(r => r.Name)
                .ToListAsync();

            ViewBag.Departments = new SelectList(departments, "DepartmentId", "Name", departmentId);
            ViewBag.Levels = new SelectList(levels, "LevelId", "Name", levelId);
            ViewBag.Roles = new MultiSelectList(roles, "RoleId", "Name", roleIds);
        }


        // GET: Employees/Edit/5
        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var emp = await _context.Employees
                .Include(e => e.Employeeroles)
                    .ThenInclude(er => er.Role)
                .Include(e => e.Department)
                .Include(e => e.Level)
                .FirstOrDefaultAsync(e => e.EmployeeId == id.Value);

            if (emp == null) return NotFound();

            var dto = new EmployeeEditDto
            {
                EmployeeId = emp.EmployeeId,
                FullName = emp.FullName,
                Email = emp.Email,
                DepartmentId = emp.DepartmentId,
                RoleIds = emp.Employeeroles.Select(er => er.RoleId).ToList(),
                LevelId = emp.LevelId
            };

            // ViewBag để đổ dropdown / listbox
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "Name", dto.DepartmentId);
            ViewBag.Roles = new MultiSelectList(_context.Roles, "RoleId", "Name", dto.RoleIds);
            ViewBag.Levels = new SelectList(_context.Levels, "LevelId", "Name", dto.LevelId);

            return View(dto);

        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeEditDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "Name", dto.DepartmentId);
                ViewBag.Roles = new MultiSelectList(_context.Roles, "RoleId", "Name", dto.RoleIds);
                ViewBag.Levels = new SelectList(_context.Levels, "LevelId", "Name", dto.LevelId);
                return View(dto);
            }

            var emp = await _context.Employees
                .Include(e => e.Employeeroles)
                .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId);

            if (emp == null) return NotFound();

            emp.FullName = dto.FullName;
            emp.Email = dto.Email;
            emp.DepartmentId = dto.DepartmentId;
            emp.LevelId = dto.LevelId;

            // cập nhật roles
            emp.Employeeroles.Clear();
            foreach (var roleId in dto.RoleIds)
            {
                emp.Employeeroles.Add(new Employeerole
                {
                    EmployeeId = emp.EmployeeId,
                    RoleId = roleId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Delete/5
        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Level)
                .Include(e => e.Employeeroles) 
                    .ThenInclude(er => er.Role)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            
            var employeeTests = await _context.Employeetests
                .Where(et => et.EmployeeId == id)
                .ToListAsync();

            var employeeTestIds = employeeTests.Select(et => et.EmployeeTestId).ToList();

            var answers = await _context.Employeeanswers
                .Where(a => employeeTestIds.Contains(a.EmployeeTestId))
                .ToListAsync();

            _context.Employeeanswers.RemoveRange(answers);
            _context.Employeetests.RemoveRange(employeeTests);

            _context.Employeetrainings.RemoveRange(
                _context.Employeetrainings.Where(et => et.EmployeeId == id));

            _context.Employeeroles.RemoveRange(
                _context.Employeeroles.Where(er => er.EmployeeId == id));

            _context.Employeeskills.RemoveRange(
                _context.Employeeskills.Where(es => es.EmployeeId == id));

            _context.Feedbacks.RemoveRange(
                _context.Feedbacks.Where(f => f.EmployeeId == id));

            _context.Notifications.RemoveRange(
                _context.Notifications.Where(n => n.EmployeeId == id));

            _context.Auditlogs.RemoveRange(
                _context.Auditlogs.Where(a => a.EmployeeId == id));

            var createdTests = await _context.Tests
                .Where(t => t.CreatedBy == id)
                .ToListAsync();
            _context.Tests.RemoveRange(createdTests);


            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }

    }
}
