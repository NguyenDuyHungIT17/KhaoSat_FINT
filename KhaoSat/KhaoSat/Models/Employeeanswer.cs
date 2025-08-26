using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[PrimaryKey("EmployeeTestId", "QuestionId")]
[Table("EMPLOYEEANSWERS")]
public partial class Employeeanswer
{
    [Key]
    public int EmployeeTestId { get; set; }

    [Key]
    public int QuestionId { get; set; }

    public string? Answer { get; set; }

    public double? Score { get; set; }

    [ForeignKey("EmployeeTestId")]
    [InverseProperty("Employeeanswers")]
    public virtual Employeetest EmployeeTest { get; set; } = null!;

    [ForeignKey("QuestionId")]
    [InverseProperty("Employeeanswers")]
    public virtual Question Question { get; set; } = null!;
}
