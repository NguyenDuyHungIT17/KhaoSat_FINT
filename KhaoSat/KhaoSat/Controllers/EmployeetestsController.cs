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
            var test = await _context.Tests
            .Where(t => t.TestId == empTest.TestId)
            .FirstOrDefaultAsync();

            ViewBag.DurationMinutes = test?.DurationMinutes ?? 0;
            ViewBag.RemainingSeconds = 0;
            if (empTest.StartTime.HasValue && test?.DurationMinutes != null)
            {
                var endTime = empTest.StartTime.Value.AddMinutes(test.DurationMinutes.Value);
                var remain = (endTime - DateTime.Now).TotalSeconds;
                if (remain < 0) remain = 0;
                ViewBag.RemainingSeconds = (int)remain;
            }
            return View(questions);
        }

        // POST: /EmployeeTests/SubmitTest
        // POST: /Employeetests/SubmitTest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTest(int EmployeeTestId)
        {
            var empTest = await _context.Employeetests
                .Include(et => et.Test)
                    .ThenInclude(t => t.Testquestions)
                        .ThenInclude(tq => tq.Question)
                            .ThenInclude(q => q.QuestionOptions)
                .Include(et => et.Test)
                    .ThenInclude(t => t.Testquestions)
                        .ThenInclude(tq => tq.Question)
                            .ThenInclude(q => q.QuestionTrueFalse)
                .Include(et => et.Test)
                    .ThenInclude(t => t.Testquestions)
                        .ThenInclude(tq => tq.Question)
                            .ThenInclude(q => q.QuestionMatchings)
                .Include(et => et.Test)
                    .ThenInclude(t => t.Testquestions)
                        .ThenInclude(tq => tq.Question)
                            .ThenInclude(q => q.QuestionDragDrops)
                .FirstOrDefaultAsync(et => et.EmployeeTestId == EmployeeTestId);

            if (empTest == null) return NotFound();

            // lấy tất cả câu trả lời
            var savedAnswers = await _context.Employeeanswers
                .Where(a => a.EmployeeTestId == EmployeeTestId)
                .ToListAsync();

            double rawScore = 0;
            int totalQuestions = empTest.Test.Testquestions.Count;

            foreach (var tq in empTest.Test.Testquestions)
            {
                var q = tq.Question;
                double qScore = 0.0;

                var userAns = savedAnswers.FirstOrDefault(a => a.QuestionId == q.QuestionId);
                if (userAns == null) continue;

                if (q.QuestionOptions.Any())
                {
                    var correctSet = q.QuestionOptions.Where(o => o.IsCorrect)
                                                      .Select(o => o.OptionId.ToString())
                                                      .ToHashSet();
                    var selectedSet = (userAns.Answer ?? "")
                                      .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => s.Trim()).ToHashSet();
                    if (correctSet.SetEquals(selectedSet)) qScore = 1.0;
                }
                else if (q.QuestionTrueFalse != null)
                {
                    if (bool.TryParse(userAns.Answer, out var userVal))
                    {
                        if (userVal == q.QuestionTrueFalse.CorrectAnswer) qScore = 1.0;
                    }
                }
                else if (q.QuestionMatchings.Any())
                {
                    var pairs = userAns.Answer?.Split('|', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                    int correct = 0, n = q.QuestionMatchings.Count();
                    var matchings = q.QuestionMatchings.ToList();
                    for (int i = 0; i < n && i < pairs.Length; i++)
                    {
                        var expected = $"{matchings[i].LeftItem}->{matchings[i].RightItem}";
                        if (pairs[i].Contains(expected)) correct++;
                    }
                    if (n > 0) qScore = (double)correct / n;
                }
                else if (q.QuestionDragDrops.Any())
                {
                    var items = q.QuestionDragDrops.ToList();
                    int n = items.Count;
                    int correct = 0;
                    foreach (var it in items)
                    {
                        if (userAns.Answer != null && userAns.Answer.Contains($"{it.DraggableText}->{it.DropTarget}"))
                            correct++;
                    }
                    if (n > 0) qScore = (double)correct / n;
                }
                else
                {
                    qScore = 0.0; // Essay chấm sau
                }

                userAns.Score = qScore;
                rawScore += qScore;
            }

            double total10 = totalQuestions > 0
                ? Math.Round(rawScore / totalQuestions * 10.0, 2)
                : 0.0;

            empTest.TotalScore = total10;
            empTest.EndTime = DateTime.Now;
            if (empTest.StartTime.HasValue && empTest.Test.DurationMinutes.HasValue)
            {
                var deadline = empTest.StartTime.Value.AddMinutes(empTest.Test.DurationMinutes.Value);
                empTest.Status = DateTime.Now > deadline ? "Timeout" : "Completed";
            }
            else empTest.Status = "Completed";

            await _context.SaveChangesAsync();

            // 🔹 Nếu là AJAX request → trả JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, score = total10 });
            }

            // 🔹 Nếu submit form bình thường → redirect
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAnswer(int employeeTestId, int questionId, string answer)
        {
            var empTest = await _context.Employeetests
                .Include(et => et.Test)
                    .ThenInclude(t => t.Testquestions)
                        .ThenInclude(tq => tq.Question)
                            .ThenInclude(q => q.QuestionOptions)
                .Include(et => et.Test)
                    .ThenInclude(t => t.Testquestions)
                        .ThenInclude(tq => tq.Question)
                            .ThenInclude(q => q.QuestionTrueFalse)
                .Include(et => et.Test)
                    .ThenInclude(t => t.Testquestions)
                        .ThenInclude(tq => tq.Question)
                            .ThenInclude(q => q.QuestionMatchings)
                .Include(et => et.Test)
                    .ThenInclude(t => t.Testquestions)
                        .ThenInclude(tq => tq.Question)
                            .ThenInclude(q => q.QuestionDragDrops)
                .FirstOrDefaultAsync(et => et.EmployeeTestId == employeeTestId);

            if (empTest == null) return NotFound();

            // 🔹 Lấy hoặc tạo mới câu trả lời
            var existing = await _context.Employeeanswers
                .FirstOrDefaultAsync(a => a.EmployeeTestId == employeeTestId && a.QuestionId == questionId);

            if (existing == null)
            {
                existing = new Employeeanswer
                {
                    EmployeeTestId = employeeTestId,
                    QuestionId = questionId
                };
                _context.Employeeanswers.Add(existing);
            }

            existing.Answer = answer;

            // 🔹 Tính điểm cho câu hiện tại
            var tq = empTest.Test.Testquestions.FirstOrDefault(t => t.QuestionId == questionId);
            double qScore = 0.0;

            if (tq != null)
            {
                var q = tq.Question;

                // --- MCQ ---
                if (q.QuestionOptions.Any())
                {
                    var correctSet = q.QuestionOptions.Where(o => o.IsCorrect)
                                                      .Select(o => o.OptionId.ToString())
                                                      .ToHashSet();

                    var selectedSet = (answer ?? "")
                                      .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => s.Trim())
                                      .ToHashSet();

                    if (correctSet.SetEquals(selectedSet)) qScore = 1.0;
                }
                // --- True/False ---
                else if (q.QuestionTrueFalse != null)
                {
                    if (bool.TryParse(answer, out var userVal))
                    {
                        if (userVal == q.QuestionTrueFalse.CorrectAnswer) qScore = 1.0;
                    }
                }
                // --- Matching ---
                else if (q.QuestionMatchings.Any())
                {
                    var pairs = answer?.Split('|', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                    int correct = 0, n = q.QuestionMatchings.Count();

                    var matchings = q.QuestionMatchings.ToList();
                    for (int i = 0; i < n && i < pairs.Length; i++)
                    {
                        var expected = $"{matchings[i].LeftItem}->{matchings[i].RightItem}";
                        if (pairs[i].Contains(expected)) correct++;
                    }

                    if (n > 0) qScore = (double)correct / n;
                }
                // --- DragDrop ---
                else if (q.QuestionDragDrops.Any())
                {
                    var items = q.QuestionDragDrops.ToList();
                    int n = items.Count;
                    int correct = 0;

                    foreach (var it in items)
                    {
                        if (answer != null && answer.Contains($"{it.DraggableText}->{it.DropTarget}"))
                            correct++;
                    }

                    if (n > 0) qScore = (double)correct / n;
                }
                // --- Essay ---
                else
                {
                    qScore = 0.0; // chấm tay sau
                }
            }

            existing.Score = qScore;

            // 🔹 Cập nhật luôn tổng điểm của toàn bài
            var allAnswers = await _context.Employeeanswers
                 .Where(a => a.EmployeeTestId == employeeTestId)
                 .ToListAsync();

            int totalQuestions = empTest.Test.Testquestions.Count;

            // Fix lỗi nullable
            double rawScore = allAnswers.Sum(a => a.Score ?? 0);

            double total10 = totalQuestions > 0
                ? Math.Round(rawScore / totalQuestions * 10.0, 2)
                : 0.0;

            empTest.TotalScore = total10;

            await _context.SaveChangesAsync();

            return Json(new { success = true, qScore, total10 });
        }

    }
}
