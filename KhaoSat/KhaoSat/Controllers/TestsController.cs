using KhaoSat.Models;
using KhaoSat.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KhaoSat.Controllers
{
    public class TestsController : Controller
    {
        private readonly AppDbContext _context;

        public TestsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Tests
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Tests.Include(t => t.CreatedByNavigation);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Tests/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var test = await _context.Tests
                .Include(t => t.CreatedByNavigation) // load người tạo
                .Include(t => t.Testquestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.QuestionOptions)
                .Include(t => t.Testquestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.QuestionTrueFalse)
                .Include(t => t.Testquestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.QuestionMatchings)
                .Include(t => t.Testquestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.QuestionDragDrops)
                .FirstOrDefaultAsync(t => t.TestId == id);

            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }


        // GET: Tests/Create
        public IActionResult Create()
        {
            // Dropdown Type
            ViewBag.Roles = _context.Roles
                .Where(r => r.RoleId != 1)
                .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                .ToList();

            // Dropdown CreatedBy
            ViewBag.CreatedBy = (from e in _context.Employees
                                 join er in _context.Employeeroles on e.EmployeeId equals er.EmployeeId
                                 join r in _context.Roles on er.RoleId equals r.RoleId
                                 where r.RoleId == 1
                                 select new SelectListItem
                                 {
                                     Value = e.EmployeeId.ToString(),
                                     Text = e.FullName
                                 }).ToList();

            // Danh sách câu hỏi từ ngân hàng câu hỏi
            ViewBag.Questions = _context.Questions
                .Select(q => new
                {
                    q.QuestionId,
                    q.Content
                }).ToList();

            return View();
        }

        // POST: Tests/Create
        [HttpPost]
        public IActionResult Create(TestCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Tạo Test
                var test = new Test
                {
                    Name = model.TestName,
                    Type = model.Type,
                    DurationMinutes = model.DurationMinutes,
                    PassingScore = model.PassingScore,
                    CreatedBy = model.CreatedBy
                };

                _context.Tests.Add(test);
                _context.SaveChanges();

                // 2. Thêm các câu hỏi được chọn (nếu có)
                if (model.SelectedQuestions != null && model.SelectedQuestions.Any())
                {
                    foreach (var qId in model.SelectedQuestions)
                    {
                        var tq = new Testquestion
                        {
                            TestId = test.TestId,
                            QuestionId = qId
                        };
                        _context.Testquestions.Add(tq);
                    }
                    _context.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            // Nếu có lỗi validation thì load lại ViewBag
            ViewBag.Questions = _context.Questions
                .Select(q => new { q.QuestionId, q.Content })
                .ToList();

            return View(model);
        }

        //auto create
        // GET: Tests/AutoCreate
        public IActionResult AutoCreate()
        {
            // Dropdown Role
            ViewBag.Roles = _context.Roles
                .Where(r => r.RoleId != 1)
                .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                .ToList();

            // Dropdown CreatedBy
            ViewBag.CreatedBy = _context.Employees
                .Select(e => new SelectListItem
                {
                    Value = e.EmployeeId.ToString(),
                    Text = e.FullName
                }).ToList();

            return View();
        }

        // POST: Tests/AutoCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AutoCreate(string testName, string role, string difficulty, int soCauHoi, int durationMinutes = 60, double passingScore = 70, int createdBy = 1)
        {
            if (string.IsNullOrWhiteSpace(testName) || string.IsNullOrWhiteSpace(role) || soCauHoi <= 0)
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }

            // 1. Tạo bài test mới
            var test = new Test
            {
                Name = testName,
                Type = role,
                DurationMinutes = durationMinutes,
                PassingScore = passingScore,
                CreatedBy = createdBy
            };
            _context.Tests.Add(test);
            _context.SaveChanges(); // để có TestId

            // 2. Lấy ngẫu nhiên N câu hỏi theo Role + Difficulty
            var questions = _context.Questions
                .Where(q => q.Role != null && q.Role.ToLower() == role.ToLower()
                            && (q.Difficulty == null || q.Difficulty.ToLower() == difficulty.ToLower()))
                .OrderBy(q => Guid.NewGuid())
                .Take(soCauHoi)
                .ToList();

            // 3. Thêm vào Testquestions
            int order = 1;
            foreach (var q in questions)
            {
                var tq = new Testquestion
                {
                    TestId = test.TestId,
                    QuestionId = q.QuestionId,
                    QuestionOrder = order++
                };
                _context.Testquestions.Add(tq);
            }
            _context.SaveChanges();

            // 4. Chuyển đến trang chi tiết test vừa tạo
            return RedirectToAction("Index");
        }


        // GET: Tests/Edit/5
        // GET: Tests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var test = await _context.Tests.FindAsync(id);
            if (test == null) return NotFound();

            // Chỉ lấy admin
            ViewBag.CreatedBy = _context.Employees
                .Join(_context.Employeeroles, e => e.EmployeeId, er => er.EmployeeId, (e, er) => new { e, er })
                .Join(_context.Roles, x => x.er.RoleId, r => r.RoleId, (x, r) => new { x.e, r })
                .Where(x => x.r.RoleId == 1)
                .Select(x => new SelectListItem { Value = x.e.EmployeeId.ToString(), Text = x.e.FullName })
                .ToList();

            // Lấy danh sách câu hỏi
            ViewBag.Questions = _context.Questions
                .Select(q => new { q.QuestionId, q.Content })
                .ToList();

            // Lấy danh sách các QuestionId đã được chọn cho bài test này
            var selectedQuestions = _context.Testquestions
                .Where(tq => tq.TestId == test.TestId)
                .Select(tq => tq.QuestionId)
                .ToList();

            ViewBag.Roles = _context.Roles
                .Where(r => r.RoleId != 1)
                .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                .ToList();

            var model = new TestEditViewModel
            {
                TestId = test.TestId,
                TestName = test.Name,
                Type = test.Type,
                DurationMinutes = test.DurationMinutes,
                PassingScore = test.PassingScore,
                CreatedBy = test.CreatedBy,
                SelectedQuestions = selectedQuestions
            };

            return View(model);
        }

        // POST: Tests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TestEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload ViewBag nếu validation lỗi
                ViewBag.CreatedBy = _context.Employees
                    .Join(_context.Employeeroles, e => e.EmployeeId, er => er.EmployeeId, (e, er) => new { e, er })
                    .Join(_context.Roles, x => x.er.RoleId, r => r.RoleId, (x, r) => new { x.e, r })
                    .Where(x => x.r.RoleId == 1)
                    .Select(x => new SelectListItem { Value = x.e.EmployeeId.ToString(), Text = x.e.FullName })
                    .ToList();

                ViewBag.Questions = _context.Questions
                    .Select(q => new { q.QuestionId, q.Content })
                    .ToList();

                ViewBag.Roles = _context.Roles
                .Where(r => r.RoleId != 1)
                .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                .ToList();

                return View(model);
            }

            var test = await _context.Tests.FindAsync(model.TestId);
            if (test == null) return NotFound();

            test.Name = model.TestName;
            test.Type = model.Type;
            test.DurationMinutes = model.DurationMinutes;
            test.PassingScore = model.PassingScore;
            test.CreatedBy = model.CreatedBy;

            _context.Update(test);
            await _context.SaveChangesAsync();

            // Cập nhật câu hỏi
            var existingTQs = _context.Testquestions.Where(tq => tq.TestId == test.TestId).ToList();
            _context.Testquestions.RemoveRange(existingTQs);
            if (model.SelectedQuestions != null && model.SelectedQuestions.Any())
            {
                foreach (var qId in model.SelectedQuestions)
                {
                    _context.Testquestions.Add(new Testquestion
                    {
                        TestId = test.TestId,
                        QuestionId = qId
                    });
                }
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // GET: Tests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var test = await _context.Tests
                .Include(t => t.CreatedByNavigation)
                .FirstOrDefaultAsync(m => m.TestId == id);

            if (test == null) return NotFound();

            return View(test);
        }

        // POST: Tests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var test = await _context.Tests.FindAsync(id);
            if (test != null)
            {
                _context.Tests.Remove(test);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TestExists(int id)
        {
            return _context.Tests.Any(e => e.TestId == id);
        }
    }
}
