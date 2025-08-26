using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

[Table("EMPLOYEES")]
[Index("Email", Name = "UQ__EMPLOYEE__A9D10534DE4C5D70", IsUnique = true)]
public partial class Employee
{
    [Key]
    public int EmployeeId { get; set; }

    public int DepartmentId { get; set; }

    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    public DateTime? DateOfBirth { get; set; }

    public DateTime? HireDate { get; set; }

    [StringLength(255)]
    public string Password { get; set; } = null!;

    public int? LevelId { get; set; }

    [InverseProperty("Employee")]
    public virtual ICollection<Auditlog> Auditlogs { get; set; } = new List<Auditlog>();

    [ForeignKey("DepartmentId")]
    [InverseProperty("Employees")]
    public virtual Department Department { get; set; } = null!;

    [InverseProperty("Employee")]
    public virtual ICollection<Employeerole> Employeeroles { get; set; } = new List<Employeerole>();

    [InverseProperty("Employee")]
    public virtual ICollection<Employeeskill> Employeeskills { get; set; } = new List<Employeeskill>();

    [InverseProperty("Employee")]
    public virtual ICollection<Employeetest> Employeetests { get; set; } = new List<Employeetest>();

    [InverseProperty("Employee")]
    public virtual ICollection<Employeetraining> Employeetrainings { get; set; } = new List<Employeetraining>();

    [InverseProperty("Employee")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    [ForeignKey("LevelId")]
    [InverseProperty("Employees")]
    public virtual Level? Level { get; set; }

    [InverseProperty("Employee")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
