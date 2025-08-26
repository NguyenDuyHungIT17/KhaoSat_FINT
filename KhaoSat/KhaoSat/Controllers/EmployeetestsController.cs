using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KhaoSat.Models;

namespace KhaoSat.Controllers
{
    public class EmployeetestsController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeetestsController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> StartTest(int testId)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Home");

            // Tạo record Employeetest nếu chưa có
            var empTest = await _context.Employeetests
                .FirstOrDefaultAsync(et => et.EmployeeId == empId.Value && et.TestId == testId);

            if (empTest == null)
            {
                empTest = new Employeetest
                {
                    EmployeeId = empId.Value,
                    TestId = testId,
                    StartTime = DateTime.Now
                };
                _context.Employeetests.Add(empTest);
                await _context.SaveChangesAsync();
            }

            // Load các câu hỏi qua bảng Testquestion
            var questions = await _context.Testquestions
                .Where(tq => tq.TestId == testId)
                .Include(tq => tq.Question)
                    .ThenInclude(q => q.QuestionOptions)
                .Include(tq => tq.Question)
                    .ThenInclude(q => q.QuestionTrueFalse)
                .Include(tq => tq.Question)
                    .ThenInclude(q => q.QuestionMatchings)
                .Include(tq => tq.Question)
                    .ThenInclude(q => q.QuestionDragDrops)
                .Select(tq => tq.Question)
                .ToListAsync();

            ViewBag.EmployeeTestId = empTest.EmployeeTestId;
            return View(questions);
        }

        // POST: /EmployeeTests/SubmitTest
        [HttpPost]
        public async Task<IActionResult> SubmitTest(int employeeTestId, Dictionary<int, string> answers)
        {
            foreach (var kv in answers)
            {
                int questionId = kv.Key;
                string answer = kv.Value;

                var empAnswer = new Employeeanswer
                {
                    EmployeeTestId = employeeTestId,
                    QuestionId = questionId,
                    Answer = answer
                };
                _context.Employeeanswers.Add(empAnswer);
            }
            await _context.SaveChangesAsync();

            // Tính tổng điểm đơn giản: số câu đúng chia tổng câu * 10
            var empTest = await _context.Employeetests
                .Include(et => et.Test)
                    .ThenInclude(t => t.Testquestions)
                        .ThenInclude(tq => tq.Question)
                            .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(et => et.EmployeeTestId == employeeTestId);

            int total = 0;
            int correct = 0;
            foreach (var tq in empTest.Test.Testquestions)
            {
                var q = tq.Question;
                if (q.QuestionOptions.Any())
                {
                    var userAns = answers[q.QuestionId];
                    var correctOpt = q.QuestionOptions.FirstOrDefault(o => o.IsCorrect)?.OptionId.ToString();
                    if (userAns == correctOpt) correct++;
                }
                else if (q.QuestionTrueFalse != null)
                {
                    var userAns = answers[q.QuestionId];
                    if (bool.TryParse(userAns, out bool ua) && ua == q.QuestionTrueFalse.CorrectAnswer)
                        correct++;
                }
                // Matching & DragDrop tính sau hoặc bỏ qua demo
            }

            total = (int)((double)correct / empTest.Test.Testquestions.Count * 10);
            empTest.TotalScore = total;
            empTest.EndTime = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("UserIndex", "Home");
        }
    

        // GET: Employeetests
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Employeetests.Include(e => e.Employee).Include(e => e.Test);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Employeetests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeetest = await _context.Employeetests
                .Include(e => e.Employee)
                .Include(e => e.Test)
                .FirstOrDefaultAsync(m => m.EmployeeTestId == id);
            if (employeetest == null)
            {
                return NotFound();
            }

            return View(employeetest);
        }

        // GET: Employeetests/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId");
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId");
            return View();
        }

        // POST: Employeetests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeTestId,EmployeeId,TestId,StartTime,EndTime,Status,TotalScore")] Employeetest employeetest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employeetest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", employeetest.EmployeeId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", employeetest.TestId);
            return View(employeetest);
        }

        // GET: Employeetests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeetest = await _context.Employeetests.FindAsync(id);
            if (employeetest == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", employeetest.EmployeeId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", employeetest.TestId);
            return View(employeetest);
        }

        // POST: Employeetests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeTestId,EmployeeId,TestId,StartTime,EndTime,Status,TotalScore")] Employeetest employeetest)
        {
            if (id != employeetest.EmployeeTestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeetest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeetestExists(employeetest.EmployeeTestId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", employeetest.EmployeeId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", employeetest.TestId);
            return View(employeetest);
        }

        // GET: Employeetests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeetest = await _context.Employeetests
                .Include(e => e.Employee)
                .Include(e => e.Test)
                .FirstOrDefaultAsync(m => m.EmployeeTestId == id);
            if (employeetest == null)
            {
                return NotFound();
            }

            return View(employeetest);
        }

        // POST: Employeetests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employeetest = await _context.Employeetests.FindAsync(id);
            if (employeetest != null)
            {
                _context.Employeetests.Remove(employeetest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeetestExists(int id)
        {
            return _context.Employeetests.Any(e => e.EmployeeTestId == id);
        }
    }
}
