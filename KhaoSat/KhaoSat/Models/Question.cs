using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("QUESTIONS")]
public partial class Question
{
    [Key]
    public int QuestionId { get; set; }

    public string Content { get; set; } = null!;

    [StringLength(50)]
    public string? Type { get; set; }

    public int? SkillId { get; set; }

    [StringLength(50)]
    public string? Difficulty { get; set; }

    [StringLength(100)]
    public string? Role { get; set; }

    [InverseProperty("Question")]
    public virtual ICollection<Employeeanswer> Employeeanswers { get; set; } = new List<Employeeanswer>();

    [InverseProperty("Question")]
    public virtual ICollection<QuestionDragDrop> QuestionDragDrops { get; set; } = new List<QuestionDragDrop>();

    [InverseProperty("Question")]
    public virtual ICollection<QuestionMatching> QuestionMatchings { get; set; } = new List<QuestionMatching>();

    [InverseProperty("Question")]
    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();

    [InverseProperty("Question")]
    public virtual QuestionTrueFalse? QuestionTrueFalse { get; set; }

    [ForeignKey("SkillId")]
    [InverseProperty("Questions")]
    public virtual Skill? Skill { get; set; }

    [InverseProperty("Question")]
    public virtual ICollection<Testquestion> Testquestions { get; set; } = new List<Testquestion>();
}
