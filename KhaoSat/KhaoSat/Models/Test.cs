using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("TESTS")]
public partial class Test
{
    [Key]
    public int TestId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string? Type { get; set; }

    public int? DurationMinutes { get; set; }

    public double? PassingScore { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Tests")]
    public virtual Employee? CreatedByNavigation { get; set; }

    [InverseProperty("Test")]
    public virtual ICollection<Employeetest> Employeetests { get; set; } = new List<Employeetest>();

    [InverseProperty("Test")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    [InverseProperty("Test")]
    public virtual ICollection<Testquestion> Testquestions { get; set; } = new List<Testquestion>();
}
