using KhaoSat.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Controllers
{
    public class EmployeeanswersController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeeanswersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Employeeanswers
        // Danh sách câu hỏi tự luận chưa chấm
        public async Task<IActionResult> Index()
        {
            var answers = await _context.Employeeanswers
                .Include(e => e.EmployeeTest)
                    .ThenInclude(et => et.Employee)
                .Include(e => e.EmployeeTest)
                    .ThenInclude(et => et.Test)
                .Include(e => e.Question)
                .Where(e => e.Question.Type.ToLower() == "essay" && e.Score == -1)
                .ToListAsync();

            return View(answers);
        }

        // GET: Employeeanswers/Grade?employeeTestId=1&questionId=2
        // Form chấm điểm câu hỏi tự luận
        public async Task<IActionResult> Grade(int employeeTestId, int questionId)
        {
            var answer = await _context.Employeeanswers
                .Include(e => e.EmployeeTest)
                    .ThenInclude(et => et.Employee)
                .Include(e => e.EmployeeTest)
                    .ThenInclude(et => et.Test)
                .Include(e => e.Question)
                .FirstOrDefaultAsync(e => e.EmployeeTestId == employeeTestId
                                       && e.QuestionId == questionId);

            if (answer == null)
                return NotFound();

            return View(answer);
        }

        // POST: Employeeanswers/GradeConfirmed
        // Lưu điểm câu hỏi tự luận (Đúng = 1, Sai = 0)
        // POST: Employeeanswers/GradeConfirmed
        // POST: Employeeanswers/GradeConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeConfirmed(int employeeTestId, int questionId, bool isCorrect)
        {
            var answer = await _context.Employeeanswers
                .FirstOrDefaultAsync(e => e.EmployeeTestId == employeeTestId
                                       && e.QuestionId == questionId);

            if (answer == null) return NotFound();

            // Chấm điểm câu hỏi: đúng = 1, sai = 0
            answer.Score = isCorrect ? 1.0 : 0.0;
            _context.Update(answer);

            // 🔹 Lấy bài test
            var empTest = await _context.Employeetests
                .Include(et => et.Test)
                    .ThenInclude(t => t.Testquestions)
                .FirstOrDefaultAsync(et => et.EmployeeTestId == employeeTestId);

            if (empTest != null)
            {
                // 🔹 Lấy tất cả câu trả lời đã lưu
                var allAnswers = await _context.Employeeanswers
                     .Where(a => a.EmployeeTestId == employeeTestId)
                     .ToListAsync();

                int totalQuestions = empTest.Test.Testquestions.Count;

                // Fix lỗi nullable: câu chưa chấm có Score = -1 → tính là 0
                double rawScore = allAnswers.Sum(a => a.Score > 0 ? a.Score.Value : 0);

                double total10 = totalQuestions > 0
                    ? Math.Round(rawScore / totalQuestions * 10.0, 2)
                    : 0.0;

                empTest.TotalScore = total10;
                _context.Update(empTest);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
