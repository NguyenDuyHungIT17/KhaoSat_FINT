using KhaoSat.Models;
using KhaoSat.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace KhaoSat.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly AppDbContext _context;

        public QuestionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Question/Index
        public async Task<IActionResult> Index()
        {
            var questions = await _context.Questions
                .Include(q => q.Skill)
                .ToListAsync();
            return View(questions);
        }

        // GET: Question/Create
        public IActionResult Create()
        {
            return View(new QuestionCreateViewModel());
        }

        // POST: Question/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionCreateViewModel vm)
        {
            if (vm.ExcelFile != null && vm.ExcelFile.Length > 0)
            {
              

                // Import từ Excel
                using (var stream = new MemoryStream())
                {
                    await vm.ExcelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            ModelState.AddModelError("", "Không tìm thấy sheet trong file Excel.");
                            return View(vm);
                        }

                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++) // bỏ dòng header
                        {
                            var content = worksheet.Cells[row, 1].Text?.Trim();
                            var type = worksheet.Cells[row, 2].Text?.Trim();
                            var difficulty = worksheet.Cells[row, 3].Text?.Trim();
                            var role = worksheet.Cells[row, 4].Text?.Trim();
                            var skillName = worksheet.Cells[row, 5].Text?.Trim();

                            if (string.IsNullOrWhiteSpace(content)) continue;

                            // 🔹 Tìm hoặc tạo skill
                            Skill? skill = null;
                            if (!string.IsNullOrEmpty(skillName))
                            {
                                skill = await _context.Skills.FirstOrDefaultAsync(s => s.Name == skillName);
                                if (skill == null)
                                {
                                    skill = new Skill { Name = skillName };
                                    _context.Skills.Add(skill);
                                    await _context.SaveChangesAsync();
                                }
                            }

                            var question = new Question
                            {
                                Content = content,
                                Type = type,
                                Difficulty = difficulty,
                                Role = role,
                                SkillId = skill?.SkillId
                            };

                            _context.Questions.Add(question);
                            await _context.SaveChangesAsync();

                            // 🔹 MCQ
                            if (type == "MCQ")
                            {
                                string optionA = worksheet.Cells[row, 6].Text?.Trim();
                                string optionB = worksheet.Cells[row, 7].Text?.Trim();
                                string optionC = worksheet.Cells[row, 8].Text?.Trim();
                                string optionD = worksheet.Cells[row, 9].Text?.Trim();
                                string correct = worksheet.Cells[row, 10].Text?.Trim(); // ví dụ: A,B

                                var options = new List<(string Text, string Key)> {
                                    (optionA, "A"),
                                    (optionB, "B"),
                                    (optionC, "C"),
                                    (optionD, "D")
                                };

                                foreach (var opt in options)
                                {
                                    if (!string.IsNullOrWhiteSpace(opt.Text))
                                    {
                                        _context.QuestionOptions.Add(new QuestionOption
                                        {
                                            QuestionId = question.QuestionId,
                                            OptionText = opt.Text,
                                            IsCorrect = correct?.Split(',').Select(x => x.Trim().ToUpper()).Contains(opt.Key) ?? false
                                        });
                                    }
                                }
                            }
                            // 🔹 True/False
                            else if (type == "TrueFalse")
                            {
                                var ans = worksheet.Cells[row, 11].Text?.Trim().ToLower(); // cột TrueFalseAnswer
                                bool? correct = ans == "true" ? true : ans == "false" ? false : null;

                                if (correct.HasValue)
                                {
                                    _context.QuestionTrueFalses.Add(new QuestionTrueFalse
                                    {
                                        QuestionId = question.QuestionId,
                                        CorrectAnswer = correct.Value
                                    });
                                }
                            }
                            // 🔹 Matching
                            else if (type == "Matching")
                            {
                                for (int c = 6; c <= 9; c += 2)
                                {
                                    var left = worksheet.Cells[row, c].Text?.Trim();
                                    var right = worksheet.Cells[row, c + 1].Text?.Trim();
                                    if (!string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right))
                                    {
                                        _context.QuestionMatchings.Add(new QuestionMatching
                                        {
                                            QuestionId = question.QuestionId,
                                            LeftItem = left,
                                            RightItem = right
                                        });
                                    }
                                }
                            }
                            // 🔹 DragDrop
                            else if (type == "DragDrop")
                            {
                                for (int c = 6; c <= 9; c += 2)
                                {
                                    var drag = worksheet.Cells[row, c].Text?.Trim();
                                    var drop = worksheet.Cells[row, c + 1].Text?.Trim();
                                    if (!string.IsNullOrWhiteSpace(drag) && !string.IsNullOrWhiteSpace(drop))
                                    {
                                        _context.QuestionDragDrops.Add(new QuestionDragDrop
                                        {
                                            QuestionId = question.QuestionId,
                                            DraggableText = drag,
                                            DropTarget = drop
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Thêm thủ công
                if (!ModelState.IsValid)
                    return View(vm);

                Skill? skill = null;
                if (!string.IsNullOrWhiteSpace(vm.SkillName))
                {
                    skill = await _context.Skills.FirstOrDefaultAsync(s => s.Name == vm.SkillName);
                    if (skill == null)
                    {
                        skill = new Skill { Name = vm.SkillName };
                        _context.Skills.Add(skill);
                        await _context.SaveChangesAsync();
                    }
                }

                var question = new Question
                {
                    Content = vm.Content,
                    Type = vm.Type,
                    Difficulty = vm.Difficulty,
                    Role = vm.Role,
                    SkillId = skill?.SkillId
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                if (vm.Type == "MCQ" && vm.Options.Any())
                {
                    foreach (var opt in vm.Options)
                    {
                        if (!string.IsNullOrWhiteSpace(opt.OptionText))
                        {
                            _context.QuestionOptions.Add(new QuestionOption
                            {
                                QuestionId = question.QuestionId,
                                OptionText = opt.OptionText,
                                IsCorrect = opt.IsCorrect
                            });
                        }
                    }
                }
                else if (vm.Type == "TrueFalse" && vm.TrueFalseAnswer.HasValue)
                {
                    _context.QuestionTrueFalses.Add(new QuestionTrueFalse
                    {
                        QuestionId = question.QuestionId,
                        CorrectAnswer = vm.TrueFalseAnswer.Value
                    });
                }
                else if (vm.Type == "Matching" && vm.Matchings.Any())
                {
                    foreach (var match in vm.Matchings)
                    {
                        _context.QuestionMatchings.Add(new QuestionMatching
                        {
                            QuestionId = question.QuestionId,
                            LeftItem = match.LeftItem,
                            RightItem = match.RightItem
                        });
                    }
                }
                else if (vm.Type == "DragDrop" && vm.DragDrops.Any())
                {
                    foreach (var dd in vm.DragDrops)
                    {
                        _context.QuestionDragDrops.Add(new QuestionDragDrop
                        {
                            QuestionId = question.QuestionId,
                            DraggableText = dd.DraggableText,
                            DropTarget = dd.DropTarget
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Question/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Skill)
                .Include(q => q.QuestionOptions)
                .Include(q => q.QuestionTrueFalse)
                .Include(q => q.QuestionMatchings)
                .Include(q => q.QuestionDragDrops)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null) return NotFound();

            var vm = new QuestionEditViewModel
            {
                QuestionId = question.QuestionId,
                Content = question.Content,
                Type = question.Type,
                SkillId = question.SkillId,
                SkillName = question.Skill?.Name,
                Difficulty = question.Difficulty,
                Role = question.Role,
                Options = question.QuestionOptions?.Select(o => new QuestionOptionViewModel
                {
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                }).ToList(),
                TrueFalseAnswer = question.QuestionTrueFalse?.CorrectAnswer,
                Matchings = question.QuestionMatchings?.Select(m => new QuestionMatchingViewModel
                {
                    LeftItem = m.LeftItem,
                    RightItem = m.RightItem
                }).ToList(),
                DragDrops = question.QuestionDragDrops?.Select(d => new QuestionDragDropViewModel
                {
                    DraggableText = d.DraggableText,
                    DropTarget = d.DropTarget
                }).ToList()
            };

            return View(vm);
        }

        // POST: Question/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuestionEditViewModel vm)
        {
            var question = await _context.Questions
                .Include(q => q.QuestionOptions)
                .Include(q => q.QuestionTrueFalse)
                .Include(q => q.QuestionMatchings)
                .Include(q => q.QuestionDragDrops)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null) return NotFound();

            // Skill
            Skill? skill = null;
            if (!string.IsNullOrWhiteSpace(vm.SkillName))
            {
                skill = await _context.Skills.FirstOrDefaultAsync(s => s.Name == vm.SkillName);
                if (skill == null)
                {
                    skill = new Skill { Name = vm.SkillName };
                    _context.Skills.Add(skill);
                    await _context.SaveChangesAsync();
                }
            }

            question.Content = vm.Content;
            question.Type = vm.Type;
            question.SkillId = skill?.SkillId;
            question.Difficulty = vm.Difficulty;
            question.Role = vm.Role;

            // Xoá dữ liệu cũ
            _context.QuestionOptions.RemoveRange(question.QuestionOptions);
            _context.QuestionMatchings.RemoveRange(question.QuestionMatchings);
            _context.QuestionDragDrops.RemoveRange(question.QuestionDragDrops);
            if (question.QuestionTrueFalse != null)
                _context.QuestionTrueFalses.Remove(question.QuestionTrueFalse);

            await _context.SaveChangesAsync();

            // Thêm mới
            if (vm.Type == "MCQ" && vm.Options != null)
            {
                foreach (var opt in vm.Options)
                {
                    _context.QuestionOptions.Add(new QuestionOption
                    {
                        QuestionId = question.QuestionId,
                        OptionText = opt.OptionText,
                        IsCorrect = opt.IsCorrect
                    });
                }
            }
            else if (vm.Type == "TrueFalse" && vm.TrueFalseAnswer.HasValue)
            {
                _context.QuestionTrueFalses.Add(new QuestionTrueFalse
                {
                    QuestionId = question.QuestionId,
                    CorrectAnswer = vm.TrueFalseAnswer.Value
                });
            }
            else if (vm.Type == "Matching" && vm.Matchings != null)
            {
                foreach (var match in vm.Matchings)
                {
                    _context.QuestionMatchings.Add(new QuestionMatching
                    {
                        QuestionId = question.QuestionId,
                        LeftItem = match.LeftItem,
                        RightItem = match.RightItem
                    });
                }
            }
            else if (vm.Type == "DragDrop" && vm.DragDrops != null)
            {
                foreach (var dd in vm.DragDrops)
                {
                    _context.QuestionDragDrops.Add(new QuestionDragDrop
                    {
                        QuestionId = question.QuestionId,
                        DraggableText = dd.DraggableText,
                        DropTarget = dd.DropTarget
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Question/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null) return NotFound();

            return View(question);
        }

        // POST: Question/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Questions
                .Include(q => q.QuestionOptions)
                .Include(q => q.QuestionTrueFalse)
                .Include(q => q.QuestionMatchings)
                .Include(q => q.QuestionDragDrops)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null) return NotFound();

            _context.QuestionOptions.RemoveRange(question.QuestionOptions);
            _context.QuestionMatchings.RemoveRange(question.QuestionMatchings);
            _context.QuestionDragDrops.RemoveRange(question.QuestionDragDrops);
            if (question.QuestionTrueFalse != null)
                _context.QuestionTrueFalses.Remove(question.QuestionTrueFalse);

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
