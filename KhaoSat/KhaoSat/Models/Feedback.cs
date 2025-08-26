using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhaoSat.Models;

[Table("FEEDBACKS")]
public partial class Feedback
{
    [Key]
    public int FeedbackId { get; set; }

    public int EmployeeId { get; set; }

    public int TestId { get; set; }

    public string? Content { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("Feedbacks")]
    public virtual Employee Employee { get; set; } = null!;

    [ForeignKey("TestId")]
    [InverseProperty("Feedbacks")]
    public virtual Test Test { get; set; } = null!;
}
 