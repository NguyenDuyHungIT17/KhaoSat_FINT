using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[PrimaryKey("EmployeeId", "TrainingId")]
[Table("EMPLOYEETRAININGS")]
public partial class Employeetraining
{
    [Key]
    public int EmployeeId { get; set; }

    [Key]
    public int TrainingId { get; set; }

    [StringLength(100)]
    public string? Result { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("Employeetrainings")]
    public virtual Employee Employee { get; set; } = null!;

    [ForeignKey("TrainingId")]
    [InverseProperty("Employeetrainings")]
    public virtual Training Training { get; set; } = null!;
}
