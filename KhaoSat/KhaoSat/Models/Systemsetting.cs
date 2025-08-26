using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("SYSTEMSETTINGS")]
[Index("Key", Name = "UQ__SYSTEMSE__C41E0289461E93B7", IsUnique = true)]
public partial class Systemsetting
{
    [Key]
    public int SettingId { get; set; }

    [StringLength(100)]
    public string Key { get; set; } = null!;

    [StringLength(255)]
    public string? Value { get; set; }
}
