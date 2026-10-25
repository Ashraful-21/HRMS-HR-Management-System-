-- HRMS_schema.sql  |  SQL Server 2019+  |  Idempotent - safe to re-run
USE master;
GO
IF DB_ID('HRMS') IS NULL CREATE DATABASE HRMS;
GO
USE HRMS;
GO
-- ===== 1. ORGANISATION =====
IF OBJECT_ID('dbo.Branches','U') IS NULL
CREATE TABLE dbo.Branches (
    Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(150) NOT NULL, Code NVARCHAR(20) NOT NULL UNIQUE,
    Address NVARCHAR(500) NULL, City NVARCHAR(100) NULL, Country NVARCHAR(100) NULL,
    Phone NVARCHAR(30) NULL, Email NVARCHAR(200) NULL, IsHeadOffice BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1, IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), CreatedBy INT NULL,
    UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.Departments','U') IS NULL
CREATE TABLE dbo.Departments (
    Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(150) NOT NULL, Code NVARCHAR(20) NOT NULL UNIQUE,
    BranchId INT NOT NULL REFERENCES dbo.Branches(Id), ParentId INT NULL REFERENCES dbo.Departments(Id),
    HeadEmployeeId INT NULL, Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1, IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), CreatedBy INT NULL,
    UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.Designations','U') IS NULL
CREATE TABLE dbo.Designations (
    Id INT IDENTITY PRIMARY KEY, Title NVARCHAR(150) NOT NULL, Code NVARCHAR(20) NOT NULL UNIQUE,
    DepartmentId INT NOT NULL REFERENCES dbo.Departments(Id),
    IsActive BIT NOT NULL DEFAULT 1, IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), CreatedBy INT NULL,
    UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.Grades','U') IS NULL
CREATE TABLE dbo.Grades (
    Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(100) NOT NULL, Code NVARCHAR(20) NOT NULL UNIQUE,
    MinSalary DECIMAL(18,2) NULL, MaxSalary DECIMAL(18,2) NULL, Description NVARCHAR(300) NULL,
    IsActive BIT NOT NULL DEFAULT 1, IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), CreatedBy INT NULL,
    UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.Shifts','U') IS NULL
