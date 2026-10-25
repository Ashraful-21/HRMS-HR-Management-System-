using ClosedXML.Excel;
using HRMS.Constants;
using HRMS.Data;
using HRMS.Dtos.Common;
using HRMS.Dtos.Employees;
using HRMS.IRepositories;
using HRMS.IServices;
using HRMS.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Services;

public class EmployeeService(
    HrmsDbContext db,
    IUnitOfWork uow,
    IFileStorageService fileStorage,
    IAuditService auditService,
    ILogger<EmployeeService> logger) : IEmployeeService
{
    // ==========================================
    // EMPLOYEE MAIN CRUD
    // ==========================================
    public async Task<ApiResponse<PagedResult<EmployeeSummaryDto>>> GetEmployeesPagedAsync(EmployeeFilterQuery query)
    {
        var q = uow.Repository<Employee>().Query()
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .AsQueryable();

        // Filters
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(e => e.FullName.ToLower().Contains(search) ||
                             e.EmployeeCode.ToLower().Contains(search) ||
                             e.WorkEmail.ToLower().Contains(search) ||
                             (e.WorkPhone != null && e.WorkPhone.Contains(search)));
        }

        if (query.BranchId.HasValue)
            q = q.Where(e => e.BranchId == query.BranchId.Value);

        if (query.DepartmentId.HasValue)
            q = q.Where(e => e.DepartmentId == query.DepartmentId.Value);

        if (query.DesignationId.HasValue)
            q = q.Where(e => e.DesignationId == query.DesignationId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(e => e.Status == query.Status);

        if (query.JoiningFrom.HasValue)
            q = q.Where(e => e.JoiningDate >= query.JoiningFrom.Value);

        if (query.JoiningTo.HasValue)
            q = q.Where(e => e.JoiningDate <= query.JoiningTo.Value);

        var total = await q.CountAsync();

        var items = await q
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new EmployeeSummaryDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Gender = e.Gender,
                WorkEmail = e.WorkEmail,
                WorkPhone = e.WorkPhone,
                BranchId = e.BranchId,
                BranchName = e.Branch.Name,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department.Name,
                DesignationId = e.DesignationId,
                DesignationTitle = e.Designation.Title,
                Status = e.Status,
                EmploymentType = e.EmploymentType,
                JoiningDate = e.JoiningDate,
                ProfilePhotoUrl = e.ProfilePhotoUrl
            })
            .ToListAsync();

        return ApiResponse<PagedResult<EmployeeSummaryDto>>.Ok(new PagedResult<EmployeeSummaryDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<EmployeeDetailDto>> GetEmployeeByIdAsync(int id)
    {
        var e = await uow.Repository<Employee>().Query()
            .Include(x => x.Branch)
            .Include(x => x.Department)
            .Include(x => x.Designation)
            .Include(x => x.Grade)
            .Include(x => x.Shift)
            .Include(x => x.ReportingManager)
            .Include(x => x.EmployeeAddresses)
            .Include(x => x.EmployeeEmergencyContacts)
            .Include(x => x.EmployeeBankAccounts)
            .Include(x => x.EmployeeEducations)
            .Include(x => x.EmployeeExperiences)
            .Include(x => x.EmployeeDocuments.Where(d => !d.IsDeleted))
            .Include(x => x.EmployeeLifecycleEvents)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (e == null)
            return ApiResponse<EmployeeDetailDto>.Fail("Employee not found.");

        var dto = MapToDetailDto(e);
        var user = await db.Users
            .AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.EmployeeId == id && !u.IsDeleted);

        if (user != null)
        {
            dto.PortalAccount = new EmployeePortalAccountDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                MustChangePassword = user.MustChangePassword,
                LastLoginAt = user.LastLoginAt,
                Roles = user.UserRoles.Where(r => !r.Role.IsDeleted).Select(r => r.Role.Name).ToList()
            };
        }

        return ApiResponse<EmployeeDetailDto>.Ok(dto);
    }

    public async Task<ApiResponse<EmployeeDetailDto>> CreateEmployeeAsync(CreateEmployeeRequest req, int? userId)
    {
        // Unique email check
        var emailExists = await uow.Repository<Employee>().Query()
            .AnyAsync(e => e.WorkEmail.ToLower() == req.WorkEmail.Trim().ToLower());
        if (emailExists)
            return ApiResponse<EmployeeDetailDto>.Fail($"Work email '{req.WorkEmail}' is already registered.");

        // Atomic code generation
        var code = await GenerateEmployeeCodeAsync();

        var employee = new Employee
        {
            EmployeeCode = code,
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            FullName = $"{req.FirstName.Trim()} {req.LastName.Trim()}",
            Gender = req.Gender,
            DateOfBirth = req.DateOfBirth,
            NationalId = req.NationalId,
            BloodGroup = req.BloodGroup,
            PersonalEmail = req.PersonalEmail,
            WorkEmail = req.WorkEmail.Trim().ToLower(),
            PersonalPhone = req.PersonalPhone,
            WorkPhone = req.WorkPhone,
            MaritalStatus = req.MaritalStatus,
            Nationality = req.Nationality,
            Religion = req.Religion,
            BranchId = req.BranchId,
            DepartmentId = req.DepartmentId,
            DesignationId = req.DesignationId,
            GradeId = req.GradeId,
            ShiftId = req.ShiftId,
            ReportingManagerId = req.ReportingManagerId,
            JoiningDate = req.JoiningDate,
            ProbationEndDate = req.ProbationEndDate,
            EmploymentType = req.EmploymentType,
            Status = req.Status,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await uow.Repository<Employee>().AddAsync(employee);
        await uow.SaveChangesAsync();

        // Add automatic Onboarding lifecycle event
        var onboardingEvent = new EmployeeLifecycleEvent
        {
            EmployeeId = employee.Id,
            EventType = "Onboarding",
            EventDate = req.JoiningDate,
            NewValue = $"Joined as {req.EmploymentType} ({req.Status})",
            Remarks = "Initial employee onboarding",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        await uow.Repository<EmployeeLifecycleEvent>().AddAsync(onboardingEvent);
        await auditService.LogAsync(AuditActions.Create, "Employees", employee.Id.ToString(), null, new { employee.EmployeeCode, employee.FullName, employee.WorkEmail });

        // Optional portal login creation
        if (req.CreatePortalAccount && !string.IsNullOrWhiteSpace(req.PortalPassword))
        {
            var username = !string.IsNullOrWhiteSpace(req.PortalUsername)
                ? req.PortalUsername.Trim().ToLower()
                : employee.WorkEmail.Split('@')[0].ToLower();

            if (await db.Users.AnyAsync(u => u.Username.ToLower() == username))
            {
                username = $"{username}{System.Security.Cryptography.RandomNumberGenerator.GetInt32(10, 99)}";
            }

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            var user = new User
            {
                Username = username,
                Email = employee.WorkEmail.Trim().ToLower(),
                PasswordHash = string.Empty,
                EmployeeId = employee.Id,
                IsActive = true,
                MustChangePassword = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            user.PasswordHash = hasher.HashPassword(user, req.PortalPassword);

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var roleName = !string.IsNullOrWhiteSpace(req.PortalRole) ? req.PortalRole : "Employee";
            var role = await db.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower() && !r.IsDeleted);
            if (role != null)
            {
                db.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = userId
                });
                await db.SaveChangesAsync();
            }

            await auditService.LogAsync(AuditActions.Create, "Users", user.Id.ToString(), null, new { Action = "CreateEmployeePortalAccount", employee.Id, user.Username });
        }

        return await GetEmployeeByIdAsync(employee.Id);
    }

    public async Task<ApiResponse<EmployeeDetailDto>> UpdateEmployeeAsync(int id, UpdateEmployeeRequest req, int? userId)
    {
        var employee = await uow.Repository<Employee>().GetByIdAsync(id);
        if (employee == null)
            return ApiResponse<EmployeeDetailDto>.Fail("Employee not found.");

        var emailExists = await uow.Repository<Employee>().Query()
            .AnyAsync(e => e.Id != id && e.WorkEmail.ToLower() == req.WorkEmail.Trim().ToLower());
        if (emailExists)
            return ApiResponse<EmployeeDetailDto>.Fail($"Work email '{req.WorkEmail}' is already registered.");

        // Check for department / designation transfer
        if (employee.DepartmentId != req.DepartmentId || employee.DesignationId != req.DesignationId)
        {
            var transferEvent = new EmployeeLifecycleEvent
            {
                EmployeeId = id,
                EventType = "Transfer",
                EventDate = DateOnly.FromDateTime(DateTime.UtcNow),
                OldValue = $"Dept: {employee.DepartmentId}, Desig: {employee.DesignationId}",
                NewValue = $"Dept: {req.DepartmentId}, Desig: {req.DesignationId}",
                Remarks = "Internal department/designation update",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            await uow.Repository<EmployeeLifecycleEvent>().AddAsync(transferEvent);
        }

        // Check for status change
        if (employee.Status != req.Status)
        {
            var statusEvent = new EmployeeLifecycleEvent
            {
                EmployeeId = id,
                EventType = req.Status == "Resigned" ? "Resignation" : req.Status == "Terminated" ? "Termination" : "StatusChange",
                EventDate = DateOnly.FromDateTime(DateTime.UtcNow),
                OldValue = employee.Status,
                NewValue = req.Status,
                Remarks = req.TerminationReason ?? "Status updated",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            await uow.Repository<EmployeeLifecycleEvent>().AddAsync(statusEvent);
        }

        employee.FirstName = req.FirstName.Trim();
        employee.LastName = req.LastName.Trim();
        employee.FullName = $"{req.FirstName.Trim()} {req.LastName.Trim()}";
        employee.Gender = req.Gender;
        employee.DateOfBirth = req.DateOfBirth;
        employee.NationalId = req.NationalId;
        employee.BloodGroup = req.BloodGroup;
        employee.PersonalEmail = req.PersonalEmail;
        employee.WorkEmail = req.WorkEmail.Trim().ToLower();
        employee.PersonalPhone = req.PersonalPhone;
        employee.WorkPhone = req.WorkPhone;
        employee.MaritalStatus = req.MaritalStatus;
        employee.Nationality = req.Nationality;
        employee.Religion = req.Religion;
        employee.BranchId = req.BranchId;
        employee.DepartmentId = req.DepartmentId;
        employee.DesignationId = req.DesignationId;
        employee.GradeId = req.GradeId;
        employee.ShiftId = req.ShiftId;
        employee.ReportingManagerId = req.ReportingManagerId;
        employee.JoiningDate = req.JoiningDate;
        employee.ProbationEndDate = req.ProbationEndDate;
        employee.ConfirmationDate = req.ConfirmationDate;
        employee.EmploymentType = req.EmploymentType;
        employee.Status = req.Status;
        employee.ResignationDate = req.ResignationDate;
        employee.LastWorkingDate = req.LastWorkingDate;
        employee.TerminationReason = req.TerminationReason;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Employees", employee.Id.ToString(), null, new { employee.EmployeeCode, employee.FullName });

        return await GetEmployeeByIdAsync(id);
    }

    public async Task<ApiResponse> DeleteEmployeeAsync(int id, int? userId)
    {
        var employee = await uow.Repository<Employee>().GetByIdAsync(id);
        if (employee == null)
            return ApiResponse.Fail("Employee not found.");

        employee.IsDeleted = true;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedBy = userId;

        // Cascade soft delete documents
        var docs = await uow.Repository<EmployeeDocument>().Query()
            .Where(d => d.EmployeeId == id && !d.IsDeleted)
            .ToListAsync();

        foreach (var doc in docs)
        {
            doc.IsDeleted = true;
            doc.UpdatedAt = DateTime.UtcNow;
            doc.UpdatedBy = userId;
        }

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Delete, "Employees", employee.Id.ToString(), null, null);

        return ApiResponse.Ok("Employee deleted successfully.");
    }

    public async Task<ApiResponse<string>> UploadProfilePhotoAsync(int id, IFormFile file, int? userId)
    {
        var employee = await uow.Repository<Employee>().GetByIdAsync(id);
        if (employee == null)
            return ApiResponse<string>.Fail("Employee not found.");

        if (file.Length == 0)
            return ApiResponse<string>.Fail("No file provided.");

        var (url, publicId) = await fileStorage.UploadAsync(file, "profile-photos");

        employee.ProfilePhotoUrl = url;
        employee.ProfilePhotoPublicId = publicId;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Employees", id.ToString(), null, new { Action = "UploadProfilePhoto", Url = url });

        return ApiResponse<string>.Ok(url, "Profile photo uploaded successfully.");
    }

    // ==========================================
    // SUB-ENTITIES: ADDRESSES
    // ==========================================
    public async Task<ApiResponse<EmployeeAddressDto>> AddAddressAsync(int employeeId, UpsertEmployeeAddressRequest req, int? userId)
    {
        var empExists = await uow.Repository<Employee>().ExistsAsync(employeeId);
        if (!empExists) return ApiResponse<EmployeeAddressDto>.Fail("Employee not found.");

        var addr = new EmployeeAddress
        {
            EmployeeId = employeeId,
            AddressType = req.AddressType,
            Street = req.Street,
            City = req.City,
            State = req.State,
            PostalCode = req.PostalCode,
            Country = req.Country,
            CreatedAt = DateTime.UtcNow
        };

        await uow.Repository<EmployeeAddress>().AddAsync(addr);
        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Create, "EmployeeAddresses", addr.Id.ToString(), null, addr);

        return ApiResponse<EmployeeAddressDto>.Ok(new EmployeeAddressDto
        {
            Id = addr.Id,
            EmployeeId = addr.EmployeeId,
            AddressType = addr.AddressType,
            Street = addr.Street,
            City = addr.City,
            State = addr.State,
            PostalCode = addr.PostalCode,
            Country = addr.Country
        });
    }

    public async Task<ApiResponse> DeleteAddressAsync(int employeeId, int addressId, int? userId)
    {
        var addr = await uow.Repository<EmployeeAddress>().GetByIdAsync(addressId);
        if (addr == null || addr.EmployeeId != employeeId)
            return ApiResponse.Fail("Address not found.");

        uow.Repository<EmployeeAddress>().Remove(addr);
        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Delete, "EmployeeAddresses", addressId.ToString(), null, null);

        return ApiResponse.Ok("Address removed successfully.");
    }

    // ==========================================
    // SUB-ENTITIES: EMERGENCY CONTACTS
    // ==========================================
    public async Task<ApiResponse<EmployeeEmergencyContactDto>> AddEmergencyContactAsync(int employeeId, UpsertEmergencyContactRequest req, int? userId)
    {
        var empExists = await uow.Repository<Employee>().ExistsAsync(employeeId);
        if (!empExists) return ApiResponse<EmployeeEmergencyContactDto>.Fail("Employee not found.");

        if (req.IsPrimary)
        {
            var existingPrimaries = await uow.Repository<EmployeeEmergencyContact>().Query()
                .Where(c => c.EmployeeId == employeeId && c.IsPrimary)
                .ToListAsync();
            foreach (var p in existingPrimaries) p.IsPrimary = false;
        }

        var contact = new EmployeeEmergencyContact
        {
            EmployeeId = employeeId,
            Name = req.Name,
            Relationship = req.Relationship,
            Phone = req.Phone,
            Email = req.Email,
            Address = req.Address,
            IsPrimary = req.IsPrimary,
            CreatedAt = DateTime.UtcNow
        };

        await uow.Repository<EmployeeEmergencyContact>().AddAsync(contact);
        await uow.SaveChangesAsync();

        return ApiResponse<EmployeeEmergencyContactDto>.Ok(new EmployeeEmergencyContactDto
        {
            Id = contact.Id,
            EmployeeId = contact.EmployeeId,
            Name = contact.Name,
            Relationship = contact.Relationship,
            Phone = contact.Phone,
            Email = contact.Email,
            Address = contact.Address,
            IsPrimary = contact.IsPrimary
        });
    }

    public async Task<ApiResponse> DeleteEmergencyContactAsync(int employeeId, int contactId, int? userId)
    {
        var c = await uow.Repository<EmployeeEmergencyContact>().GetByIdAsync(contactId);
        if (c == null || c.EmployeeId != employeeId) return ApiResponse.Fail("Emergency contact not found.");

        uow.Repository<EmployeeEmergencyContact>().Remove(c);
        await uow.SaveChangesAsync();

        return ApiResponse.Ok("Emergency contact removed successfully.");
    }

    // ==========================================
    // SUB-ENTITIES: BANK ACCOUNTS
    // ==========================================
    public async Task<ApiResponse<EmployeeBankAccountDto>> AddBankAccountAsync(int employeeId, UpsertBankAccountRequest req, int? userId)
    {
        var empExists = await uow.Repository<Employee>().ExistsAsync(employeeId);
        if (!empExists) return ApiResponse<EmployeeBankAccountDto>.Fail("Employee not found.");

        if (req.IsPrimary)
        {
            var existingPrimaries = await uow.Repository<EmployeeBankAccount>().Query()
                .Where(b => b.EmployeeId == employeeId && b.IsPrimary)
                .ToListAsync();
            foreach (var p in existingPrimaries) p.IsPrimary = false;
        }

        var account = new EmployeeBankAccount
        {
            EmployeeId = employeeId,
            BankName = req.BankName,
            AccountNumber = req.AccountNumber,
            AccountTitle = req.AccountTitle,
            BranchName = req.BranchName,
            RoutingNumber = req.RoutingNumber,
            SwiftCode = req.SwiftCode,
            IsPrimary = req.IsPrimary,
            CreatedAt = DateTime.UtcNow
        };

        await uow.Repository<EmployeeBankAccount>().AddAsync(account);
        await uow.SaveChangesAsync();

        return ApiResponse<EmployeeBankAccountDto>.Ok(new EmployeeBankAccountDto
        {
            Id = account.Id,
            EmployeeId = account.EmployeeId,
            BankName = account.BankName,
            AccountNumber = account.AccountNumber,
            AccountTitle = account.AccountTitle,
            BranchName = account.BranchName,
            RoutingNumber = account.RoutingNumber,
            SwiftCode = account.SwiftCode,
            IsPrimary = account.IsPrimary
        });
    }

    public async Task<ApiResponse> DeleteBankAccountAsync(int employeeId, int accountId, int? userId)
    {
        var b = await uow.Repository<EmployeeBankAccount>().GetByIdAsync(accountId);
        if (b == null || b.EmployeeId != employeeId) return ApiResponse.Fail("Bank account not found.");

        uow.Repository<EmployeeBankAccount>().Remove(b);
        await uow.SaveChangesAsync();

        return ApiResponse.Ok("Bank account removed successfully.");
    }

    // ==========================================
    // SUB-ENTITIES: EDUCATION
    // ==========================================
    public async Task<ApiResponse<EmployeeEducationDto>> AddEducationAsync(int employeeId, UpsertEducationRequest req, int? userId)
    {
        var empExists = await uow.Repository<Employee>().ExistsAsync(employeeId);
        if (!empExists) return ApiResponse<EmployeeEducationDto>.Fail("Employee not found.");

        var edu = new EmployeeEducation
        {
            EmployeeId = employeeId,
            Degree = req.Degree,
            Institution = req.Institution,
            FieldOfStudy = req.FieldOfStudy,
            StartYear = req.StartYear,
            EndYear = req.EndYear,
            Grade = req.Grade,
            IsHighest = req.IsHighest,
            CreatedAt = DateTime.UtcNow
        };

        await uow.Repository<EmployeeEducation>().AddAsync(edu);
        await uow.SaveChangesAsync();

        return ApiResponse<EmployeeEducationDto>.Ok(new EmployeeEducationDto
        {
            Id = edu.Id,
            EmployeeId = edu.EmployeeId,
            Degree = edu.Degree,
            Institution = edu.Institution,
            FieldOfStudy = edu.FieldOfStudy,
            StartYear = edu.StartYear,
            EndYear = edu.EndYear,
            Grade = edu.Grade,
            IsHighest = edu.IsHighest
        });
    }

    public async Task<ApiResponse> DeleteEducationAsync(int employeeId, int educationId, int? userId)
    {
        var edu = await uow.Repository<EmployeeEducation>().GetByIdAsync(educationId);
        if (edu == null || edu.EmployeeId != employeeId) return ApiResponse.Fail("Education record not found.");

        uow.Repository<EmployeeEducation>().Remove(edu);
        await uow.SaveChangesAsync();

        return ApiResponse.Ok("Education record removed successfully.");
    }

    // ==========================================
    // SUB-ENTITIES: EXPERIENCE
    // ==========================================
    public async Task<ApiResponse<EmployeeExperienceDto>> AddExperienceAsync(int employeeId, UpsertExperienceRequest req, int? userId)
    {
        var empExists = await uow.Repository<Employee>().ExistsAsync(employeeId);
        if (!empExists) return ApiResponse<EmployeeExperienceDto>.Fail("Employee not found.");

        var exp = new EmployeeExperience
        {
            EmployeeId = employeeId,
            Company = req.Company,
            Designation = req.Designation,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            IsCurrent = req.IsCurrent,
            Description = req.Description,
            CreatedAt = DateTime.UtcNow
        };

        await uow.Repository<EmployeeExperience>().AddAsync(exp);
        await uow.SaveChangesAsync();

        return ApiResponse<EmployeeExperienceDto>.Ok(new EmployeeExperienceDto
        {
            Id = exp.Id,
            EmployeeId = exp.EmployeeId,
            Company = exp.Company,
            Designation = exp.Designation,
            StartDate = exp.StartDate,
            EndDate = exp.EndDate,
            IsCurrent = exp.IsCurrent,
            Description = exp.Description
        });
    }

    public async Task<ApiResponse> DeleteExperienceAsync(int employeeId, int experienceId, int? userId)
    {
        var exp = await uow.Repository<EmployeeExperience>().GetByIdAsync(experienceId);
        if (exp == null || exp.EmployeeId != employeeId) return ApiResponse.Fail("Experience record not found.");

        uow.Repository<EmployeeExperience>().Remove(exp);
        await uow.SaveChangesAsync();

        return ApiResponse.Ok("Experience record removed successfully.");
    }

    // ==========================================
    // SUB-ENTITIES: DOCUMENTS
    // ==========================================
    public async Task<ApiResponse<EmployeeDocumentDto>> UploadDocumentAsync(int employeeId, string documentType, string title, bool isPrivate, DateOnly? expiryDate, IFormFile file, int? userId)
    {
        var empExists = await uow.Repository<Employee>().ExistsAsync(employeeId);
        if (!empExists) return ApiResponse<EmployeeDocumentDto>.Fail("Employee not found.");

        var (url, publicId) = await fileStorage.UploadAsync(file, "employee-documents");

        var doc = new EmployeeDocument
        {
            EmployeeId = employeeId,
            DocumentType = documentType,
            Title = title,
            FileUrl = url,
            FilePublicId = publicId,
            IsPrivate = isPrivate,
            ExpiryDate = expiryDate,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await uow.Repository<EmployeeDocument>().AddAsync(doc);
        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Create, "EmployeeDocuments", doc.Id.ToString(), null, new { doc.Title, doc.DocumentType, doc.FileUrl });

        return ApiResponse<EmployeeDocumentDto>.Ok(new EmployeeDocumentDto
        {
            Id = doc.Id,
            EmployeeId = doc.EmployeeId,
            DocumentType = doc.DocumentType,
            Title = doc.Title,
            FileUrl = doc.FileUrl,
            IsPrivate = doc.IsPrivate,
            ExpiryDate = doc.ExpiryDate,
            CreatedAt = doc.CreatedAt
        });
    }

    public async Task<ApiResponse> DeleteDocumentAsync(int employeeId, int documentId, int? userId)
    {
        var doc = await uow.Repository<EmployeeDocument>().GetByIdAsync(documentId);
        if (doc == null || doc.EmployeeId != employeeId) return ApiResponse.Fail("Document not found.");

        doc.IsDeleted = true;
        doc.UpdatedAt = DateTime.UtcNow;
        doc.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Delete, "EmployeeDocuments", documentId.ToString(), null, null);

        return ApiResponse.Ok("Document deleted successfully.");
    }

    // ==========================================
    // LIFECYCLE EVENTS
    // ==========================================
    public async Task<ApiResponse<EmployeeLifecycleEventDto>> AddLifecycleEventAsync(int employeeId, CreateLifecycleEventRequest req, int? userId)
    {
        var empExists = await uow.Repository<Employee>().ExistsAsync(employeeId);
        if (!empExists) return ApiResponse<EmployeeLifecycleEventDto>.Fail("Employee not found.");

        var evt = new EmployeeLifecycleEvent
        {
            EmployeeId = employeeId,
            EventType = req.EventType,
            EventDate = req.EventDate,
            OldValue = req.OldValue,
            NewValue = req.NewValue,
            Remarks = req.Remarks,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await uow.Repository<EmployeeLifecycleEvent>().AddAsync(evt);
        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Create, "EmployeeLifecycleEvents", evt.Id.ToString(), null, evt);

        return ApiResponse<EmployeeLifecycleEventDto>.Ok(new EmployeeLifecycleEventDto
        {
            Id = evt.Id,
            EmployeeId = evt.EmployeeId,
            EventType = evt.EventType,
            EventDate = evt.EventDate,
            OldValue = evt.OldValue,
            NewValue = evt.NewValue,
            Remarks = evt.Remarks,
            CreatedAt = evt.CreatedAt
        });
    }

    public async Task<ApiResponse<List<EmployeeLifecycleEventDto>>> GetLifecycleEventsAsync(int employeeId)
    {
        var items = await uow.Repository<EmployeeLifecycleEvent>().Query()
            .Where(e => e.EmployeeId == employeeId)
            .OrderByDescending(e => e.EventDate)
            .ThenByDescending(e => e.CreatedAt)
            .Select(e => new EmployeeLifecycleEventDto
            {
                Id = e.Id,
                EmployeeId = e.EmployeeId,
                EventType = e.EventType,
                EventDate = e.EventDate,
                OldValue = e.OldValue,
                NewValue = e.NewValue,
                Remarks = e.Remarks,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<EmployeeLifecycleEventDto>>.Ok(items);
    }

    // ==========================================
    // EXCEL EXPORT (ClosedXML)
    // ==========================================
    public async Task<byte[]> ExportEmployeesToExcelAsync(EmployeeFilterQuery query)
    {
        var employeesRes = await GetEmployeesPagedAsync(new EmployeeFilterQuery
        {
            Page = 1,
            PageSize = 10000,
            Search = query.Search,
            BranchId = query.BranchId,
            DepartmentId = query.DepartmentId,
            DesignationId = query.DesignationId,
            Status = query.Status
        });

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Employees");

        // Headers
        var headers = new[]
        {
            "Employee Code", "Full Name", "Gender", "Work Email", "Phone",
            "Branch", "Department", "Designation", "Status", "Employment Type", "Joining Date"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e1b4b");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Data rows
        int row = 2;
        foreach (var emp in employeesRes.Data!.Items)
        {
            ws.Cell(row, 1).Value = emp.EmployeeCode;
            ws.Cell(row, 2).Value = emp.FullName;
            ws.Cell(row, 3).Value = emp.Gender;
            ws.Cell(row, 4).Value = emp.WorkEmail;
            ws.Cell(row, 5).Value = emp.WorkPhone ?? "-";
            ws.Cell(row, 6).Value = emp.BranchName;
            ws.Cell(row, 7).Value = emp.DepartmentName;
            ws.Cell(row, 8).Value = emp.DesignationTitle;
            ws.Cell(row, 9).Value = emp.Status;
            ws.Cell(row, 10).Value = emp.EmploymentType;
            ws.Cell(row, 11).Value = emp.JoiningDate.ToString("yyyy-MM-dd");

            if (row % 2 == 0)
            {
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8fafc");
            }
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ==========================================
    // PRIVATE HELPERS
    // ==========================================
    private async Task<string> GenerateEmployeeCodeAsync()
    {
        var year = DateTime.UtcNow.Year;
        var nextSeqParam = new SqlParameter
        {
            ParameterName = "@NextSeq",
            SqlDbType = System.Data.SqlDbType.Int,
            Direction = System.Data.ParameterDirection.Output
        };

        await db.Database.ExecuteSqlRawAsync(
            """
            DECLARE @Seq INT;
            UPDATE dbo.EmployeeCodeSequences WITH (UPDLOCK, ROWLOCK)
            SET @Seq = LastSeq = LastSeq + 1
            WHERE Year = @Year;

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO dbo.EmployeeCodeSequences (Year, LastSeq)
                VALUES (@Year, 1);
                SET @Seq = 1;
            END

            SET @NextSeq = @Seq;
            """,
            new SqlParameter("@Year", year),
            nextSeqParam);

        var seq = (int)nextSeqParam.Value;
        return $"EMP-{year}-{seq:D4}";
    }

    private static EmployeeDetailDto MapToDetailDto(Employee e)
    {
        return new EmployeeDetailDto
        {
            Id = e.Id,
            EmployeeCode = e.EmployeeCode,
            FullName = e.FullName,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Gender = e.Gender,
            DateOfBirth = e.DateOfBirth,
            NationalId = e.NationalId,
            BloodGroup = e.BloodGroup,
            PersonalEmail = e.PersonalEmail,
            WorkEmail = e.WorkEmail,
            PersonalPhone = e.PersonalPhone,
            WorkPhone = e.WorkPhone,
            MaritalStatus = e.MaritalStatus,
            Nationality = e.Nationality,
            Religion = e.Religion,
            BranchId = e.BranchId,
            BranchName = e.Branch.Name,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department.Name,
            DesignationId = e.DesignationId,
            DesignationTitle = e.Designation.Title,
            GradeId = e.GradeId,
            GradeName = e.Grade?.Name,
            ShiftId = e.ShiftId,
            ShiftName = e.Shift?.Name,
            ReportingManagerId = e.ReportingManagerId,
            ReportingManagerName = e.ReportingManager?.FullName,
            JoiningDate = e.JoiningDate,
            ProbationEndDate = e.ProbationEndDate,
            ConfirmationDate = e.ConfirmationDate,
            EmploymentType = e.EmploymentType,
            Status = e.Status,
            ResignationDate = e.ResignationDate,
            LastWorkingDate = e.LastWorkingDate,
            TerminationReason = e.TerminationReason,
            ProfilePhotoUrl = e.ProfilePhotoUrl,
            CreatedAt = e.CreatedAt,
            Addresses = e.EmployeeAddresses.Select(a => new EmployeeAddressDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                AddressType = a.AddressType,
                Street = a.Street,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country
            }).ToList(),
            EmergencyContacts = e.EmployeeEmergencyContacts.Select(c => new EmployeeEmergencyContactDto
            {
                Id = c.Id,
                EmployeeId = c.EmployeeId,
                Name = c.Name,
                Relationship = c.Relationship,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                IsPrimary = c.IsPrimary
            }).ToList(),
            BankAccounts = e.EmployeeBankAccounts.Select(b => new EmployeeBankAccountDto
            {
                Id = b.Id,
                EmployeeId = b.EmployeeId,
                BankName = b.BankName,
                AccountNumber = b.AccountNumber,
                AccountTitle = b.AccountTitle,
                BranchName = b.BranchName,
                RoutingNumber = b.RoutingNumber,
                SwiftCode = b.SwiftCode,
                IsPrimary = b.IsPrimary
            }).ToList(),
            Educations = e.EmployeeEducations.Select(ed => new EmployeeEducationDto
            {
                Id = ed.Id,
                EmployeeId = ed.EmployeeId,
                Degree = ed.Degree,
                Institution = ed.Institution,
                FieldOfStudy = ed.FieldOfStudy,
                StartYear = ed.StartYear,
                EndYear = ed.EndYear,
                Grade = ed.Grade,
                IsHighest = ed.IsHighest
            }).ToList(),
            Experiences = e.EmployeeExperiences.Select(ex => new EmployeeExperienceDto
            {
                Id = ex.Id,
                EmployeeId = ex.EmployeeId,
                Company = ex.Company,
                Designation = ex.Designation,
                StartDate = ex.StartDate,
                EndDate = ex.EndDate,
                IsCurrent = ex.IsCurrent,
                Description = ex.Description
            }).ToList(),
            Documents = e.EmployeeDocuments.Select(d => new EmployeeDocumentDto
            {
                Id = d.Id,
                EmployeeId = d.EmployeeId,
                DocumentType = d.DocumentType,
                Title = d.Title,
                FileUrl = d.FileUrl,
                IsPrivate = d.IsPrivate,
                ExpiryDate = d.ExpiryDate,
                CreatedAt = d.CreatedAt
            }).ToList(),
            LifecycleEvents = e.EmployeeLifecycleEvents.Select(le => new EmployeeLifecycleEventDto
            {
                Id = le.Id,
                EmployeeId = le.EmployeeId,
                EventType = le.EventType,
                EventDate = le.EventDate,
                OldValue = le.OldValue,
                NewValue = le.NewValue,
                Remarks = le.Remarks,
                CreatedAt = le.CreatedAt
            }).ToList()
        };
    }

    // ==========================================
    // PORTAL LOGIN ACCOUNT MANAGEMENT
    // ==========================================
    public async Task<ApiResponse<EmployeePortalAccountDto>> GetPortalAccountAsync(int employeeId)
    {
        var user = await db.Users
            .AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.EmployeeId == employeeId && !u.IsDeleted);

        if (user == null)
            return ApiResponse<EmployeePortalAccountDto>.Fail("No portal login account associated with this employee.");

        var dto = new EmployeePortalAccountDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            IsActive = user.IsActive,
            MustChangePassword = user.MustChangePassword,
            LastLoginAt = user.LastLoginAt,
            Roles = user.UserRoles.Where(r => !r.Role.IsDeleted).Select(r => r.Role.Name).ToList()
        };

        return ApiResponse<EmployeePortalAccountDto>.Ok(dto);
    }

    public async Task<ApiResponse<EmployeePortalAccountDto>> ManagePortalAccountAsync(int employeeId, ManageEmployeePortalAccountRequest request, int? currentUserId)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);
        if (employee == null) return ApiResponse<EmployeePortalAccountDto>.Fail("Employee not found.");

        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.EmployeeId == employeeId && !u.IsDeleted);

        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();

        if (user == null)
        {
            var username = !string.IsNullOrWhiteSpace(request.Username)
                ? request.Username.Trim().ToLower()
                : employee.WorkEmail.Split('@')[0].ToLower();

            var email = !string.IsNullOrWhiteSpace(request.Email)
                ? request.Email.Trim().ToLower()
                : employee.WorkEmail.Trim().ToLower();

            if (await db.Users.AnyAsync(u => u.Username.ToLower() == username || u.Email.ToLower() == email))
            {
                return ApiResponse<EmployeePortalAccountDto>.Fail("Username or email already assigned to another user account.");
            }

            user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = string.Empty,
                EmployeeId = employeeId,
                IsActive = request.IsActive,
                MustChangePassword = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            var pwd = !string.IsNullOrWhiteSpace(request.Password) ? request.Password : "Password@123";
            user.PasswordHash = hasher.HashPassword(user, pwd);

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var role = await db.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == request.Role.ToLower() && !r.IsDeleted);
            if (role != null)
            {
                db.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = currentUserId
                });
                await db.SaveChangesAsync();
            }

            await auditService.LogAsync(AuditActions.Create, "Users", user.Id.ToString(), null, new { Action = "CreateEmployeePortalAccount", employeeId });
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user.Email = request.Email.Trim().ToLower();
                employee.WorkEmail = user.Email;
            }
            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                user.Username = request.Username.Trim().ToLower();
            }
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = hasher.HashPassword(user, request.Password);
            }

            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = currentUserId;

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                var role = await db.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == request.Role.ToLower() && !r.IsDeleted);
                if (role != null)
                {
                    db.UserRoles.RemoveRange(user.UserRoles);
                    db.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                        AssignedAt = DateTime.UtcNow,
                        AssignedBy = currentUserId
                    });
                }
            }

            await db.SaveChangesAsync();
            await auditService.LogAsync(AuditActions.Update, "Users", user.Id.ToString(), null, new { Action = "UpdateEmployeePortalAccount", employeeId });
        }

        return await GetPortalAccountAsync(employeeId);
    }

    public async Task<ApiResponse> ResetEmployeePasswordAsync(int employeeId, ResetEmployeePasswordRequest request, int? currentUserId)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId && !u.IsDeleted);
        if (user == null) return ApiResponse.Fail("Employee does not have a portal login account.");

        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
        user.MustChangePassword = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUserId;

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Users", user.Id.ToString(), null, new { Action = "ResetEmployeePassword", employeeId });

        return ApiResponse.Ok("Employee portal password reset successfully.");
    }
}
