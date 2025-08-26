using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("SKILLS")]
public partial class Skill
{
    [Key]
    public int SkillId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string? Level { get; set; }

    [InverseProperty("Skill")]
    public virtual ICollection<Employeeskill> Employeeskills { get; set; } = new List<Employeeskill>();

    [InverseProperty("Skill")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
