using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("LEVELS")]
public partial class Level
{
    [Key]
    public int LevelId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    [InverseProperty("Level")]
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
