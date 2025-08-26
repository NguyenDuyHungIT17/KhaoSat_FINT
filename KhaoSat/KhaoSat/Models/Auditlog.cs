using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("AUDITLOGS")]
public partial class Auditlog
{
    [Key]
    public int AuditLogId { get; set; }

    public int EmployeeId { get; set; }

    [StringLength(255)]
    public string? Action { get; set; }

    [StringLength(255)]
    public string? Target { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Timestamp { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("Auditlogs")]
    public virtual Employee Employee { get; set; } = null!;
}
