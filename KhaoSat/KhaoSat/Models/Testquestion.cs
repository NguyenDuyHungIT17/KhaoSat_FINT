using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[PrimaryKey("TestId", "QuestionId")]
[Table("TESTQUESTIONS")]
public partial class Testquestion
{
    [Key]
    public int TestId { get; set; }

    [Key]
    public int QuestionId { get; set; }

    public int? QuestionOrder { get; set; }

    [ForeignKey("QuestionId")]
    [InverseProperty("Testquestions")]
    public virtual Question Question { get; set; } = null!;

    [ForeignKey("TestId")]
    [InverseProperty("Testquestions")]
    public virtual Test Test { get; set; } = null!;
}
