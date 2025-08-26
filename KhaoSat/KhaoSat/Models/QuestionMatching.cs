using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("QuestionMatching")]
public partial class QuestionMatching
{
    [Key]
    public int PairId { get; set; }

    public int QuestionId { get; set; }

    public string? LeftItem { get; set; }

    public string? RightItem { get; set; }

    [ForeignKey("QuestionId")]
    [InverseProperty("QuestionMatchings")]
    public virtual Question Question { get; set; } = null!;
}
