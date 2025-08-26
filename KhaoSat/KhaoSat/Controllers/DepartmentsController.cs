using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KhaoSat.Models;
using KhaoSat.Models.Dtos.Department;

namespace KhaoSat.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly AppDbContext _context;

        public DepartmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            var department = await _context.Departments
                                .Include(d => d.Company)
                                .Include(d => d.Employees)
                                .ToListAsync();
            return View(department);
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.Company)
                .FirstOrDefaultAsync(m => m.DepartmentId == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            var dto = new DepartmentCreateDto(); 
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentCreateDto depart)
        {
            if (ModelState.IsValid)
            {
                var newDepart = new Department
                {
                    Name = depart.DepartmentName,
                    CompanyId = 1
                };

                _context.Departments.Add(newDepart);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(depart);
        }


        // GET: Departments/Edit/5
        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            var dto = new DepartmentEditDto
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name
            };

            return View(dto);
        }


        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentEditDto dto)
        {
            if (id != dto.DepartmentId) return NotFound();

            if (ModelState.IsValid)
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null) return NotFound();

                department.Name = dto.Name;

                _context.Update(department);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(dto);
        }



        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                             .Include(d => d.Company)
                             .Include(d => d.Employees)
                             .FirstOrDefaultAsync(d => d.DepartmentId == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            var employees = await _context.Employees
                .Where(e => e.DepartmentId == id)
                .ToListAsync();

            foreach (var employee in employees)
            {
                var employeeId = employee.EmployeeId;

                var employeeTests = await _context.Employeetests
                    .Where(et => et.EmployeeId == employeeId)
                    .ToListAsync();

                var employeeTestIds = employeeTests.Select(et => et.EmployeeTestId).ToList();

                var answers = await _context.Employeeanswers
                    .Where(a => employeeTestIds.Contains(a.EmployeeTestId))
                    .ToListAsync();

                _context.Employeeanswers.RemoveRange(answers);
                _context.Employeetests.RemoveRange(employeeTests);

                _context.Employeetrainings.RemoveRange(
                    _context.Employeetrainings.Where(et => et.EmployeeId == employeeId));

                _context.Employeeroles.RemoveRange(
                    _context.Employeeroles.Where(er => er.EmployeeId == employeeId));

                _context.Employeeskills.RemoveRange(
                    _context.Employeeskills.Where(es => es.EmployeeId == employeeId));

                _context.Feedbacks.RemoveRange(
                    _context.Feedbacks.Where(f => f.EmployeeId == employeeId));

                _context.Notifications.RemoveRange(
                    _context.Notifications.Where(n => n.EmployeeId == employeeId));

                _context.Auditlogs.RemoveRange(
                    _context.Auditlogs.Where(a => a.EmployeeId == employeeId));

                var createdTests = await _context.Tests
                    .Where(t => t.CreatedBy == employeeId)
                    .ToListAsync();

                _context.Tests.RemoveRange(createdTests);

                _context.Employees.Remove(employee);
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.DepartmentId == id);
        }
    }
}
