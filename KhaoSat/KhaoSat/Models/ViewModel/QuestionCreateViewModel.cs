namespace KhaoSat.Models.ViewModel
{
    public class QuestionCreateViewModel
    {
        public string Content { get; set; } = null!;

        public string? Type { get; set; }   // MCQ, TrueFalse, Matching, DragDrop...

        public int? SkillId { get; set; }
        public string? SkillName { get; set; } // nhập text skill

        public string? Difficulty { get; set; }
        public string? Role { get; set; }

        // Multiple Choice
        public List<QuestionOptionViewModel> Options { get; set; } = new();

        // True/False
        public bool? TrueFalseAnswer { get; set; }

        // Matching
        public List<QuestionMatchingViewModel> Matchings { get; set; } = new();

        // DragDrop
        public List<QuestionDragDropViewModel> DragDrops { get; set; } = new();
        public IFormFile? ExcelFile { get; set; }
    }

    public class QuestionEditViewModel : QuestionCreateViewModel
    {
        public int QuestionId { get; set; }
    }
    public class QuestionOptionViewModel
    {
        public string? OptionText { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuestionMatchingViewModel
    {
        public string? LeftItem { get; set; }
        public string? RightItem { get; set; }
    }

    public class QuestionDragDropViewModel
    {
        public string? DraggableText { get; set; }
        public string? DropTarget { get; set; }
    }


}
