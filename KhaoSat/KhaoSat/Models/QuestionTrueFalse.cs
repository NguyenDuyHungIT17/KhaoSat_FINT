using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("QuestionTrueFalse")]
public partial class QuestionTrueFalse
{
    [Key]
    public int QuestionId { get; set; }

    public bool CorrectAnswer { get; set; }

    [ForeignKey("QuestionId")]
    [InverseProperty("QuestionTrueFalse")]
    public virtual Question Question { get; set; } = null!;
}
