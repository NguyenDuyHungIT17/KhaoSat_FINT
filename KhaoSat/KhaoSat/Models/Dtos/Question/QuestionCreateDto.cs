namespace KhaoSat.Models.Dtos.Question
{
    public class QuestionCreateDto
    {
        public string Content { get; set; } = null!;
        public string? Type { get; set; }
        public int? SkillId { get; set; }
        public string? Difficulty { get; set; }
        public string? Role { get; set; }

        // Các chi tiết tùy loại
        public List<QuestionOptionDto>? Options { get; set; }
        public QuestionTrueFalseDto? TrueFalse { get; set; }
        public List<QuestionMatchingDto>? Matchings { get; set; }
        public List<QuestionDragDropDto>? DragDrops { get; set; }
    }

    public class QuestionOptionDto
    {
        public string? OptionText { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuestionTrueFalseDto
    {
        public bool CorrectAnswer { get; set; }
    }

    public class QuestionMatchingDto
    {
        public string? LeftItem { get; set; }
        public string? RightItem { get; set; }
    }

    public class QuestionDragDropDto
    {
        public string? DraggableText { get; set; }
        public string? DropTarget { get; set; }
    }
}
