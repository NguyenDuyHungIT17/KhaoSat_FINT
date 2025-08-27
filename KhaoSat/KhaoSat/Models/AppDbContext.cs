using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KhaoSat.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        
            // Tăng timeout lên 60 giây
            this.Database.SetCommandTimeout(120);
        
    }

    public virtual DbSet<Auditlog> Auditlogs { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Employeeanswer> Employeeanswers { get; set; }

    public virtual DbSet<Employeerole> Employeeroles { get; set; }

    public virtual DbSet<Employeeskill> Employeeskills { get; set; }

    public virtual DbSet<Employeetest> Employeetests { get; set; }

    public virtual DbSet<Employeetraining> Employeetrainings { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Level> Levels { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionDragDrop> QuestionDragDrops { get; set; }

    public virtual DbSet<QuestionMatching> QuestionMatchings { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    public virtual DbSet<QuestionTrueFalse> QuestionTrueFalses { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<Systemsetting> Systemsettings { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<Testquestion> Testquestions { get; set; }

    public virtual DbSet<Training> Trainings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=fintKhaoSat;User Id=sa;Password=Duyhung@18022004sqlserver;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auditlog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AUDITLOG__EB5F6CBD11EDF908");

            entity.Property(e => e.Timestamp).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Employee).WithMany(p => p.Auditlogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AUDITLOGS_EMPLOYEES");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PK__COMPANIE__2D971CAC0EA6120D");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__DEPARTME__B2079BED168DCBEF");

            entity.HasOne(d => d.Company).WithMany(p => p.Departments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DEPARTMENTS_COMPANIES");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__EMPLOYEE__7AD04F1110748962");

            entity.Property(e => e.Password).HasDefaultValue("");

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EMPLOYEES_DEPARTMENTS");

            entity.HasOne(d => d.Level).WithMany(p => p.Employees).HasConstraintName("FK_EMPLOYEES_LEVELS");
        });

        modelBuilder.Entity<Employeeanswer>(entity =>
        {
            entity.HasOne(d => d.EmployeeTest).WithMany(p => p.Employeeanswers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EMPLOYEEANSWERS_EMPLOYEETESTS");

            entity.HasOne(d => d.Question).WithMany(p => p.Employeeanswers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EMPLOYEEANSWERS_QUESTIONS");
        });

        modelBuilder.Entity<Employeerole>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Employee).WithMany(p => p.Employeeroles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EMPLOYEEROLES_EMPLOYEES");

            entity.HasOne(d => d.Role).WithMany(p => p.Employeeroles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EMPLOYEEROLES_ROLES");
        });

        modelBuilder.Entity<Employeeskill>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Employee).WithMany(p => p.Employeeskills)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EMPLOYEESKILLS_EMPLOYEES");

            entity.HasOne(d => d.Skill).WithMany(p => p.Employeeskills)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EMPLOYEESKILLS_SKILLS");
        });

        modelBuilder.Entity<Employeetest>(entity =>
        {
            entity.HasKey(e => e.EmployeeTestId).HasName("PK__EMPLOYEE__DC84927933BF83BF");

            entity.HasOne(d => d.Employee).WithMany(p => p.Employeetests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EMPLOYEETESTS_EMPLOYEES");

            entity.HasOne(d => d.Test).WithMany(p => p.Employeetests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EMPLOYEETESTS_TESTS");
        });

        modelBuilder.Entity<Employeetraining>(entity =>
        {
            entity.HasOne(d => d.Employee).WithMany(p => p.Employeetrainings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EMPLOYEETRAININGS_EMPLOYEES");

            entity.HasOne(d => d.Training).WithMany(p => p.Employeetrainings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EMPLOYEETRAININGS_TRAININGS");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__FEEDBACK__6A4BEDD625744C16");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Employee).WithMany(p => p.Feedbacks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FEEDBACKS_EMPLOYEES");

            entity.HasOne(d => d.Test).WithMany(p => p.Feedbacks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FEEDBACKS_TESTS");
        });

        modelBuilder.Entity<Level>(entity =>
        {
            entity.HasKey(e => e.LevelId).HasName("PK__LEVELS__09F03C26F671A2EE");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__NOTIFICA__20CF2E1201F5B4E0");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("unread");

            entity.HasOne(d => d.Employee).WithMany(p => p.Notifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NOTIFICATIONS_EMPLOYEES");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__QUESTION__0DC06FAC86207AA6");

            entity.HasOne(d => d.Skill).WithMany(p => p.Questions).HasConstraintName("FK_QUESTIONS_SKILLS");
        });

        modelBuilder.Entity<QuestionDragDrop>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Question__727E838BD3510DFB");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionDragDrops).HasConstraintName("FK__QuestionD__Quest__0C85DE4D");
        });

        modelBuilder.Entity<QuestionMatching>(entity =>
        {
            entity.HasKey(e => e.PairId).HasName("PK__Question__B543F7CC50E9C375");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionMatchings).HasConstraintName("FK__QuestionM__Quest__09A971A2");
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__Question__92C7A1FF3B7EA073");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionOptions).HasConstraintName("FK__QuestionO__Quest__03F0984C");
        });

        modelBuilder.Entity<QuestionTrueFalse>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06FACA94C2890");

            entity.Property(e => e.QuestionId).ValueGeneratedNever();

            entity.HasOne(d => d.Question).WithOne(p => p.QuestionTrueFalse).HasConstraintName("FK__QuestionT__Quest__06CD04F7");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__ROLES__8AFACE1ABC9A2DF0");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("PK__SKILLS__DFA0918726209838");
        });

        modelBuilder.Entity<Systemsetting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PK__SYSTEMSE__54372B1D66A4DDD1");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PK__TESTS__8CC3316040D14BD7");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Tests).HasConstraintName("FK_TESTS_EMPLOYEES");
        });

        modelBuilder.Entity<Testquestion>(entity =>
        {
            entity.HasOne(d => d.Question).WithMany(p => p.Testquestions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TESTQUESTIONS_QUESTIONS");

            entity.HasOne(d => d.Test).WithMany(p => p.Testquestions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TESTQUESTIONS_TESTS");
        });

        modelBuilder.Entity<Training>(entity =>
        {
            entity.HasKey(e => e.TrainingId).HasName("PK__TRAINING__E8D71D825432F222");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