CREATE TABLE dbo.Shifts (
    Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(100) NOT NULL, Code NVARCHAR(20) NOT NULL UNIQUE,
    StartTime TIME NOT NULL, EndTime TIME NOT NULL, GraceMinutes INT NOT NULL DEFAULT 0,
    IsNightShift BIT NOT NULL DEFAULT 0, WorkingHours DECIMAL(5,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1, IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), CreatedBy INT NULL,
    UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.Holidays','U') IS NULL
CREATE TABLE dbo.Holidays (
    Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(200) NOT NULL, HolidayDate DATE NOT NULL,
    Type NVARCHAR(50) NOT NULL DEFAULT 'Public', BranchId INT NULL REFERENCES dbo.Branches(Id),
    IsRecurring BIT NOT NULL DEFAULT 0, Description NVARCHAR(500) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL,
    CONSTRAINT UQ_Holidays_DateBranch UNIQUE (HolidayDate, BranchId)
);
GO
IF OBJECT_ID('dbo.SystemSettings','U') IS NULL
CREATE TABLE dbo.SystemSettings (
    Id INT IDENTITY PRIMARY KEY, [Key] NVARCHAR(200) NOT NULL UNIQUE, [Value] NVARCHAR(MAX) NULL,
    Category NVARCHAR(100) NOT NULL DEFAULT 'General', DataType NVARCHAR(50) NOT NULL DEFAULT 'String',
    Description NVARCHAR(500) NULL, IsPublic BIT NOT NULL DEFAULT 0, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
-- ===== 2. EMPLOYEES =====
IF OBJECT_ID('dbo.Employees','U') IS NULL
CREATE TABLE dbo.Employees (
    Id INT IDENTITY PRIMARY KEY, EmployeeCode NVARCHAR(20) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL, LastName NVARCHAR(100) NOT NULL,
    FullName AS (FirstName + ' ' + LastName) PERSISTED,
    Gender NVARCHAR(20) NOT NULL, DateOfBirth DATE NOT NULL, NationalId NVARCHAR(50) NULL UNIQUE,
    BloodGroup NVARCHAR(10) NULL, PersonalEmail NVARCHAR(200) NULL,
    WorkEmail NVARCHAR(200) NOT NULL UNIQUE, PersonalPhone NVARCHAR(30) NULL, WorkPhone NVARCHAR(30) NULL,
    MaritalStatus NVARCHAR(30) NULL, Nationality NVARCHAR(100) NULL, Religion NVARCHAR(100) NULL,
    ProfilePhotoUrl NVARCHAR(500) NULL, ProfilePhotoPublicId NVARCHAR(300) NULL,
    BranchId INT NOT NULL REFERENCES dbo.Branches(Id),
    DepartmentId INT NOT NULL REFERENCES dbo.Departments(Id),
    DesignationId INT NOT NULL REFERENCES dbo.Designations(Id),
    GradeId INT NULL REFERENCES dbo.Grades(Id), ShiftId INT NULL REFERENCES dbo.Shifts(Id),
    ReportingManagerId INT NULL REFERENCES dbo.Employees(Id),
    JoiningDate DATE NOT NULL, ProbationEndDate DATE NULL, ConfirmationDate DATE NULL,
    EmploymentType NVARCHAR(50) NOT NULL DEFAULT 'Full-Time',
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    ResignationDate DATE NULL, LastWorkingDate DATE NULL, TerminationReason NVARCHAR(500) NULL,
    RowVersion ROWVERSION NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Departments_HeadEmployee')
    ALTER TABLE dbo.Departments ADD CONSTRAINT FK_Departments_HeadEmployee
    FOREIGN KEY (HeadEmployeeId) REFERENCES dbo.Employees(Id);
GO
IF OBJECT_ID('dbo.EmployeeAddresses','U') IS NULL
CREATE TABLE dbo.EmployeeAddresses (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id) ON DELETE CASCADE,
    AddressType NVARCHAR(30) NOT NULL, Street NVARCHAR(300) NOT NULL, City NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NULL, PostalCode NVARCHAR(20) NULL, Country NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt DATETIME2 NULL
);
GO
IF OBJECT_ID('dbo.EmployeeEducations','U') IS NULL
CREATE TABLE dbo.EmployeeEducations (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id) ON DELETE CASCADE,
    Degree NVARCHAR(200) NOT NULL, Institution NVARCHAR(300) NOT NULL, FieldOfStudy NVARCHAR(200) NULL,
    StartYear INT NULL, EndYear INT NULL, Grade NVARCHAR(50) NULL, IsHighest BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt DATETIME2 NULL
);
GO
IF OBJECT_ID('dbo.EmployeeExperiences','U') IS NULL
CREATE TABLE dbo.EmployeeExperiences (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id) ON DELETE CASCADE,
    Company NVARCHAR(300) NOT NULL, Designation NVARCHAR(200) NULL, StartDate DATE NOT NULL,
    EndDate DATE NULL, IsCurrent BIT NOT NULL DEFAULT 0, Description NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt DATETIME2 NULL
);
GO
IF OBJECT_ID('dbo.EmployeeEmergencyContacts','U') IS NULL
CREATE TABLE dbo.EmployeeEmergencyContacts (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id) ON DELETE CASCADE,
    Name NVARCHAR(200) NOT NULL, Relationship NVARCHAR(100) NOT NULL, Phone NVARCHAR(30) NOT NULL,
    Email NVARCHAR(200) NULL, Address NVARCHAR(500) NULL, IsPrimary BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt DATETIME2 NULL
);
GO
IF OBJECT_ID('dbo.EmployeeBankAccounts','U') IS NULL
CREATE TABLE dbo.EmployeeBankAccounts (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id) ON DELETE CASCADE,
    BankName NVARCHAR(200) NOT NULL, AccountNumber NVARCHAR(50) NOT NULL,
    AccountTitle NVARCHAR(200) NOT NULL, BranchName NVARCHAR(200) NULL,
    RoutingNumber NVARCHAR(50) NULL, SwiftCode NVARCHAR(50) NULL, IsPrimary BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt DATETIME2 NULL
);
GO
IF OBJECT_ID('dbo.EmployeeDocuments','U') IS NULL
CREATE TABLE dbo.EmployeeDocuments (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id) ON DELETE CASCADE,
    DocumentType NVARCHAR(100) NOT NULL, Title NVARCHAR(300) NOT NULL,
    FileUrl NVARCHAR(500) NOT NULL, FilePublicId NVARCHAR(300) NOT NULL,
    IsPrivate BIT NOT NULL DEFAULT 1, ExpiryDate DATE NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.EmployeeLifecycleEvents','U') IS NULL
