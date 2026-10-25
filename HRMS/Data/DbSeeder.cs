using Microsoft.EntityFrameworkCore;
using HRMS.Data;
using HRMS.Options;
using HRMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HRMS.Data;

/// <summary>
/// Idempotent database seeder. Runs on startup.
/// Creates the SuperAdmin user from config (Seed:AdminEmail / Seed:AdminPassword).
/// Populates demo branches, departments, employees, attendance, leave, payroll,
/// recruitment, performance, assets, announcements, and notifications.
/// </summary>
public class DbSeeder(HrmsDbContext db, IOptions<SeedOptions> seedOpts,
    ILogger<DbSeeder> logger)
{
    private readonly SeedOptions _seed = seedOpts.Value;

    public async Task SeedAsync()
    {
        try
        {
            if (!await db.Database.CanConnectAsync())
            {
                logger.LogWarning("Database not reachable — skipping seed.");
                return;
            }

            await SeedAdminUserAsync();
            await SeedDefaultLeaveTypesAsync();
            await SeedDefaultSalaryComponentsAsync();
            await SeedDemoDataAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database seeding.");
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var exists = await db.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.Username == _seed.AdminUsername || u.Email == _seed.AdminEmail);

        if (exists)
        {
            logger.LogDebug("SuperAdmin user already exists — skip seed.");
            return;
        }

        var hasher = new PasswordHasher<object>();
        var hashPwd = hasher.HashPassword(null!, _seed.AdminPassword);

        await db.Database.ExecuteSqlRawAsync(
            """
            BEGIN TRANSACTION;

            INSERT INTO dbo.Users (Username, Email, PasswordHash, IsActive, MustChangePassword, IsDeleted, CreatedAt)
            VALUES ({0}, {1}, {2}, 1, 0, 0, SYSUTCDATETIME());

            DECLARE @UserId INT = SCOPE_IDENTITY();

            INSERT INTO dbo.UserRoles (UserId, RoleId, AssignedAt)
            SELECT @UserId, Id, SYSUTCDATETIME()
            FROM dbo.Roles WHERE Name = 'SuperAdmin';

            COMMIT;
            """,
            _seed.AdminUsername, _seed.AdminEmail, hashPwd);

        logger.LogInformation("SuperAdmin user '{Username}' seeded successfully.", _seed.AdminUsername);
    }

    private async Task SeedDefaultLeaveTypesAsync()
    {
        if (await db.LeaveTypes.AnyAsync()) return;

        var defaultTypes = new List<LeaveType>
        {
            new() { Name = "Annual Leave", Code = "AL", DaysAllowed = 15, IsCarryForwardAllowed = true, MaxCarryForwardDays = 5, IsPaidLeave = true, IsHalfDayAllowed = true, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Sick Leave", Code = "SL", DaysAllowed = 14, IsCarryForwardAllowed = false, IsPaidLeave = true, IsHalfDayAllowed = true, IsAttachmentRequired = true, AttachmentRequiredAfterDays = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Casual Leave", Code = "CL", DaysAllowed = 10, IsCarryForwardAllowed = false, IsPaidLeave = true, IsHalfDayAllowed = true, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Maternity Leave", Code = "ML", DaysAllowed = 112, IsCarryForwardAllowed = false, GenderRestriction = "Female", IsPaidLeave = true, IsHalfDayAllowed = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Unpaid Leave (LWP)", Code = "LWP", DaysAllowed = 30, IsCarryForwardAllowed = false, IsPaidLeave = false, IsHalfDayAllowed = true, IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        db.LeaveTypes.AddRange(defaultTypes);
        await db.SaveChangesAsync();
        logger.LogInformation("Default leave types seeded.");
    }

    private async Task SeedDefaultSalaryComponentsAsync()
    {
        if (await db.SalaryComponents.AnyAsync()) return;

        var defaultComponents = new List<SalaryComponent>
        {
            new() { Name = "Basic Salary", Code = "BASIC", Type = "Earning", IsFixed = true, IsTaxable = true, IsActive = true, SortOrder = 1, CreatedAt = DateTime.UtcNow },
            new() { Name = "House Rent Allowance", Code = "HRA", Type = "Earning", IsFixed = false, IsTaxable = true, IsActive = true, SortOrder = 2, CreatedAt = DateTime.UtcNow },
            new() { Name = "Medical Allowance", Code = "MED", Type = "Earning", IsFixed = false, IsTaxable = false, IsActive = true, SortOrder = 3, CreatedAt = DateTime.UtcNow },
            new() { Name = "Conveyance Allowance", Code = "CONV", Type = "Earning", IsFixed = false, IsTaxable = false, IsActive = true, SortOrder = 4, CreatedAt = DateTime.UtcNow },
            new() { Name = "Performance Bonus", Code = "BONUS", Type = "Earning", IsFixed = false, IsTaxable = true, IsActive = true, SortOrder = 5, CreatedAt = DateTime.UtcNow },
            new() { Name = "Provident Fund", Code = "PF", Type = "Deduction", IsFixed = false, IsTaxable = false, IsActive = true, SortOrder = 6, CreatedAt = DateTime.UtcNow },
            new() { Name = "Income Tax / TDS", Code = "TAX", Type = "Deduction", IsFixed = false, IsTaxable = false, IsActive = true, SortOrder = 7, CreatedAt = DateTime.UtcNow },
            new() { Name = "Health Insurance", Code = "INS", Type = "Deduction", IsFixed = true, IsTaxable = false, IsActive = true, SortOrder = 8, CreatedAt = DateTime.UtcNow }
        };

        db.SalaryComponents.AddRange(defaultComponents);
        await db.SaveChangesAsync();
        logger.LogInformation("Default salary components seeded.");
    }

    private async Task SeedDemoDataAsync()
    {
        // 1. Branches
        if (!await db.Branches.AnyAsync(b => b.Code == "DHK-HQ"))
        {
            db.Branches.Add(new Branch
            {
                Name = "Dhaka Head Office",
                Code = "DHK-HQ",
                Address = "Gulshan Avenue 12, Tower B",
                City = "Dhaka",
                Country = "Bangladesh",
                Phone = "+880-1700-112233",
                Email = "hq@company.com",
                IsHeadOffice = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        if (!await db.Branches.AnyAsync(b => b.Code == "CTG-HUB"))
        {
            db.Branches.Add(new Branch
            {
                Name = "Chattogram Hub",
                Code = "CTG-HUB",
                Address = "Agrabad C/A",
                City = "Chattogram",
                Country = "Bangladesh",
                Phone = "+880-1800-445566",
                Email = "ctg@company.com",
                IsHeadOffice = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();

        var hqBranch = await db.Branches.FirstAsync(b => b.Code == "DHK-HQ");

        // 2. Shifts
        if (!await db.Shifts.AnyAsync(s => s.Code == "DAY-9-6"))
        {
            db.Shifts.Add(new Shift
            {
                Name = "Standard Day Shift",
                Code = "DAY-9-6",
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(18, 0),
                GraceMinutes = 15,
                WorkingHours = 8.0m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();
        var defaultShift = await db.Shifts.FirstAsync(s => s.Code == "DAY-9-6");

        // 3. Grades
        if (!await db.Grades.AnyAsync(g => g.Code == "GR-SENIOR"))
        {
            db.Grades.Add(new Grade { Name = "Senior Grade", Code = "GR-SENIOR", MinSalary = 80000, MaxSalary = 150000, IsActive = true, CreatedAt = DateTime.UtcNow });
            db.Grades.Add(new Grade { Name = "Mid Grade", Code = "GR-MID", MinSalary = 50000, MaxSalary = 80000, IsActive = true, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
        var gradeSenior = await db.Grades.FirstAsync(g => g.Code == "GR-SENIOR");

        // 4. Departments
        if (!await db.Departments.AnyAsync(d => d.Code == "DEP-ENG"))
        {
            db.Departments.Add(new Department { Name = "Engineering", Code = "DEP-ENG", BranchId = hqBranch.Id, Description = "Software Engineering & Tech", IsActive = true, CreatedAt = DateTime.UtcNow });
            db.Departments.Add(new Department { Name = "Human Resources", Code = "DEP-HR", BranchId = hqBranch.Id, Description = "People & Culture", IsActive = true, CreatedAt = DateTime.UtcNow });
            db.Departments.Add(new Department { Name = "Finance & Accounts", Code = "DEP-FIN", BranchId = hqBranch.Id, Description = "Financial management & payroll", IsActive = true, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
        var deptEng = await db.Departments.FirstAsync(d => d.Code == "DEP-ENG");
        var deptHr = await db.Departments.FirstAsync(d => d.Code == "DEP-HR");
        var deptFin = await db.Departments.FirstAsync(d => d.Code == "DEP-FIN");

        // 5. Designations
        if (!await db.Designations.AnyAsync(d => d.Code == "DSG-SWE"))
        {
            db.Designations.Add(new Designation { Title = "Senior Software Engineer", Code = "DSG-SWE", DepartmentId = deptEng.Id, IsActive = true, CreatedAt = DateTime.UtcNow });
            db.Designations.Add(new Designation { Title = "HR Business Partner", Code = "DSG-HRBP", DepartmentId = deptHr.Id, IsActive = true, CreatedAt = DateTime.UtcNow });
            db.Designations.Add(new Designation { Title = "Payroll Specialist", Code = "DSG-PAY", DepartmentId = deptFin.Id, IsActive = true, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
        var desigSwe = await db.Designations.FirstAsync(d => d.Code == "DSG-SWE");
        var desigHrbp = await db.Designations.FirstAsync(d => d.Code == "DSG-HRBP");

        // 6. Demo Employees
        if (!await db.Employees.AnyAsync())
        {
            var emp1 = new Employee
            {
                EmployeeCode = "EMP-2026-0001",
                FirstName = "Arifur",
                LastName = "Rahman",
                Gender = "Male",
                DateOfBirth = new DateOnly(1993, 5, 14),
                WorkEmail = "arif.rahman@hrms.local",
                PersonalPhone = "+880-1711-223344",
                BranchId = hqBranch.Id,
                DepartmentId = deptEng.Id,
                DesignationId = desigSwe.Id,
                GradeId = gradeSenior.Id,
                ShiftId = defaultShift.Id,
                JoiningDate = new DateOnly(2023, 1, 15),
                EmploymentType = "Full-Time",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            var emp2 = new Employee
            {
                EmployeeCode = "EMP-2026-0002",
                FirstName = "Nusrat",
                LastName = "Jahan",
                Gender = "Female",
                DateOfBirth = new DateOnly(1995, 8, 22),
                WorkEmail = "nusrat.jahan@hrms.local",
                PersonalPhone = "+880-1811-334455",
                BranchId = hqBranch.Id,
                DepartmentId = deptHr.Id,
                DesignationId = desigHrbp.Id,
                GradeId = gradeSenior.Id,
                ShiftId = defaultShift.Id,
                JoiningDate = new DateOnly(2023, 6, 1),
                EmploymentType = "Full-Time",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            db.Employees.AddRange(emp1, emp2);
            await db.SaveChangesAsync();

            // Setup Salary Structures
            db.SalaryStructures.Add(new SalaryStructure
            {
                EmployeeId = emp1.Id,
                EffectiveFrom = new DateOnly(2026, 1, 1),
                GrossSalary = 120000,
                IsCurrent = true,
                CreatedAt = DateTime.UtcNow
            });
            db.SalaryStructures.Add(new SalaryStructure
            {
                EmployeeId = emp2.Id,
                EffectiveFrom = new DateOnly(2026, 1, 1),
                GrossSalary = 95000,
                IsCurrent = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // 6b. Demo Users for Employees (Automatic employee login setup)
        var empRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Employee");
        var activeEmps = await db.Employees.Where(e => e.Status == "Active").Take(2).ToListAsync();
        if (empRole != null && activeEmps.Count > 0)
        {
            var hasher = new PasswordHasher<object>();
            var defaultUserPwd = hasher.HashPassword(null!, "Password@123");

            foreach (var emp in activeEmps)
            {
                var username = (emp.FirstName ?? "employee").ToLowerInvariant().Trim();
                if (!await db.Users.AnyAsync(u => u.Username == username || u.EmployeeId == emp.Id))
                {
                    var u = new User
                    {
                        Username = username,
                        Email = emp.WorkEmail,
                        PasswordHash = defaultUserPwd,
                        EmployeeId = emp.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    db.Users.Add(u);
                    await db.SaveChangesAsync();
                    db.UserRoles.Add(new UserRole { UserId = u.Id, RoleId = empRole.Id, AssignedAt = DateTime.UtcNow });
                    await db.SaveChangesAsync();
                }
            }
        }

        var demoEmp = await db.Employees.FirstAsync();

        // 7. Attendance Logs for Today
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (!await db.AttendanceLogs.AnyAsync(a => a.EmployeeId == demoEmp.Id && a.LogDate == today))
        {
            db.AttendanceLogs.Add(new AttendanceLog
            {
                EmployeeId = demoEmp.Id,
                LogDate = today,
                CheckInTime = DateTime.UtcNow.Date.AddHours(9).AddMinutes(5),
                Status = "Present",
                IsLate = false,
                WorkingMinutes = 480,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // 8. Recruitment Job Openings & Candidate
        if (!await db.JobOpenings.AnyAsync(j => j.Title.Contains("Fullstack Engineer")))
        {
            var job = new JobOpening
            {
                Title = "Senior Fullstack Engineer (.NET & React)",
                DepartmentId = deptEng.Id,
                DesignationId = desigSwe.Id,
                BranchId = hqBranch.Id,
                Vacancies = 2,
                EmploymentType = "Full-Time",
                ExperienceMinYrs = 4,
                ExperienceMaxYrs = 7,
                SalaryMin = 90000,
                SalaryMax = 140000,
                Description = "Looking for an experienced engineer to build enterprise cloud solutions.",
                Requirements = ".NET 10, C#, React 19, TypeScript, SQL Server, Clean Architecture",
                PostedDate = today.AddDays(-10),
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };
            db.JobOpenings.Add(job);
            await db.SaveChangesAsync();

            var cand = new Candidate
            {
                FirstName = "Tanvir",
                LastName = "Hasan",
                Email = "tanvir.hasan.dev@example.com",
                Phone = "+880-1799-887766",
                LinkedInUrl = "https://linkedin.com/in/tanvir-hasan",
                Source = "LinkedIn",
                CreatedAt = DateTime.UtcNow
            };
            db.Candidates.Add(cand);
            await db.SaveChangesAsync();

            var app = new Application
            {
                JobOpeningId = job.Id,
                CandidateId = cand.Id,
                Stage = "Interview",
                StageOrder = 3,
                Notes = "Strong technical background, shortlisted for technical interview.",
                CreatedAt = DateTime.UtcNow
            };
            db.Applications.Add(app);
            await db.SaveChangesAsync();

            db.Interviews.Add(new Interview
            {
                ApplicationId = app.Id,
                InterviewerId = demoEmp.Id,
                ScheduledAt = DateTime.UtcNow.AddDays(2),
                Mode = "Online",
                MeetingLink = "https://meet.google.com/hrms-demo-interview",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // 9. Performance Review Cycle & Goal
        if (!await db.ReviewCycles.AnyAsync(r => r.Year == today.Year))
        {
            var cycle = new ReviewCycle
            {
                Name = $"Annual Performance Review {today.Year}",
                Year = today.Year,
                StartDate = new DateOnly(today.Year, 1, 1),
                EndDate = new DateOnly(today.Year, 12, 31),
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            db.ReviewCycles.Add(cycle);
            await db.SaveChangesAsync();

            var pReview = new PerformanceReview
            {
                ReviewCycleId = cycle.Id,
                EmployeeId = demoEmp.Id,
                ReviewerId = demoEmp.Id,
                Status = "Pending",
                SelfRating = 4.5m,
                SelfComments = "Delivered all core platform features ahead of schedule.",
                CreatedAt = DateTime.UtcNow
            };
            db.PerformanceReviews.Add(pReview);
            await db.SaveChangesAsync();

            db.PerformanceGoals.Add(new PerformanceGoal
            {
                ReviewId = pReview.Id,
                GoalDescription = "Complete HRMS enterprise modules and UI workflow",
                Target = "100% Feature Complete",
                Achievement = "Completed",
                Weight = 40.0m,
                SelfScore = 4.8m,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // 10. Asset Categories & Assets
        if (!await db.AssetCategories.AnyAsync(c => c.Code == "LAPTOP"))
        {
            var catLaptop = new AssetCategory { Name = "Laptops & Computers", Code = "LAPTOP", CreatedAt = DateTime.UtcNow };
            var catMonitor = new AssetCategory { Name = "Displays & Monitors", Code = "MONITOR", CreatedAt = DateTime.UtcNow };
            db.AssetCategories.AddRange(catLaptop, catMonitor);
            await db.SaveChangesAsync();

            var asset1 = new Asset
            {
                AssetCode = "AST-MBP-01",
                Name = "MacBook Pro 16\" M3 Pro (36GB RAM)",
                CategoryId = catLaptop.Id,
                SerialNumber = "C02XYZ12345",
                PurchaseDate = new DateOnly(2024, 2, 10),
                PurchaseCost = 2850.00m,
                Condition = "Good",
                Status = "Assigned",
                Notes = "Assigned to Lead Engineer",
                CreatedAt = DateTime.UtcNow
            };

            var asset2 = new Asset
            {
                AssetCode = "AST-DELL-02",
                Name = "Dell UltraSharp 27\" 4K Monitor",
                CategoryId = catMonitor.Id,
                SerialNumber = "CN-0987654",
                PurchaseDate = new DateOnly(2024, 3, 1),
                PurchaseCost = 550.00m,
                Condition = "Good",
                Status = "Available",
                Notes = "In storage inventory",
                CreatedAt = DateTime.UtcNow
            };

            db.Assets.AddRange(asset1, asset2);
            await db.SaveChangesAsync();

            db.AssetAssignments.Add(new AssetAssignment
            {
                AssetId = asset1.Id,
                EmployeeId = demoEmp.Id,
                AssignedDate = new DateOnly(2024, 2, 15),
                Condition = "Good",
                IsReturned = false,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // 11. Announcements
        if (!await db.Announcements.AnyAsync(a => a.Title.Contains("HRMS Enterprise Portal")))
        {
            db.Announcements.Add(new Announcement
            {
                Title = "Welcome to HRMS Enterprise Portal v1.0",
                Content = "All staff and administrators now have full access to workforce, attendance, leaves, payroll, recruitment pipeline, and performance appraisal modules.",
                IsPinned = true,
                PublishFrom = DateTime.UtcNow.AddDays(-1),
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            });
            db.Announcements.Add(new Announcement
            {
                Title = "Upcoming Company Townhall & Review Meeting",
                Content = "The quarterly executive townhall will be held next Thursday at 3:00 PM in the Head Office conference hall and streaming online.",
                IsPinned = false,
                PublishFrom = DateTime.UtcNow.AddDays(-1),
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // 12. Notifications for SuperAdmin User
        var adminUser = await db.Users.FirstOrDefaultAsync(u => u.Username == _seed.AdminUsername);
        if (adminUser != null && !await db.Notifications.AnyAsync(n => n.UserId == adminUser.Id))
        {
            db.Notifications.Add(new Notification
            {
                UserId = adminUser.Id,
                Title = "System Ready",
                Message = "HRMS Enterprise backend and frontend modules are fully initialized.",
                Type = "Success",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            db.Notifications.Add(new Notification
            {
                UserId = adminUser.Id,
                Title = "New Job Application",
                Message = "Tanvir Hasan applied for Senior Fullstack Engineer position.",
                Type = "Info",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        logger.LogInformation("Enterprise demo data initialized successfully.");
    }
}
