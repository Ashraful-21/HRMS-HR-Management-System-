using System;
using System.Collections.Generic;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Data;

public partial class HrmsDbContext : DbContext
{
    public HrmsDbContext(DbContextOptions<HrmsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<Asset> Assets { get; set; }

    public virtual DbSet<AssetAssignment> AssetAssignments { get; set; }

    public virtual DbSet<AssetCategory> AssetCategories { get; set; }

    public virtual DbSet<AttendanceLog> AttendanceLogs { get; set; }

    public virtual DbSet<AttendanceRegularization> AttendanceRegularizations { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Candidate> Candidates { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Designation> Designations { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeAddress> EmployeeAddresses { get; set; }

    public virtual DbSet<EmployeeBankAccount> EmployeeBankAccounts { get; set; }

    public virtual DbSet<EmployeeCodeSequence> EmployeeCodeSequences { get; set; }

    public virtual DbSet<EmployeeDocument> EmployeeDocuments { get; set; }

    public virtual DbSet<EmployeeEducation> EmployeeEducations { get; set; }

    public virtual DbSet<EmployeeEmergencyContact> EmployeeEmergencyContacts { get; set; }

    public virtual DbSet<EmployeeExperience> EmployeeExperiences { get; set; }

    public virtual DbSet<EmployeeLifecycleEvent> EmployeeLifecycleEvents { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<Holiday> Holidays { get; set; }

    public virtual DbSet<Interview> Interviews { get; set; }

    public virtual DbSet<JobOpening> JobOpenings { get; set; }

    public virtual DbSet<LeaveApplication> LeaveApplications { get; set; }

    public virtual DbSet<LeaveBalance> LeaveBalances { get; set; }

    public virtual DbSet<LeaveType> LeaveTypes { get; set; }

    public virtual DbSet<Loan> Loans { get; set; }

    public virtual DbSet<LoanRepayment> LoanRepayments { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<OfferLetter> OfferLetters { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<PayrollComponentDetail> PayrollComponentDetails { get; set; }

    public virtual DbSet<PayrollDetail> PayrollDetails { get; set; }

    public virtual DbSet<PayrollRun> PayrollRuns { get; set; }

    public virtual DbSet<PerformanceGoal> PerformanceGoals { get; set; }

    public virtual DbSet<PerformanceReview> PerformanceReviews { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<ReviewCycle> ReviewCycles { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<SalaryComponent> SalaryComponents { get; set; }

    public virtual DbSet<SalaryStructure> SalaryStructures { get; set; }

    public virtual DbSet<SalaryStructureComponent> SalaryStructureComponents { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<VwEmployeeDirectory> VwEmployeeDirectories { get; set; }

    public virtual DbSet<VwLeaveBalanceSummary> VwLeaveBalanceSummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Announce__3214EC073AC02D6B");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsPublished).HasDefaultValue(true);
            entity.Property(e => e.PublishFrom).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Title).HasMaxLength(300);

            entity.HasOne(d => d.Branch).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__Announcem__Branc__15DA3E5D");

            entity.HasOne(d => d.Department).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__Announcem__Depar__14E61A24");
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Applicat__3214EC076E377C05");

            entity.HasIndex(e => new { e.JobOpeningId, e.CandidateId }, "UQ_Application_JobCandidate").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.Stage)
                .HasMaxLength(50)
                .HasDefaultValue("Applied");
            entity.Property(e => e.StageOrder).HasDefaultValue(1);

            entity.HasOne(d => d.Candidate).WithMany(p => p.Applications)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__Candi__60083D91");

            entity.HasOne(d => d.JobOpening).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobOpeningId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__JobOp__5F141958");
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Assets__3214EC074E66EBA5");

            entity.HasIndex(e => e.AssetCode, "UQ__Assets__2DDE52401F50791A").IsUnique();

            entity.Property(e => e.AssetCode).HasMaxLength(50);
            entity.Property(e => e.Condition)
                .HasMaxLength(30)
                .HasDefaultValue("Good");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.PurchaseCost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Available");

            entity.HasOne(d => d.Category).WithMany(p => p.Assets)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Assets__Category__0A688BB1");
        });

        modelBuilder.Entity<AssetAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AssetAss__3214EC07F3A6C502");

            entity.Property(e => e.Condition)
                .HasMaxLength(30)
                .HasDefaultValue("Good");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ReturnCondition).HasMaxLength(30);

            entity.HasOne(d => d.Asset).WithMany(p => p.AssetAssignments)
                .HasForeignKey(d => d.AssetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AssetAssi__Asset__7F80E8EA");

            entity.HasOne(d => d.Employee).WithMany(p => p.AssetAssignments)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AssetAssi__Emplo__00750D23");
        });

        modelBuilder.Entity<AssetCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AssetCat__3214EC07F5AE3876");

            entity.HasIndex(e => e.Code, "UQ__AssetCat__A25C5AA78E4F41EF").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<AttendanceLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC07CA9C7FA5");

            entity.HasIndex(e => new { e.EmployeeId, e.LogDate }, "IX_Attendance_EmployeeDate");

            entity.HasIndex(e => e.LogDate, "IX_Attendance_LogDate");

            entity.HasIndex(e => new { e.EmployeeId, e.LogDate }, "UQ_Attendance_EmpDate").IsUnique();

            entity.Property(e => e.CheckInIp).HasMaxLength(50);
            entity.Property(e => e.CheckInLat).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.CheckInLng).HasColumnType("decimal(11, 8)");
            entity.Property(e => e.CheckOutIp).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Present");

            entity.HasOne(d => d.Employee).WithMany(p => p.AttendanceLogs)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Emplo__0B27A5C0");

            entity.HasOne(d => d.Shift).WithMany(p => p.AttendanceLogs)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK__Attendanc__Shift__0C1BC9F9");
        });

        modelBuilder.Entity<AttendanceRegularization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC07B959E1AD");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.ReviewRemarks).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.AttendanceLog).WithMany(p => p.AttendanceRegularizations)
                .HasForeignKey(d => d.AttendanceLogId)
                .HasConstraintName("FK__Attendanc__Atten__1975C517");

            entity.HasOne(d => d.Employee).WithMany(p => p.AttendanceRegularizationEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Emplo__1881A0DE");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.AttendanceRegularizationReviewedByNavigations)
                .HasForeignKey(d => d.ReviewedBy)
                .HasConstraintName("FK__Attendanc__Revie__1B5E0D89");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditLog__3214EC07EC41DDA7");

            entity.HasIndex(e => new { e.EntityType, e.EntityId }, "IX_AuditLogs_EntityType");

            entity.HasIndex(e => e.Timestamp, "IX_AuditLogs_Timestamp").IsDescending();

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.EntityId).HasMaxLength(50);
            entity.Property(e => e.EntityType).HasMaxLength(200);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Branches__3214EC0726573F03");

            entity.HasIndex(e => e.Code, "UQ__Branches__A25C5AA7B2278EAA").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(30);
        });

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Candidat__3214EC07CDDC9152");

            entity.HasIndex(e => e.Email, "UQ__Candidat__A9D10534C5F9BB45").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.FullName)
                .HasMaxLength(201)
                .HasComputedColumnSql("(([FirstName]+' ')+[LastName])", true);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.LinkedInUrl).HasMaxLength(300);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.ResumePublicId).HasMaxLength(300);
            entity.Property(e => e.ResumeUrl).HasMaxLength(500);
            entity.Property(e => e.Source).HasMaxLength(100);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departme__3214EC077D931FD8");

            entity.HasIndex(e => e.Code, "UQ__Departme__A25C5AA73C9D5092").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.Branch).WithMany(p => p.Departments)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Departmen__Branc__52593CB8");

            entity.HasOne(d => d.HeadEmployee).WithMany(p => p.Departments)
                .HasForeignKey(d => d.HeadEmployeeId)
                .HasConstraintName("FK_Departments_HeadEmployee");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK__Departmen__Paren__534D60F1");
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Designat__3214EC07BDCBE380");

            entity.HasIndex(e => e.Code, "UQ__Designat__A25C5AA706D4702D").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.Department).WithMany(p => p.Designations)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Designati__Depar__59FA5E80");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC07AAEE44E5");

            entity.HasIndex(e => e.BranchId, "IX_Employees_BranchId");

            entity.HasIndex(e => e.DepartmentId, "IX_Employees_DepartmentId");

            entity.HasIndex(e => e.ReportingManagerId, "IX_Employees_ManagerId");

            entity.HasIndex(e => e.Status, "IX_Employees_Status").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.EmployeeCode, "UQ__Employee__1F642548B739FF1B").IsUnique();

            entity.HasIndex(e => e.WorkEmail, "UQ__Employee__D5632109CDAD8170").IsUnique();

            entity.HasIndex(e => e.NationalId, "UQ__Employee__E9AA32FAE36C20EE").IsUnique();

            entity.Property(e => e.BloodGroup).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EmployeeCode).HasMaxLength(20);
            entity.Property(e => e.EmploymentType)
                .HasMaxLength(50)
                .HasDefaultValue("Full-Time");
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.FullName)
                .HasMaxLength(201)
                .HasComputedColumnSql("(([FirstName]+' ')+[LastName])", true);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.MaritalStatus).HasMaxLength(30);
            entity.Property(e => e.NationalId).HasMaxLength(50);
            entity.Property(e => e.Nationality).HasMaxLength(100);
            entity.Property(e => e.PersonalEmail).HasMaxLength(200);
            entity.Property(e => e.PersonalPhone).HasMaxLength(30);
            entity.Property(e => e.ProfilePhotoPublicId).HasMaxLength(300);
            entity.Property(e => e.ProfilePhotoUrl).HasMaxLength(500);
            entity.Property(e => e.Religion).HasMaxLength(100);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
            entity.Property(e => e.TerminationReason).HasMaxLength(500);
            entity.Property(e => e.WorkEmail).HasMaxLength(200);
            entity.Property(e => e.WorkPhone).HasMaxLength(30);

            entity.HasOne(d => d.Branch).WithMany(p => p.Employees)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Employees__Branc__44952D46");

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Employees__Depar__4589517F");

            entity.HasOne(d => d.Designation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DesignationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Employees__Desig__467D75B8");

            entity.HasOne(d => d.Grade).WithMany(p => p.Employees)
                .HasForeignKey(d => d.GradeId)
                .HasConstraintName("FK__Employees__Grade__477199F1");

            entity.HasOne(d => d.ReportingManager).WithMany(p => p.InverseReportingManager)
                .HasForeignKey(d => d.ReportingManagerId)
                .HasConstraintName("FK__Employees__Repor__4959E263");

            entity.HasOne(d => d.Shift).WithMany(p => p.Employees)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK__Employees__Shift__4865BE2A");
        });

        modelBuilder.Entity<EmployeeAddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC072A7C3244");

            entity.Property(e => e.AddressType).HasMaxLength(30);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Street).HasMaxLength(300);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeAddresses)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__EmployeeA__Emplo__50FB042B");
        });

        modelBuilder.Entity<EmployeeBankAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC07E8F40927");

            entity.Property(e => e.AccountNumber).HasMaxLength(50);
            entity.Property(e => e.AccountTitle).HasMaxLength(200);
            entity.Property(e => e.BankName).HasMaxLength(200);
            entity.Property(e => e.BranchName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsPrimary).HasDefaultValue(true);
            entity.Property(e => e.RoutingNumber).HasMaxLength(50);
            entity.Property(e => e.SwiftCode).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeBankAccounts)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__EmployeeB__Emplo__6319B466");
        });

        modelBuilder.Entity<EmployeeCodeSequence>(entity =>
        {
            entity.HasKey(e => e.Year).HasName("PK__Employee__D4BD605510861E05");

            entity.Property(e => e.Year).ValueGeneratedNever();
        });

        modelBuilder.Entity<EmployeeDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC07465EDB9D");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DocumentType).HasMaxLength(100);
            entity.Property(e => e.FilePublicId).HasMaxLength(300);
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.IsPrivate).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(300);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeDocuments)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__EmployeeD__Emplo__67DE6983");
        });

        modelBuilder.Entity<EmployeeEducation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC073EDFAD68");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Degree).HasMaxLength(200);
            entity.Property(e => e.FieldOfStudy).HasMaxLength(200);
            entity.Property(e => e.Grade).HasMaxLength(50);
            entity.Property(e => e.Institution).HasMaxLength(300);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeEducations)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__EmployeeE__Emplo__54CB950F");
        });

        modelBuilder.Entity<EmployeeEmergencyContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC07565902B3");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Relationship).HasMaxLength(100);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeEmergencyContacts)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__EmployeeE__Emplo__5E54FF49");
        });

        modelBuilder.Entity<EmployeeExperience>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC07BFCF6151");

            entity.Property(e => e.Company).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Designation).HasMaxLength(200);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeExperiences)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__EmployeeE__Emplo__59904A2C");
        });

        modelBuilder.Entity<EmployeeLifecycleEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC0765FCBEFF");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EventType).HasMaxLength(50);
            entity.Property(e => e.Remarks).HasMaxLength(1000);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeLifecycleEvents)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__EmployeeL__Emplo__6D9742D9");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Grades__3214EC075C731526");

            entity.HasIndex(e => e.Code, "UQ__Grades__A25C5AA729796806").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MinSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Holidays__3214EC07A9985216");

            entity.HasIndex(e => new { e.HolidayDate, e.BranchId }, "UQ_Holidays_DateBranch").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasDefaultValue("Public");

            entity.HasOne(d => d.Branch).WithMany(p => p.Holidays)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__Holidays__Branch__6EF57B66");
        });

        modelBuilder.Entity<Interview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Intervie__3214EC072AB07D21");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Location).HasMaxLength(300);
            entity.Property(e => e.MeetingLink).HasMaxLength(500);
            entity.Property(e => e.Mode)
                .HasMaxLength(30)
                .HasDefaultValue("InPerson");
            entity.Property(e => e.Result).HasMaxLength(30);
            entity.Property(e => e.Score).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Application).WithMany(p => p.Interviews)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("FK__Interview__Appli__66B53B20");

            entity.HasOne(d => d.Interviewer).WithMany(p => p.Interviews)
                .HasForeignKey(d => d.InterviewerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Interview__Inter__67A95F59");
        });

        modelBuilder.Entity<JobOpening>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JobOpeni__3214EC07D30594C7");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EmploymentType)
                .HasMaxLength(50)
                .HasDefaultValue("Full-Time");
            entity.Property(e => e.PostedDate).HasDefaultValueSql("(CONVERT([date],sysutcdatetime()))");
            entity.Property(e => e.SalaryMax).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SalaryMin).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Open");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Vacancies).HasDefaultValue(1);

            entity.HasOne(d => d.Branch).WithMany(p => p.JobOpenings)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JobOpenin__Branc__6442E2C9");

            entity.HasOne(d => d.Department).WithMany(p => p.JobOpenings)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JobOpenin__Depar__625A9A57");

            entity.HasOne(d => d.Designation).WithMany(p => p.JobOpenings)
                .HasForeignKey(d => d.DesignationId)
                .HasConstraintName("FK__JobOpenin__Desig__634EBE90");
        });

        modelBuilder.Entity<LeaveApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeaveApp__3214EC07489A4F4F");

            entity.HasIndex(e => new { e.StartDate, e.EndDate }, "IX_LeaveApplications_Dates");

            entity.HasIndex(e => new { e.EmployeeId, e.Status }, "IX_LeaveApplications_Employee");

            entity.Property(e => e.ApprovalRemarks).HasMaxLength(500);
            entity.Property(e => e.AttachmentPublicId).HasMaxLength(300);
            entity.Property(e => e.AttachmentUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.HalfDayType).HasMaxLength(20);
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalDays).HasColumnType("decimal(5, 1)");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.LeaveApplicationApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK__LeaveAppl__Appro__2C88998B");

            entity.HasOne(d => d.Employee).WithMany(p => p.LeaveApplicationEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LeaveAppl__Emplo__28B808A7");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.LeaveApplications)
                .HasForeignKey(d => d.LeaveTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LeaveAppl__Leave__29AC2CE0");

            entity.HasOne(d => d.RejectedByNavigation).WithMany(p => p.LeaveApplicationRejectedByNavigations)
                .HasForeignKey(d => d.RejectedBy)
                .HasConstraintName("FK__LeaveAppl__Rejec__2D7CBDC4");
        });

        modelBuilder.Entity<LeaveBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeaveBal__3214EC07605475C3");

            entity.HasIndex(e => new { e.EmployeeId, e.LeaveTypeId, e.Year }, "UQ_LeaveBalance_EmpTypeYear").IsUnique();

            entity.Property(e => e.Balance)
                .HasComputedColumnSql("(([TotalDays]+[CarryForwardDays])-[UsedDays])", true)
                .HasColumnType("decimal(7, 1)");
            entity.Property(e => e.CarryForwardDays).HasColumnType("decimal(5, 1)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.TotalDays).HasColumnType("decimal(5, 1)");
            entity.Property(e => e.UsedDays).HasColumnType("decimal(5, 1)");

            entity.HasOne(d => d.Employee).WithMany(p => p.LeaveBalances)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__LeaveBala__Emplo__2116E6DF");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.LeaveBalances)
                .HasForeignKey(d => d.LeaveTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LeaveBala__Leave__220B0B18");
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeaveTyp__3214EC0792EDF0D5");

            entity.HasIndex(e => e.Code, "UQ__LeaveTyp__A25C5AA748A529B3").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DaysAllowed).HasColumnType("decimal(5, 1)");
            entity.Property(e => e.GenderRestriction).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsHalfDayAllowed).HasDefaultValue(true);
            entity.Property(e => e.IsPaidLeave).HasDefaultValue(true);
            entity.Property(e => e.MaxCarryForwardDays).HasColumnType("decimal(5, 1)");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Loans__3214EC07F6F887E7");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ApprovedAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.InstallmentAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LoanType)
                .HasMaxLength(50)
                .HasDefaultValue("Personal");
            entity.Property(e => e.OutstandingAmount)
                .HasComputedColumnSql("(isnull([ApprovedAmount],(0))-[PaidAmount])", true)
                .HasColumnType("decimal(19, 2)");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.LoanApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK__Loans__ApprovedB__4FD1D5C8");

            entity.HasOne(d => d.Employee).WithMany(p => p.LoanEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Loans__EmployeeI__4B0D20AB");
        });

        modelBuilder.Entity<LoanRepayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoanRepa__3214EC074DF5FE72");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Loan).WithMany(p => p.LoanRepayments)
                .HasForeignKey(d => d.LoanId)
                .HasConstraintName("FK__LoanRepay__LoanI__54968AE5");

            entity.HasOne(d => d.PayrollDetail).WithMany(p => p.LoanRepayments)
                .HasForeignKey(d => d.PayrollDetailId)
                .HasConstraintName("FK__LoanRepay__Payro__558AAF1E");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC07C53FD7B3");

            entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt }, "IX_Notifications_UserId").IsDescending(false, false, true);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasDefaultValue("Info");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__062DE679");
        });

        modelBuilder.Entity<OfferLetter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OfferLet__3214EC0703607584");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FilePublicId).HasMaxLength(300);
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.OfferedSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Sent");

            entity.HasOne(d => d.Application).WithMany(p => p.OfferLetters)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("FK__OfferLett__Appli__6D6238AF");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Password__3214EC07EA137DB5");

            entity.HasIndex(e => e.TokenHash, "UQ__Password__BCB33F9298C36C3B").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.TokenHash).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__PasswordR__UserI__0662F0A3");
        });

        modelBuilder.Entity<PayrollComponentDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PayrollC__3214EC07B7A3BE69");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Component).WithMany(p => p.PayrollComponentDetails)
                .HasForeignKey(d => d.ComponentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PayrollCo__Compo__4830B400");

            entity.HasOne(d => d.PayrollDetail).WithMany(p => p.PayrollComponentDetails)
                .HasForeignKey(d => d.PayrollDetailId)
                .HasConstraintName("FK__PayrollCo__Payro__473C8FC7");
        });

        modelBuilder.Entity<PayrollDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PayrollD__3214EC07FCE0F89C");

            entity.HasIndex(e => e.EmployeeId, "IX_PayrollDetails_Employee");

            entity.HasIndex(e => e.PayrollRunId, "IX_PayrollDetails_RunId");

            entity.HasIndex(e => new { e.PayrollRunId, e.EmployeeId }, "UQ_PayrollDetail_RunEmployee").IsUnique();

            entity.Property(e => e.GrossEarnings).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LossOfPay).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NetPay).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaidLeaveDays).HasColumnType("decimal(5, 1)");
            entity.Property(e => e.PresentDays).HasColumnType("decimal(5, 1)");
            entity.Property(e => e.TotalDeductions).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnpaidLeaveDays).HasColumnType("decimal(5, 1)");

            entity.HasOne(d => d.Employee).WithMany(p => p.PayrollDetails)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PayrollDe__Emplo__3EA749C6");

            entity.HasOne(d => d.PayrollRun).WithMany(p => p.PayrollDetails)
                .HasForeignKey(d => d.PayrollRunId)
                .HasConstraintName("FK__PayrollDe__Payro__3DB3258D");

            entity.HasOne(d => d.SalaryStructure).WithMany(p => p.PayrollDetails)
                .HasForeignKey(d => d.SalaryStructureId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PayrollDe__Salar__3F9B6DFF");
        });

        modelBuilder.Entity<PayrollRun>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PayrollR__3214EC0785E7ED58");

            entity.HasIndex(e => new { e.Month, e.Year, e.BranchId }, "UQ_PayrollRun_MonthYearBranch").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Draft");

            entity.HasOne(d => d.Branch).WithMany(p => p.PayrollRuns)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__PayrollRu__Branc__503BEA1C");
        });

        modelBuilder.Entity<PerformanceGoal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Performa__3214EC07E352D9DD");

            entity.Property(e => e.Achievement).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.GoalDescription).HasMaxLength(1000);
            entity.Property(e => e.ManagerScore).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.SelfScore).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.Target).HasMaxLength(500);
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Review).WithMany(p => p.PerformanceGoals)
                .HasForeignKey(d => d.ReviewId)
                .HasConstraintName("FK__Performan__Revie__7BB05806");
        });

        modelBuilder.Entity<PerformanceReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Performa__3214EC07D1DFE02D");

            entity.HasIndex(e => new { e.ReviewCycleId, e.EmployeeId }, "UQ_PerformanceReview_CycleEmployee").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FinalRating).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.ManagerRating).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.SelfRating).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Employee).WithMany(p => p.PerformanceReviewEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Performan__Emplo__75035A77");

            entity.HasOne(d => d.ReviewCycle).WithMany(p => p.PerformanceReviews)
                .HasForeignKey(d => d.ReviewCycleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Performan__Revie__740F363E");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.PerformanceReviewReviewers)
                .HasForeignKey(d => d.ReviewerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Performan__Revie__75F77EB0");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permissi__3214EC07DD0E9647");

            entity.HasIndex(e => e.Code, "UQ__Permissi__A25C5AA77F6372D0").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Module).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC07CD9B0BD9");

            entity.HasIndex(e => e.UserId, "IX_RefreshTokens_UserId");

            entity.HasIndex(e => e.TokenHash, "UQ__RefreshT__BCB33F92590F1E93").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedByIp).HasMaxLength(50);
            entity.Property(e => e.ReplacedByTokenHash).HasMaxLength(500);
            entity.Property(e => e.RevokedByIp).HasMaxLength(50);
            entity.Property(e => e.TokenHash).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__RefreshTo__UserI__00AA174D");
        });

        modelBuilder.Entity<ReviewCycle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReviewCy__3214EC07BA5532D8");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Draft");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07369670C4");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F61FAAB124").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId }).HasName("PK__RolePerm__6400A1A8A563C1AD");

            entity.Property(e => e.GrantedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .HasConstraintName("FK__RolePermi__Permi__2180FB33");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__RolePermi__RoleI__208CD6FA");
        });

        modelBuilder.Entity<SalaryComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalaryCo__3214EC0779EE0D5E");

            entity.HasIndex(e => e.Code, "UQ__SalaryCo__A25C5AA745D96028").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsFixed).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Type).HasMaxLength(30);
        });

        modelBuilder.Entity<SalaryStructure>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalarySt__3214EC07E5B91F7B");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.GrossSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Remarks).HasMaxLength(500);

            entity.HasOne(d => d.Employee).WithMany(p => p.SalaryStructures)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__SalaryStr__Emplo__324172E1");
        });

        modelBuilder.Entity<SalaryStructureComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalarySt__3214EC07FF56257D");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CalculationType)
                .HasMaxLength(30)
                .HasDefaultValue("Fixed");
            entity.Property(e => e.PercentageBase).HasMaxLength(50);

            entity.HasOne(d => d.Component).WithMany(p => p.SalaryStructureComponents)
                .HasForeignKey(d => d.ComponentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalaryStr__Compo__38EE7070");

            entity.HasOne(d => d.SalaryStructure).WithMany(p => p.SalaryStructureComponents)
                .HasForeignKey(d => d.SalaryStructureId)
                .HasConstraintName("FK__SalaryStr__Salar__37FA4C37");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Shifts__3214EC074AEDE3DB");

            entity.HasIndex(e => e.Code, "UQ__Shifts__A25C5AA7CFBB6ADB").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.WorkingHours).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SystemSe__3214EC078551E116");

            entity.HasIndex(e => e.Key, "UQ__SystemSe__C41E02895E54F32F").IsUnique();

            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasDefaultValue("General");
            entity.Property(e => e.DataType)
                .HasMaxLength(50)
                .HasDefaultValue("String");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Key).HasMaxLength(200);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07CD104153");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4A15C1D76").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105341A978B29").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasOne(d => d.Employee).WithMany(p => p.Users)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Users__EmployeeI__73501C2F");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__UserRole__AF2760AD9794BDD2");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__UserRoles__RoleI__7BE56230");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserRoles__UserI__7AF13DF7");
        });

        modelBuilder.Entity<VwEmployeeDirectory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_EmployeeDirectory");

            entity.Property(e => e.Branch).HasMaxLength(150);
            entity.Property(e => e.Department).HasMaxLength(150);
            entity.Property(e => e.Designation).HasMaxLength(150);
            entity.Property(e => e.EmployeeCode).HasMaxLength(20);
            entity.Property(e => e.EmploymentType).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(201);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.Grade).HasMaxLength(100);
            entity.Property(e => e.ManagerName).HasMaxLength(201);
            entity.Property(e => e.PersonalPhone).HasMaxLength(30);
            entity.Property(e => e.ProfilePhotoUrl).HasMaxLength(500);
            entity.Property(e => e.Shift).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.WorkEmail).HasMaxLength(200);
            entity.Property(e => e.WorkPhone).HasMaxLength(30);
        });

        modelBuilder.Entity<VwLeaveBalanceSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_LeaveBalanceSummary");

            entity.Property(e => e.Balance).HasColumnType("decimal(7, 1)");
            entity.Property(e => e.CarryForwardDays).HasColumnType("decimal(5, 1)");
            entity.Property(e => e.EmployeeCode).HasMaxLength(20);
            entity.Property(e => e.FullName).HasMaxLength(201);
            entity.Property(e => e.LeaveType).HasMaxLength(100);
            entity.Property(e => e.LeaveTypeCode).HasMaxLength(20);
            entity.Property(e => e.TotalDays).HasColumnType("decimal(5, 1)");
            entity.Property(e => e.UsedDays).HasColumnType("decimal(5, 1)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
