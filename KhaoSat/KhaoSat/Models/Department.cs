using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("DEPARTMENTS")]
public partial class Department
{
    [Key]
    public int DepartmentId { get; set; }

    public int CompanyId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [ForeignKey("CompanyId")]
    [InverseProperty("Departments")]
    public virtual Company Company { get; set; } = null!;

    [InverseProperty("Department")]
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
