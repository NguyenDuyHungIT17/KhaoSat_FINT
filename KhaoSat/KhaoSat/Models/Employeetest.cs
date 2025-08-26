using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("EMPLOYEETESTS")]
public partial class Employeetest
{
    [Key]
    public int EmployeeTestId { get; set; }

    public int EmployeeId { get; set; }

    public int TestId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? StartTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EndTime { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public double? TotalScore { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("Employeetests")]
    public virtual Employee Employee { get; set; } = null!;

    [InverseProperty("EmployeeTest")]
    public virtual ICollection<Employeeanswer> Employeeanswers { get; set; } = new List<Employeeanswer>();

    [ForeignKey("TestId")]
    [InverseProperty("Employeetests")]
    public virtual Test Test { get; set; } = null!;
}
