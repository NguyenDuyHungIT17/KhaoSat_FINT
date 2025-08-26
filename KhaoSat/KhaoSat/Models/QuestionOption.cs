using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

public partial class QuestionOption
{
    [Key]
    public int OptionId { get; set; }

    public int QuestionId { get; set; }

    public string? OptionText { get; set; }

    public bool IsCorrect { get; set; }

    [ForeignKey("QuestionId")]
    [InverseProperty("QuestionOptions")]
    public virtual Question Question { get; set; } = null!;
}
