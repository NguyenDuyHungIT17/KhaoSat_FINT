using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[PrimaryKey("EmployeeId", "RoleId")]
[Table("EMPLOYEEROLES")]
public partial class Employeerole
{
    [Key]
    public int EmployeeId { get; set; }

    [Key]
    public int RoleId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("Employeeroles")]
    public virtual Employee Employee { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("Employeeroles")]
    public virtual Role Role { get; set; } = null!;
}
