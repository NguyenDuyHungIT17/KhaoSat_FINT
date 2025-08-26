using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("NOTIFICATIONS")]
public partial class Notification
{
    [Key]
    public int NotificationId { get; set; }

    public int EmployeeId { get; set; }

    public string? Message { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("Notifications")]
    public virtual Employee Employee { get; set; } = null!;
}
