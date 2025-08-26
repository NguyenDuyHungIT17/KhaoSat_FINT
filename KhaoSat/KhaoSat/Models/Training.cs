using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("TRAININGS")]
public partial class Training
{
    [Key]
    public int TrainingId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? Provider { get; set; }

    public DateOnly? Date { get; set; }

    [InverseProperty("Training")]
    public virtual ICollection<Employeetraining> Employeetrainings { get; set; } = new List<Employeetraining>();
}
