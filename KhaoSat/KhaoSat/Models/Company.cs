using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("COMPANIES")]
public partial class Company
{
    [Key]
    public int CompanyId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? Industry { get; set; }

    [InverseProperty("Company")]
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
}