CREATE TABLE dbo.EmployeeLifecycleEvents (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id) ON DELETE CASCADE,
    EventType NVARCHAR(50) NOT NULL, EventDate DATE NOT NULL,
    OldValue NVARCHAR(MAX) NULL, NewValue NVARCHAR(MAX) NULL, Remarks NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), CreatedBy INT NULL
);
GO
-- ===== 3. AUTH / USERS =====
IF OBJECT_ID('dbo.Users','U') IS NULL
CREATE TABLE dbo.Users (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NULL REFERENCES dbo.Employees(Id),
    Username NVARCHAR(100) NOT NULL UNIQUE, Email NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL, IsActive BIT NOT NULL DEFAULT 1,
    MustChangePassword BIT NOT NULL DEFAULT 0, FailedLoginCount INT NOT NULL DEFAULT 0,
    LockoutEndUtc DATETIME2 NULL, LastLoginAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.Roles','U') IS NULL
CREATE TABLE dbo.Roles (
    Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL, IsSystem BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.UserRoles','U') IS NULL
CREATE TABLE dbo.UserRoles (
    UserId INT NOT NULL REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    RoleId INT NOT NULL REFERENCES dbo.Roles(Id) ON DELETE CASCADE,
    AssignedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), AssignedBy INT NULL,
    PRIMARY KEY (UserId, RoleId)
);
GO
IF OBJECT_ID('dbo.Permissions','U') IS NULL
CREATE TABLE dbo.Permissions (
    Id INT IDENTITY PRIMARY KEY, Code NVARCHAR(200) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL, Module NVARCHAR(100) NOT NULL, Description NVARCHAR(500) NULL
);
GO
IF OBJECT_ID('dbo.RolePermissions','U') IS NULL
CREATE TABLE dbo.RolePermissions (
    RoleId INT NOT NULL REFERENCES dbo.Roles(Id) ON DELETE CASCADE,
    PermissionId INT NOT NULL REFERENCES dbo.Permissions(Id) ON DELETE CASCADE,
    GrantedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), GrantedBy INT NULL,
    PRIMARY KEY (RoleId, PermissionId)
);
GO
IF OBJECT_ID('dbo.RefreshTokens','U') IS NULL
CREATE TABLE dbo.RefreshTokens (
    Id INT IDENTITY PRIMARY KEY, UserId INT NOT NULL REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    TokenHash NVARCHAR(500) NOT NULL UNIQUE, ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), RevokedAt DATETIME2 NULL,
    ReplacedByTokenHash NVARCHAR(500) NULL, CreatedByIp NVARCHAR(50) NULL,
    RevokedByIp NVARCHAR(50) NULL, IsRevoked BIT NOT NULL DEFAULT 0
);
GO
IF OBJECT_ID('dbo.PasswordResetTokens','U') IS NULL
CREATE TABLE dbo.PasswordResetTokens (
    Id INT IDENTITY PRIMARY KEY, UserId INT NOT NULL REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    TokenHash NVARCHAR(500) NOT NULL UNIQUE, ExpiresAt DATETIME2 NOT NULL,
    UsedAt DATETIME2 NULL, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO
-- ===== 4. ATTENDANCE =====
IF OBJECT_ID('dbo.AttendanceLogs','U') IS NULL
CREATE TABLE dbo.AttendanceLogs (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
    LogDate DATE NOT NULL, CheckInTime DATETIME2 NULL, CheckOutTime DATETIME2 NULL,
    ShiftId INT NULL REFERENCES dbo.Shifts(Id),
    Status NVARCHAR(30) NOT NULL DEFAULT 'Present',
    IsLate BIT NOT NULL DEFAULT 0, LateMinutes INT NOT NULL DEFAULT 0,
    IsEarlyLeave BIT NOT NULL DEFAULT 0, EarlyLeaveMinutes INT NOT NULL DEFAULT 0,
    OvertimeMinutes INT NOT NULL DEFAULT 0, WorkingMinutes INT NOT NULL DEFAULT 0,
    CheckInIp NVARCHAR(50) NULL, CheckInLat DECIMAL(10,8) NULL, CheckInLng DECIMAL(11,8) NULL,
    CheckOutIp NVARCHAR(50) NULL, IsManualEntry BIT NOT NULL DEFAULT 0, Remarks NVARCHAR(500) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL,
    CONSTRAINT UQ_Attendance_EmpDate UNIQUE (EmployeeId, LogDate)
);
GO
IF OBJECT_ID('dbo.AttendanceRegularizations','U') IS NULL
CREATE TABLE dbo.AttendanceRegularizations (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
    AttendanceLogId INT NULL REFERENCES dbo.AttendanceLogs(Id),
    LogDate DATE NOT NULL, RequestedCheckIn DATETIME2 NULL, RequestedCheckOut DATETIME2 NULL,
    Reason NVARCHAR(1000) NOT NULL, Status NVARCHAR(30) NOT NULL DEFAULT 'Pending',
    ReviewedBy INT NULL REFERENCES dbo.Employees(Id), ReviewedAt DATETIME2 NULL,
    ReviewRemarks NVARCHAR(500) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
-- ===== 5. LEAVE =====
IF OBJECT_ID('dbo.LeaveTypes','U') IS NULL
CREATE TABLE dbo.LeaveTypes (
    Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(100) NOT NULL, Code NVARCHAR(20) NOT NULL UNIQUE,
    DaysAllowed DECIMAL(5,1) NOT NULL, IsCarryForwardAllowed BIT NOT NULL DEFAULT 0,
    MaxCarryForwardDays DECIMAL(5,1) NULL, IsPaidLeave BIT NOT NULL DEFAULT 1,
    IsHalfDayAllowed BIT NOT NULL DEFAULT 1, IsAttachmentRequired BIT NOT NULL DEFAULT 0,
    AttachmentRequiredAfterDays INT NULL, GenderRestriction NVARCHAR(20) NULL,
    IsActive BIT NOT NULL DEFAULT 1, IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), CreatedBy INT NULL,
    UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.LeaveBalances','U') IS NULL
CREATE TABLE dbo.LeaveBalances (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id) ON DELETE CASCADE,
    LeaveTypeId INT NOT NULL REFERENCES dbo.LeaveTypes(Id),
    Year INT NOT NULL, TotalDays DECIMAL(5,1) NOT NULL DEFAULT 0,
    UsedDays DECIMAL(5,1) NOT NULL DEFAULT 0, CarryForwardDays DECIMAL(5,1) NOT NULL DEFAULT 0,
    Balance AS (TotalDays + CarryForwardDays - UsedDays) PERSISTED,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt DATETIME2 NULL,
    CONSTRAINT UQ_LeaveBalance_EmpTypeYear UNIQUE (EmployeeId, LeaveTypeId, Year)
);
GO
IF OBJECT_ID('dbo.LeaveApplications','U') IS NULL
CREATE TABLE dbo.LeaveApplications (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
    LeaveTypeId INT NOT NULL REFERENCES dbo.LeaveTypes(Id),
    StartDate DATE NOT NULL, EndDate DATE NOT NULL, IsHalfDay BIT NOT NULL DEFAULT 0,
    HalfDayType NVARCHAR(20) NULL, TotalDays DECIMAL(5,1) NOT NULL, Reason NVARCHAR(1000) NOT NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Pending',
    AttachmentUrl NVARCHAR(500) NULL, AttachmentPublicId NVARCHAR(300) NULL,
    ApprovedBy INT NULL REFERENCES dbo.Employees(Id), ApprovedAt DATETIME2 NULL,
    RejectedBy INT NULL REFERENCES dbo.Employees(Id), RejectedAt DATETIME2 NULL,
    ApprovalRemarks NVARCHAR(500) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
-- ===== 6. PAYROLL =====
IF OBJECT_ID('dbo.SalaryComponents','U') IS NULL
CREATE TABLE dbo.SalaryComponents (
    Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(150) NOT NULL, Code NVARCHAR(30) NOT NULL UNIQUE,
    Type NVARCHAR(30) NOT NULL, IsFixed BIT NOT NULL DEFAULT 1, IsTaxable BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1, SortOrder INT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.SalaryStructures','U') IS NULL
CREATE TABLE dbo.SalaryStructures (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id) ON DELETE CASCADE,
    EffectiveFrom DATE NOT NULL, EffectiveTo DATE NULL, GrossSalary DECIMAL(18,2) NOT NULL,
    IsCurrent BIT NOT NULL DEFAULT 0, Remarks NVARCHAR(500) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.SalaryStructureComponents','U') IS NULL
CREATE TABLE dbo.SalaryStructureComponents (
    Id INT IDENTITY PRIMARY KEY,
    SalaryStructureId INT NOT NULL REFERENCES dbo.SalaryStructures(Id) ON DELETE CASCADE,
    ComponentId INT NOT NULL REFERENCES dbo.SalaryComponents(Id),
    CalculationType NVARCHAR(30) NOT NULL DEFAULT 'Fixed',
    Amount DECIMAL(18,2) NOT NULL, PercentageBase NVARCHAR(50) NULL
);
GO
IF OBJECT_ID('dbo.PayrollRuns','U') IS NULL
CREATE TABLE dbo.PayrollRuns (
    Id INT IDENTITY PRIMARY KEY, Month INT NOT NULL, Year INT NOT NULL,
    BranchId INT NULL REFERENCES dbo.Branches(Id), Status NVARCHAR(30) NOT NULL DEFAULT 'Draft',
    WorkingDays INT NOT NULL DEFAULT 0, ProcessedAt DATETIME2 NULL, ProcessedBy INT NULL,
    ApprovedAt DATETIME2 NULL, ApprovedBy INT NULL, PaidAt DATETIME2 NULL, PaidBy INT NULL,
    LockedAt DATETIME2 NULL, LockedBy INT NULL, Remarks NVARCHAR(1000) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL,
    CONSTRAINT UQ_PayrollRun_MonthYearBranch UNIQUE (Month, Year, BranchId)
);
GO
IF OBJECT_ID('dbo.PayrollDetails','U') IS NULL
CREATE TABLE dbo.PayrollDetails (
    Id INT IDENTITY PRIMARY KEY,
    PayrollRunId INT NOT NULL REFERENCES dbo.PayrollRuns(Id) ON DELETE CASCADE,
    EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
    SalaryStructureId INT NOT NULL REFERENCES dbo.SalaryStructures(Id),
    WorkingDays INT NOT NULL, PresentDays DECIMAL(5,1) NOT NULL,
    PaidLeaveDays DECIMAL(5,1) NOT NULL DEFAULT 0, UnpaidLeaveDays DECIMAL(5,1) NOT NULL DEFAULT 0,
    OvertimeMinutes INT NOT NULL DEFAULT 0, GrossEarnings DECIMAL(18,2) NOT NULL,
    TotalDeductions DECIMAL(18,2) NOT NULL, NetPay DECIMAL(18,2) NOT NULL,
    LossOfPay DECIMAL(18,2) NOT NULL DEFAULT 0, IsEmailSent BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_PayrollDetail_RunEmployee UNIQUE (PayrollRunId, EmployeeId)
);
GO
IF OBJECT_ID('dbo.PayrollComponentDetails','U') IS NULL
CREATE TABLE dbo.PayrollComponentDetails (
    Id INT IDENTITY PRIMARY KEY,
    PayrollDetailId INT NOT NULL REFERENCES dbo.PayrollDetails(Id) ON DELETE CASCADE,
    ComponentId INT NOT NULL REFERENCES dbo.SalaryComponents(Id),
    Amount DECIMAL(18,2) NOT NULL
);
GO
IF OBJECT_ID('dbo.Loans','U') IS NULL
CREATE TABLE dbo.Loans (
    Id INT IDENTITY PRIMARY KEY, EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
    LoanType NVARCHAR(50) NOT NULL DEFAULT 'Personal', Amount DECIMAL(18,2) NOT NULL,
    ApprovedAmount DECIMAL(18,2) NULL, PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    OutstandingAmount AS (ISNULL(ApprovedAmount,0) - PaidAmount) PERSISTED,
    Installments INT NOT NULL, InstallmentAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    StartMonth INT NULL, StartYear INT NULL, Status NVARCHAR(30) NOT NULL DEFAULT 'Pending',
    Reason NVARCHAR(1000) NULL, ApprovedBy INT NULL REFERENCES dbo.Employees(Id), ApprovedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.LoanRepayments','U') IS NULL
CREATE TABLE dbo.LoanRepayments (
    Id INT IDENTITY PRIMARY KEY, LoanId INT NOT NULL REFERENCES dbo.Loans(Id) ON DELETE CASCADE,
    PayrollDetailId INT NULL REFERENCES dbo.PayrollDetails(Id),
    Amount DECIMAL(18,2) NOT NULL, Month INT NOT NULL, Year INT NOT NULL,
    PaidAt DATETIME2 NULL, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO
-- ===== 7. RECRUITMENT =====
IF OBJECT_ID('dbo.JobOpenings','U') IS NULL
CREATE TABLE dbo.JobOpenings (
    Id INT IDENTITY PRIMARY KEY, Title NVARCHAR(200) NOT NULL,
    DepartmentId INT NOT NULL REFERENCES dbo.Departments(Id),
    DesignationId INT NULL REFERENCES dbo.Designations(Id),
    BranchId INT NOT NULL REFERENCES dbo.Branches(Id),
    Vacancies INT NOT NULL DEFAULT 1, EmploymentType NVARCHAR(50) NOT NULL DEFAULT 'Full-Time',
    ExperienceMinYrs INT NULL, ExperienceMaxYrs INT NULL,
    SalaryMin DECIMAL(18,2) NULL, SalaryMax DECIMAL(18,2) NULL,
    Description NVARCHAR(MAX) NULL, Requirements NVARCHAR(MAX) NULL,
    PostedDate DATE NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATE), ClosingDate DATE NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Open',
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.Candidates','U') IS NULL
CREATE TABLE dbo.Candidates (
    Id INT IDENTITY PRIMARY KEY, FirstName NVARCHAR(100) NOT NULL, LastName NVARCHAR(100) NOT NULL,
    FullName AS (FirstName + ' ' + LastName) PERSISTED,
    Email NVARCHAR(200) NOT NULL UNIQUE, Phone NVARCHAR(30) NULL,
    ResumeUrl NVARCHAR(500) NULL, ResumePublicId NVARCHAR(300) NULL,
    LinkedInUrl NVARCHAR(300) NULL, Source NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.Applications','U') IS NULL
CREATE TABLE dbo.Applications (
    Id INT IDENTITY PRIMARY KEY, JobOpeningId INT NOT NULL REFERENCES dbo.JobOpenings(Id),
    CandidateId INT NOT NULL REFERENCES dbo.Candidates(Id),
    Stage NVARCHAR(50) NOT NULL DEFAULT 'Applied', StageOrder INT NOT NULL DEFAULT 1,
    Notes NVARCHAR(2000) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL,
    CONSTRAINT UQ_Application_JobCandidate UNIQUE (JobOpeningId, CandidateId)
);
GO
IF OBJECT_ID('dbo.Interviews','U') IS NULL
CREATE TABLE dbo.Interviews (
    Id INT IDENTITY PRIMARY KEY, ApplicationId INT NOT NULL REFERENCES dbo.Applications(Id) ON DELETE CASCADE,
    InterviewerId INT NOT NULL REFERENCES dbo.Employees(Id),
    ScheduledAt DATETIME2 NOT NULL, Mode NVARCHAR(30) NOT NULL DEFAULT 'InPerson',
    Location NVARCHAR(300) NULL, MeetingLink NVARCHAR(500) NULL,
    Feedback NVARCHAR(MAX) NULL, Score DECIMAL(5,2) NULL, Result NVARCHAR(30) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.OfferLetters','U') IS NULL
CREATE TABLE dbo.OfferLetters (
    Id INT IDENTITY PRIMARY KEY, ApplicationId INT NOT NULL REFERENCES dbo.Applications(Id) ON DELETE CASCADE,
    OfferedSalary DECIMAL(18,2) NOT NULL, JoiningDate DATE NOT NULL, ExpiryDate DATE NOT NULL,
    FileUrl NVARCHAR(500) NULL, FilePublicId NVARCHAR(300) NULL, Status NVARCHAR(30) NOT NULL DEFAULT 'Sent',
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
-- ===== 8. PERFORMANCE =====
IF OBJECT_ID('dbo.ReviewCycles','U') IS NULL
CREATE TABLE dbo.ReviewCycles (
    Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(200) NOT NULL, Year INT NOT NULL,
    StartDate DATE NOT NULL, EndDate DATE NOT NULL, Status NVARCHAR(30) NOT NULL DEFAULT 'Draft',
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.PerformanceReviews','U') IS NULL
CREATE TABLE dbo.PerformanceReviews (
    Id INT IDENTITY PRIMARY KEY, ReviewCycleId INT NOT NULL REFERENCES dbo.ReviewCycles(Id),
    EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
    ReviewerId INT NOT NULL REFERENCES dbo.Employees(Id),
    Status NVARCHAR(30) NOT NULL DEFAULT 'Pending',
    SelfRating DECIMAL(3,1) NULL, ManagerRating DECIMAL(3,1) NULL, FinalRating DECIMAL(3,1) NULL,
    SelfComments NVARCHAR(MAX) NULL, ManagerComments NVARCHAR(MAX) NULL,
    SelfSubmittedAt DATETIME2 NULL, ManagerSubmittedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL,
    CONSTRAINT UQ_PerformanceReview_CycleEmployee UNIQUE (ReviewCycleId, EmployeeId)
);
GO
IF OBJECT_ID('dbo.PerformanceGoals','U') IS NULL
CREATE TABLE dbo.PerformanceGoals (
    Id INT IDENTITY PRIMARY KEY, ReviewId INT NOT NULL REFERENCES dbo.PerformanceReviews(Id) ON DELETE CASCADE,
    GoalDescription NVARCHAR(1000) NOT NULL, Target NVARCHAR(500) NULL, Achievement NVARCHAR(500) NULL,
    Weight DECIMAL(5,2) NULL, SelfScore DECIMAL(3,1) NULL, ManagerScore DECIMAL(3,1) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO
-- ===== 9. ASSETS =====
IF OBJECT_ID('dbo.AssetCategories','U') IS NULL
CREATE TABLE dbo.AssetCategories (
    Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(150) NOT NULL, Code NVARCHAR(20) NOT NULL UNIQUE,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO
IF OBJECT_ID('dbo.Assets','U') IS NULL
CREATE TABLE dbo.Assets (
    Id INT IDENTITY PRIMARY KEY, AssetCode NVARCHAR(50) NOT NULL UNIQUE, Name NVARCHAR(200) NOT NULL,
    CategoryId INT NOT NULL REFERENCES dbo.AssetCategories(Id),
    SerialNumber NVARCHAR(100) NULL, PurchaseDate DATE NULL, PurchaseCost DECIMAL(18,2) NULL,
    Condition NVARCHAR(30) NOT NULL DEFAULT 'Good', Status NVARCHAR(30) NOT NULL DEFAULT 'Available',
    Notes NVARCHAR(1000) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.AssetAssignments','U') IS NULL
CREATE TABLE dbo.AssetAssignments (
    Id INT IDENTITY PRIMARY KEY, AssetId INT NOT NULL REFERENCES dbo.Assets(Id),
    EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
    AssignedDate DATE NOT NULL, ReturnedDate DATE NULL,
    Condition NVARCHAR(30) NOT NULL DEFAULT 'Good', ReturnCondition NVARCHAR(30) NULL,
    Notes NVARCHAR(500) NULL, IsReturned BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), CreatedBy INT NULL,
    UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
-- ===== 10. COMMUNICATION =====
IF OBJECT_ID('dbo.Announcements','U') IS NULL
CREATE TABLE dbo.Announcements (
    Id INT IDENTITY PRIMARY KEY, Title NVARCHAR(300) NOT NULL, Content NVARCHAR(MAX) NOT NULL,
    DepartmentId INT NULL REFERENCES dbo.Departments(Id), BranchId INT NULL REFERENCES dbo.Branches(Id),
    IsPinned BIT NOT NULL DEFAULT 0, PublishFrom DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    PublishTo DATETIME2 NULL, IsPublished BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NULL, UpdatedAt DATETIME2 NULL, UpdatedBy INT NULL
);
GO
IF OBJECT_ID('dbo.Notifications','U') IS NULL
CREATE TABLE dbo.Notifications (
    Id INT IDENTITY PRIMARY KEY, UserId INT NOT NULL REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    Title NVARCHAR(300) NOT NULL, Message NVARCHAR(MAX) NOT NULL, Type NVARCHAR(50) NOT NULL DEFAULT 'Info',
    EntityType NVARCHAR(100) NULL, EntityId INT NULL,
    IsRead BIT NOT NULL DEFAULT 0, ReadAt DATETIME2 NULL, CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO
-- ===== 11. AUDIT LOG =====
IF OBJECT_ID('dbo.AuditLogs','U') IS NULL
CREATE TABLE dbo.AuditLogs (
    Id BIGINT IDENTITY PRIMARY KEY, UserId INT NULL, Username NVARCHAR(100) NULL,
    Action NVARCHAR(100) NOT NULL, EntityType NVARCHAR(200) NULL, EntityId NVARCHAR(50) NULL,
    OldValues NVARCHAR(MAX) NULL, NewValues NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(50) NULL, UserAgent NVARCHAR(500) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO
-- ===== 12. SEQUENCE TABLE =====
IF OBJECT_ID('dbo.EmployeeCodeSequences','U') IS NULL
CREATE TABLE dbo.EmployeeCodeSequences (Year INT NOT NULL PRIMARY KEY, LastSeq INT NOT NULL DEFAULT 0);
GO
-- ===== 13. VIEWS =====
GO
CREATE OR ALTER VIEW dbo.vw_EmployeeDirectory AS
SELECT e.Id, e.EmployeeCode, e.FullName, e.Gender, e.WorkEmail, e.PersonalPhone, e.WorkPhone,
    e.ProfilePhotoUrl, e.Status, e.EmploymentType, e.JoiningDate, e.ProbationEndDate, e.ConfirmationDate,
    d.Name AS Department, des.Title AS Designation, b.Name AS Branch,
    g.Name AS Grade, sh.Name AS Shift,
    CONCAT(m.FirstName,' ',m.LastName) AS ManagerName, m.Id AS ManagerId
FROM dbo.Employees e
LEFT JOIN dbo.Departments d    ON e.DepartmentId = d.Id
LEFT JOIN dbo.Designations des ON e.DesignationId = des.Id
LEFT JOIN dbo.Branches b       ON e.BranchId = b.Id
LEFT JOIN dbo.Grades g         ON e.GradeId = g.Id
LEFT JOIN dbo.Shifts sh        ON e.ShiftId = sh.Id
LEFT JOIN dbo.Employees m      ON e.ReportingManagerId = m.Id
WHERE e.IsDeleted = 0;
GO
CREATE OR ALTER VIEW dbo.vw_LeaveBalanceSummary AS
SELECT lb.EmployeeId, e.FullName, e.EmployeeCode, lt.Name AS LeaveType, lt.Code AS LeaveTypeCode,
    lb.Year, lb.TotalDays, lb.CarryForwardDays, lb.UsedDays, lb.Balance
FROM dbo.LeaveBalances lb
JOIN dbo.Employees e  ON lb.EmployeeId = e.Id
JOIN dbo.LeaveTypes lt ON lb.LeaveTypeId = lt.Id
WHERE e.IsDeleted = 0;
GO
-- ===== 14. INDEXES =====
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Employees_DepartmentId')
    CREATE INDEX IX_Employees_DepartmentId ON dbo.Employees(DepartmentId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Employees_Status')
    CREATE INDEX IX_Employees_Status ON dbo.Employees(Status) WHERE IsDeleted=0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Employees_BranchId')
    CREATE INDEX IX_Employees_BranchId ON dbo.Employees(BranchId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Employees_ManagerId')
    CREATE INDEX IX_Employees_ManagerId ON dbo.Employees(ReportingManagerId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Attendance_EmployeeDate')
    CREATE INDEX IX_Attendance_EmployeeDate ON dbo.AttendanceLogs(EmployeeId, LogDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Attendance_LogDate')
    CREATE INDEX IX_Attendance_LogDate ON dbo.AttendanceLogs(LogDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LeaveApplications_Employee')
    CREATE INDEX IX_LeaveApplications_Employee ON dbo.LeaveApplications(EmployeeId, Status);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LeaveApplications_Dates')
    CREATE INDEX IX_LeaveApplications_Dates ON dbo.LeaveApplications(StartDate, EndDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_PayrollDetails_RunId')
    CREATE INDEX IX_PayrollDetails_RunId ON dbo.PayrollDetails(PayrollRunId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_PayrollDetails_Employee')
    CREATE INDEX IX_PayrollDetails_Employee ON dbo.PayrollDetails(EmployeeId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_RefreshTokens_UserId')
    CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens(UserId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_AuditLogs_Timestamp')
    CREATE INDEX IX_AuditLogs_Timestamp ON dbo.AuditLogs(Timestamp DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_AuditLogs_EntityType')
    CREATE INDEX IX_AuditLogs_EntityType ON dbo.AuditLogs(EntityType, EntityId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Notifications_UserId')
    CREATE INDEX IX_Notifications_UserId ON dbo.Notifications(UserId, IsRead, CreatedAt DESC);
GO
-- ===== 15. SEED DATA =====
MERGE dbo.Roles AS t USING (VALUES
    ('SuperAdmin','Full system access',1),
    ('HRManager','HR management access',1),
    ('Manager','Team management access',1),
    ('Employee','Self-service access',1)
) AS s (Name,Description,IsSystem) ON t.Name=s.Name
WHEN NOT MATCHED THEN INSERT (Name,Description,IsSystem) VALUES (s.Name,s.Description,s.IsSystem);
GO
MERGE dbo.Permissions AS t USING (VALUES
    ('branches.read','View Branches','Organisation'),('branches.write','Create/Edit Branches','Organisation'),
    ('branches.delete','Delete Branches','Organisation'),('departments.read','View Departments','Organisation'),
    ('departments.write','Create/Edit Departments','Organisation'),('departments.delete','Delete Departments','Organisation'),
    ('designations.read','View Designations','Organisation'),('designations.write','Create/Edit Designations','Organisation'),
    ('grades.read','View Grades','Organisation'),('grades.write','Create/Edit Grades','Organisation'),
    ('shifts.read','View Shifts','Organisation'),('shifts.write','Create/Edit Shifts','Organisation'),
    ('holidays.read','View Holidays','Organisation'),('holidays.write','Create/Edit Holidays','Organisation'),
    ('employees.read','View Employees','Employees'),('employees.write','Create/Edit Employees','Employees'),
    ('employees.delete','Delete Employees','Employees'),('employees.export','Export Employee Data','Employees'),
    ('attendance.read','View Attendance','Attendance'),('attendance.write','Manage Attendance','Attendance'),
    ('attendance.checkin','Check In/Out','Attendance'),('attendance.import','Import Attendance','Attendance'),
    ('attendance.regularize','Regularize Attendance','Attendance'),
    ('leave.read','View Leave','Leave'),('leave.apply','Apply Leave','Leave'),
    ('leave.approve','Approve/Reject Leave','Leave'),('leave.manage','Manage Leave Types','Leave'),
    ('payroll.read','View Payroll','Payroll'),('payroll.process','Process Payroll','Payroll'),
    ('payroll.approve','Approve Payroll','Payroll'),('payroll.payslip','View Own Payslip','Payroll'),
    ('recruitment.read','View Recruitment','Recruitment'),('recruitment.write','Manage Recruitment','Recruitment'),
    ('performance.read','View Performance','Performance'),('performance.write','Manage Performance','Performance'),
    ('assets.read','View Assets','Assets'),('assets.write','Manage Assets','Assets'),
    ('reports.read','View Reports','Reports'),('reports.export','Export Reports','Reports'),
    ('users.read','View Users','Admin'),('users.write','Manage Users','Admin'),
    ('roles.read','View Roles','Admin'),('roles.write','Manage Roles & Permissions','Admin'),
    ('audit.read','View Audit Logs','Admin'),('settings.read','View Settings','Admin'),
    ('settings.write','Manage Settings','Admin'),
    ('announcements.read','View Announcements','Communication'),
    ('announcements.write','Manage Announcements','Communication'),
    ('notifications.read','View Notifications','Communication')
) AS s (Code,Name,Module) ON t.Code=s.Code
WHEN NOT MATCHED THEN INSERT (Code,Name,Module) VALUES (s.Code,s.Name,s.Module);
GO
INSERT INTO dbo.RolePermissions (RoleId,PermissionId)
SELECT r.Id,p.Id FROM dbo.Roles r CROSS JOIN dbo.Permissions p WHERE r.Name='SuperAdmin'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions x WHERE x.RoleId=r.Id AND x.PermissionId=p.Id);
INSERT INTO dbo.RolePermissions (RoleId,PermissionId)
SELECT r.Id,p.Id FROM dbo.Roles r
JOIN dbo.Permissions p ON p.Code IN ('branches.read','departments.read','designations.read','grades.read',
    'shifts.read','holidays.read','holidays.write','employees.read','employees.write','employees.export',
    'attendance.read','attendance.write','attendance.import','attendance.regularize',
    'leave.read','leave.approve','leave.manage','payroll.read','payroll.process','payroll.approve',
    'recruitment.read','recruitment.write','performance.read','performance.write',
    'assets.read','assets.write','reports.read','reports.export','users.read',
    'announcements.read','announcements.write','notifications.read')
WHERE r.Name='HRManager' AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions x WHERE x.RoleId=r.Id AND x.PermissionId=p.Id);
INSERT INTO dbo.RolePermissions (RoleId,PermissionId)
SELECT r.Id,p.Id FROM dbo.Roles r
JOIN dbo.Permissions p ON p.Code IN ('departments.read','designations.read','employees.read',
    'attendance.read','attendance.checkin','leave.read','leave.apply','leave.approve',
    'payroll.payslip','performance.read','performance.write',
    'assets.read','reports.read','announcements.read','notifications.read')
WHERE r.Name='Manager' AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions x WHERE x.RoleId=r.Id AND x.PermissionId=p.Id);
INSERT INTO dbo.RolePermissions (RoleId,PermissionId)
SELECT r.Id,p.Id FROM dbo.Roles r
JOIN dbo.Permissions p ON p.Code IN ('attendance.checkin','attendance.regularize',
    'leave.apply','leave.read','payroll.payslip','performance.read',
    'assets.read','announcements.read','notifications.read')
WHERE r.Name='Employee' AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions x WHERE x.RoleId=r.Id AND x.PermissionId=p.Id);
GO
MERGE dbo.SystemSettings AS t USING (VALUES
    ('company.name','My Company Ltd.','Company','String','Company display name',1),
    ('company.email','hr@company.com','Company','String','HR contact email',1),
    ('company.phone','+1-000-000-0000','Company','String','Company phone',1),
    ('company.address','123 Main St','Company','String','Company address',1),
    ('company.logo_url','' ,'Company','String','Company logo URL',1),
    ('payroll.currency','USD','Payroll','String','Default currency',1),
    ('payroll.ot_rate','1.5','Payroll','Number','Overtime multiplier',0),
    ('leave.fiscal_start','01-01','Leave','String','Fiscal year start MM-DD',0),
    ('attendance.grace','5','Attendance','Number','Default grace minutes',0)
) AS s ([Key],[Value],Category,DataType,Description,IsPublic) ON t.[Key]=s.[Key]
WHEN NOT MATCHED THEN INSERT ([Key],[Value],Category,DataType,Description,IsPublic)
    VALUES (s.[Key],s.[Value],s.Category,s.DataType,s.Description,s.IsPublic);
GO
PRINT 'HRMS schema created/verified successfully.';
GO
