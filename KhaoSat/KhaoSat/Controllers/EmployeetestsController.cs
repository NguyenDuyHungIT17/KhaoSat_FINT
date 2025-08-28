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

        #region Helper
        private async Task AddEmployeeSkillIfNotExists(int employeeId, int? skillId)
        {
            if (!skillId.HasValue) return;

            bool exists = await _context.Employeeskills
                .AnyAsync(es => es.EmployeeId == employeeId && es.SkillId == skillId.Value);
            if (!exists)
            {
                _context.Employeeskills.Add(new Employeeskill
                {
                    EmployeeId = employeeId,
                    SkillId = skillId.Value,
                    CreatedAt = DateTime.Now
                });
            }
        }

        private double CalculateQuestionScore(Question q, string? answer)
        {
            if (q.QuestionOptions.Any())
            {
                var correctSet = q.QuestionOptions.Where(o => o.IsCorrect)
                                                  .Select(o => o.OptionId.ToString())
                                                  .ToHashSet();
                var selectedSet = (answer ?? "")
                                  .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(s => s.Trim())
                                  .ToHashSet();
                return correctSet.SetEquals(selectedSet) ? 1.0 : 0.0;
            }
            else if (q.QuestionTrueFalse != null)
            {
                if (bool.TryParse(answer, out var val))
                    return val == q.QuestionTrueFalse.CorrectAnswer ? 1.0 : 0.0;
            }
            else if (q.QuestionMatchings.Any())
            {
                var pairs = answer?.Split('|', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                var matchings = q.QuestionMatchings.ToList();
                int correct = 0;
                for (int i = 0; i < matchings.Count && i < pairs.Length; i++)
                {
                    if (pairs[i].Contains($"{matchings[i].LeftItem}->{matchings[i].RightItem}"))
                        correct++;
                }
                return matchings.Count > 0 ? (double)correct / matchings.Count : 0.0;
            }
            else if (q.QuestionDragDrops.Any())
            {
                var items = q.QuestionDragDrops.ToList();
                int correct = items.Count(it => answer != null && answer.Contains($"{it.DraggableText}->{it.DropTarget}"));
                return items.Count > 0 ? (double)correct / items.Count : 0.0;
            }

            return -1; // Essay
        }
        #endregion

        #region StartTest
        public async Task<IActionResult> StartTest(int testId)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Home");

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
            var test = await _context.Tests.FindAsync(empTest.TestId);
            ViewBag.DurationMinutes = test?.DurationMinutes ?? 0;

            ViewBag.RemainingSeconds = 0;
            if (empTest.StartTime.HasValue && test?.DurationMinutes != null)
            {
                var remain = (empTest.StartTime.Value.AddMinutes(test.DurationMinutes.Value) - DateTime.Now).TotalSeconds;
                ViewBag.RemainingSeconds = remain < 0 ? 0 : (int)remain;
            }

            return View(questions);
        }
        #endregion

        #region SubmitTest
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

            var savedAnswers = await _context.Employeeanswers
                .Where(a => a.EmployeeTestId == EmployeeTestId)
                .ToListAsync();

            double rawScore = 0;
            int totalQuestions = empTest.Test.Testquestions.Count;

            foreach (var tq in empTest.Test.Testquestions)
            {
                var q = tq.Question;
                var userAns = savedAnswers.FirstOrDefault(a => a.QuestionId == q.QuestionId);
                if (userAns == null) continue;

                double qScore = CalculateQuestionScore(q, userAns.Answer);
                userAns.Score = qScore;
                rawScore += qScore;

                // 🔹 Gọi helper cập nhật skill nếu trả lời đúng
                if (qScore > 0)
                {
                    await AddEmployeeSkillIfNotExists(empTest.EmployeeId, q.SkillId);
                }
            }

            empTest.TotalScore = totalQuestions > 0 ? Math.Round(rawScore / totalQuestions * 10.0, 2) : 0.0;
            empTest.EndTime = DateTime.Now;
            empTest.Status = empTest.StartTime.HasValue && empTest.Test.DurationMinutes.HasValue
                ? (DateTime.Now > empTest.StartTime.Value.AddMinutes(empTest.Test.DurationMinutes.Value) ? "Timeout" : "Completed")
                : "Completed";

            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, score = empTest.TotalScore });

            return RedirectToAction("UserIndex", "Home");
        }
        #endregion

        #region SaveAnswer
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

            var tq = empTest.Test.Testquestions.FirstOrDefault(t => t.QuestionId == questionId);
            double qScore = tq != null ? CalculateQuestionScore(tq.Question, answer) : 0.0;
            existing.Score = qScore;

            if (qScore > 0 && tq?.Question.SkillId != null)
            {
                await AddEmployeeSkillIfNotExists(empTest.EmployeeId, tq.Question.SkillId);
            }

            var allAnswers = await _context.Employeeanswers
                 .Where(a => a.EmployeeTestId == employeeTestId)
                 .ToListAsync();

            int totalQuestions = empTest.Test.Testquestions.Count;
            double rawScore = allAnswers.Sum(a => a.Score ?? 0);
            empTest.TotalScore = totalQuestions > 0 ? Math.Round(rawScore / totalQuestions * 10.0, 2) : 0.0;

            await _context.SaveChangesAsync();

            return Json(new { success = true, qScore, total10 = empTest.TotalScore });
        }
        #endregion

        #region Standard CRUD
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Employeetests.Include(e => e.Employee).Include(e => e.Test);
            return View(await appDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employeetest = await _context.Employeetests
                .Include(e => e.Employee)
                .Include(e => e.Test)
                .FirstOrDefaultAsync(m => m.EmployeeTestId == id);
            if (employeetest == null) return NotFound();

            return View(employeetest);
        }

        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId");
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId");
            return View();
        }

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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employeetest = await _context.Employeetests.FindAsync(id);
            if (employeetest == null) return NotFound();

            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", employeetest.EmployeeId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", employeetest.TestId);
            return View(employeetest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeTestId,EmployeeId,TestId,StartTime,EndTime,Status,TotalScore")] Employeetest employeetest)
        {
            if (id != employeetest.EmployeeTestId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeetest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeetestExists(employeetest.EmployeeTestId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", employeetest.EmployeeId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", employeetest.TestId);
            return View(employeetest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employeetest = await _context.Employeetests
                .Include(e => e.Employee)
                .Include(e => e.Test)
                .FirstOrDefaultAsync(m => m.EmployeeTestId == id);
            if (employeetest == null) return NotFound();

            return View(employeetest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employeetest = await _context.Employeetests.FindAsync(id);
            if (employeetest != null) _context.Employeetests.Remove(employeetest);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeetestExists(int id)
        {
            return _context.Employeetests.Any(e => e.EmployeeTestId == id);
        }
        #endregion
    }
}
