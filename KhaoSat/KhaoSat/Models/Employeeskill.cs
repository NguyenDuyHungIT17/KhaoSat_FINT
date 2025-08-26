using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[PrimaryKey("EmployeeId", "SkillId")]
[Table("EMPLOYEESKILLS")]
public partial class Employeeskill
{
    [Key]
    public int EmployeeId { get; set; }

    [Key]
    public int SkillId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("Employeeskills")]
    public virtual Employee Employee { get; set; } = null!;

    [ForeignKey("SkillId")]
    [InverseProperty("Employeeskills")]
    public virtual Skill Skill { get; set; } = null!;
}
