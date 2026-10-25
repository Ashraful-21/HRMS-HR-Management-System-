using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.Data;
using HRMS.Dtos.Common;
using HRMS.DTOs;
using HRMS.IServices;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;

namespace HRMS.Services;

public class PayrollService(
    HrmsDbContext db,
    IAuditService auditService,
    ILogger<PayrollService> logger) : IPayrollService
{
    // ==========================================
    // 1. SALARY COMPONENTS
    // ==========================================
    public async Task<ApiResponse<List<SalaryComponentDto>>> GetSalaryComponentsAsync()
    {
        var list = await db.SalaryComponents
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new SalaryComponentDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Type = c.Type,
                IsFixed = c.IsFixed,
                IsTaxable = c.IsTaxable,
                IsActive = c.IsActive,
                SortOrder = c.SortOrder
            })
            .ToListAsync();

        return ApiResponse<List<SalaryComponentDto>>.Ok(list);
    }

    public async Task<ApiResponse<SalaryComponentDto>> CreateSalaryComponentAsync(CreateSalaryComponentRequest req, int currentUserId)
    {
        var code = req.Code.Trim().ToUpper();
        if (await db.SalaryComponents.AnyAsync(c => c.Code == code))
        {
            return ApiResponse<SalaryComponentDto>.Fail($"Component with code '{code}' already exists.");
        }

        var comp = new SalaryComponent
        {
            Name = req.Name.Trim(),
            Code = code,
            Type = req.Type,
            IsFixed = req.IsFixed,
            IsTaxable = req.IsTaxable,
            IsActive = req.IsActive,
            SortOrder = req.SortOrder,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        db.SalaryComponents.Add(comp);
        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "CREATE_SALARY_COMPONENT",
            "SalaryComponent",
            comp.Id.ToString(),
            null,
            new { comp.Id, comp.Name, comp.Code, comp.Type });

        return ApiResponse<SalaryComponentDto>.Ok(new SalaryComponentDto
        {
            Id = comp.Id,
            Name = comp.Name,
            Code = comp.Code,
            Type = comp.Type,
            IsFixed = comp.IsFixed,
            IsTaxable = comp.IsTaxable,
            IsActive = comp.IsActive,
            SortOrder = comp.SortOrder
        }, "Salary component created successfully.");
    }

    // ==========================================
    // 2. SALARY STRUCTURE
    // ==========================================
    public async Task<ApiResponse<SalaryStructureDto>> GetEmployeeSalaryStructureAsync(int employeeId)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee == null)
            return ApiResponse<SalaryStructureDto>.Fail("Employee not found.");

        var structure = await db.SalaryStructures
            .Include(s => s.Employee)
            .Include(s => s.SalaryStructureComponents)
                .ThenInclude(sc => sc.Component)
            .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.IsCurrent);

        if (structure == null)
        {
            // Auto-create standard industry default salary structure (Gross: 80,000 BDT)
            var components = await db.SalaryComponents.Where(c => c.IsActive).ToListAsync();
            var basic = components.FirstOrDefault(c => c.Code == "BASIC");
            var hra = components.FirstOrDefault(c => c.Code == "HRA");
            var med = components.FirstOrDefault(c => c.Code == "MED");
            var conv = components.FirstOrDefault(c => c.Code == "CONV");
            var pf = components.FirstOrDefault(c => c.Code == "PF");

            structure = new SalaryStructure
            {
                EmployeeId = employeeId,
                GrossSalary = 80000m,
                EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow),
                IsCurrent = true,
                Remarks = "Default Initial Structure",
                CreatedAt = DateTime.UtcNow
            };

            if (basic != null)
                structure.SalaryStructureComponents.Add(new SalaryStructureComponent { ComponentId = basic.Id, CalculationType = "Percentage", Amount = 50, PercentageBase = "Gross" });
            if (hra != null)
                structure.SalaryStructureComponents.Add(new SalaryStructureComponent { ComponentId = hra.Id, CalculationType = "Percentage", Amount = 50, PercentageBase = "Basic" });
            if (med != null)
                structure.SalaryStructureComponents.Add(new SalaryStructureComponent { ComponentId = med.Id, CalculationType = "Percentage", Amount = 20, PercentageBase = "Basic" });
            if (conv != null)
                structure.SalaryStructureComponents.Add(new SalaryStructureComponent { ComponentId = conv.Id, CalculationType = "Percentage", Amount = 10, PercentageBase = "Basic" });
            if (pf != null)
                structure.SalaryStructureComponents.Add(new SalaryStructureComponent { ComponentId = pf.Id, CalculationType = "Percentage", Amount = 10, PercentageBase = "Basic" });

            db.SalaryStructures.Add(structure);
            await db.SaveChangesAsync();

            structure = await db.SalaryStructures
                .Include(s => s.Employee)
                .Include(s => s.SalaryStructureComponents)
                    .ThenInclude(sc => sc.Component)
                .FirstOrDefaultAsync(s => s.Id == structure.Id);
        }

        var dto = MapStructureToDto(structure!);
        return ApiResponse<SalaryStructureDto>.Ok(dto);
    }

    public async Task<ApiResponse<SalaryStructureDto>> AssignSalaryStructureAsync(AssignSalaryStructureRequest req, int currentUserId)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == req.EmployeeId);
        if (employee == null)
            return ApiResponse<SalaryStructureDto>.Fail("Employee not found.");

        // Deactivate previous active structure
        var oldStructures = await db.SalaryStructures
            .Where(s => s.EmployeeId == req.EmployeeId && s.IsCurrent)
            .ToListAsync();

        foreach (var old in oldStructures)
        {
            old.IsCurrent = false;
            old.EffectiveTo = req.EffectiveFrom.AddDays(-1);
            old.UpdatedAt = DateTime.UtcNow;
            old.UpdatedBy = currentUserId;
        }

        var newStructure = new SalaryStructure
        {
            EmployeeId = req.EmployeeId,
            GrossSalary = req.GrossSalary,
            EffectiveFrom = req.EffectiveFrom,
            IsCurrent = true,
            Remarks = req.Remarks,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        foreach (var c in req.Components)
        {
            newStructure.SalaryStructureComponents.Add(new SalaryStructureComponent
            {
                ComponentId = c.ComponentId,
                CalculationType = c.CalculationType,
                Amount = c.Amount,
                PercentageBase = c.PercentageBase
            });
        }

        db.SalaryStructures.Add(newStructure);
        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "ASSIGN_SALARY_STRUCTURE",
            "SalaryStructure",
            newStructure.Id.ToString(),
            null,
            new { newStructure.Id, req.EmployeeId, req.GrossSalary, req.EffectiveFrom });

        var reloaded = await db.SalaryStructures
            .Include(s => s.Employee)
            .Include(s => s.SalaryStructureComponents)
                .ThenInclude(sc => sc.Component)
            .FirstAsync(s => s.Id == newStructure.Id);

        return ApiResponse<SalaryStructureDto>.Ok(MapStructureToDto(reloaded), "Salary structure assigned successfully.");
    }

    // ==========================================
    // 3. TAX BRACKET ENGINE
    // ==========================================
    public ApiResponse<TaxCalculationResultDto> CalculateTax(decimal monthlyGross)
    {
        var annualGross = monthlyGross * 12;
        var taxExempt = 350000m;
        var taxable = Math.Max(0, annualGross - taxExempt);

        decimal annualTax = 0;
        decimal remaining = taxable;

        // Progressive brackets:
        // Tier 1: Next 100,000 @ 5%
        var tier1 = Math.Min(remaining, 100000m);
        annualTax += tier1 * 0.05m;
        remaining = Math.Max(0, remaining - tier1);

        // Tier 2: Next 300,000 @ 10%
        if (remaining > 0)
        {
            var tier2 = Math.Min(remaining, 300000m);
            annualTax += tier2 * 0.10m;
            remaining = Math.Max(0, remaining - tier2);
        }

        // Tier 3: Next 400,000 @ 15%
        if (remaining > 0)
        {
            var tier3 = Math.Min(remaining, 400000m);
            annualTax += tier3 * 0.15m;
            remaining = Math.Max(0, remaining - tier3);
        }

        // Tier 4: Next 500,000 @ 20%
        if (remaining > 0)
        {
            var tier4 = Math.Min(remaining, 500000m);
            annualTax += tier4 * 0.20m;
            remaining = Math.Max(0, remaining - tier4);
        }

        // Tier 5: Remaining @ 25%
        if (remaining > 0)
        {
            annualTax += remaining * 0.25m;
        }

        var monthlyTds = Math.Round(annualTax / 12, 2);
        var effectiveRate = annualGross > 0 ? Math.Round((annualTax / annualGross) * 100, 2) : 0;

        return ApiResponse<TaxCalculationResultDto>.Ok(new TaxCalculationResultDto
        {
            MonthlyGross = monthlyGross,
            AnnualGross = annualGross,
            TaxExemptAmount = taxExempt,
            TaxableIncome = taxable,
            EstimatedAnnualTax = Math.Round(annualTax, 2),
            MonthlyTds = monthlyTds,
            EffectiveTaxRatePercentage = effectiveRate
        });
    }

    // ==========================================
    // 4. PAYROLL BATCH PROCESSING
    // ==========================================
    public async Task<ApiResponse<PayrollRunSummaryDto>> ProcessPayrollAsync(ProcessPayrollRequest req, int currentUserId)
    {
        logger.LogInformation("Processing payroll batch for {Month}/{Year}", req.Month, req.Year);

        var existingRun = await db.PayrollRuns
            .Include(r => r.PayrollDetails)
            .FirstOrDefaultAsync(r => r.Month == req.Month && r.Year == req.Year && r.BranchId == req.BranchId);

        if (existingRun != null && existingRun.Status == "Paid")
        {
            return ApiResponse<PayrollRunSummaryDto>.Fail("Payroll for this period has already been paid and locked.");
        }

        var run = existingRun;
        if (run == null)
        {
            run = new PayrollRun
            {
                Month = req.Month,
                Year = req.Year,
                BranchId = req.BranchId,
                WorkingDays = req.WorkingDays,
                Status = "Draft",
                ProcessedAt = DateTime.UtcNow,
                ProcessedBy = currentUserId,
                Remarks = req.Remarks,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };
            db.PayrollRuns.Add(run);
            await db.SaveChangesAsync();
        }
        else
        {
            // Clear prior draft details for recalculation
            db.PayrollDetails.RemoveRange(run.PayrollDetails);
            run.WorkingDays = req.WorkingDays;
            run.Status = "Draft";
            run.ProcessedAt = DateTime.UtcNow;
            run.ProcessedBy = currentUserId;
            run.UpdatedAt = DateTime.UtcNow;
            run.UpdatedBy = currentUserId;
            await db.SaveChangesAsync();
        }

        var employees = await db.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Branch)
            .Include(e => e.EmployeeBankAccounts)
            .Where(e => e.Status == "Active" && (!req.BranchId.HasValue || e.BranchId == req.BranchId.Value))
            .ToListAsync();

        var startDate = new DateOnly(req.Year, req.Month, 1);
        var endDate = new DateOnly(req.Year, req.Month, DateTime.DaysInMonth(req.Year, req.Month));

        var componentsList = await db.SalaryComponents.Where(c => c.IsActive).ToListAsync();

        foreach (var emp in employees)
        {
            // Ensure employee has active salary structure
            var structure = await db.SalaryStructures
                .Include(s => s.SalaryStructureComponents)
                    .ThenInclude(sc => sc.Component)
                .FirstOrDefaultAsync(s => s.EmployeeId == emp.Id && s.IsCurrent);

            if (structure == null)
            {
                await GetEmployeeSalaryStructureAsync(emp.Id); // creates default
                structure = await db.SalaryStructures
                    .Include(s => s.SalaryStructureComponents)
                        .ThenInclude(sc => sc.Component)
                    .FirstAsync(s => s.EmployeeId == emp.Id && s.IsCurrent);
            }

            // Attendance analysis
            var logs = await db.AttendanceLogs
                .Where(a => a.EmployeeId == emp.Id && a.LogDate >= startDate && a.LogDate <= endDate)
                .ToListAsync();

            var presentDays = logs.Count(a => a.Status == "Present" || a.Status == "Late" || a.Status == "Weekend" || a.Status == "Holiday");
            var paidLeaveDays = logs.Count(a => a.Status == "OnLeave");
            var unpaidLeaveDays = logs.Count(a => a.Status == "Absent");
            var overtimeMinutes = logs.Sum(a => a.OvertimeMinutes);

            // Pro-rating: Loss of Pay
            decimal lossOfPay = 0;
            if (unpaidLeaveDays > 0 && req.WorkingDays > 0)
            {
                var perDay = structure.GrossSalary / req.WorkingDays;
                lossOfPay = Math.Round(perDay * unpaidLeaveDays, 2);
            }

            // Calculate component breakdown
            decimal basicAmount = structure.GrossSalary * 0.50m; // Default basic 50%
            var basicComp = structure.SalaryStructureComponents.FirstOrDefault(c => c.Component.Code == "BASIC");
            if (basicComp != null)
            {
                basicAmount = basicComp.CalculationType == "Fixed"
                    ? basicComp.Amount
                    : (structure.GrossSalary * (basicComp.Amount / 100m));
            }

            var detail = new PayrollDetail
            {
                PayrollRunId = run.Id,
                EmployeeId = emp.Id,
                SalaryStructureId = structure.Id,
                WorkingDays = req.WorkingDays,
                PresentDays = presentDays,
                PaidLeaveDays = paidLeaveDays,
                UnpaidLeaveDays = unpaidLeaveDays,
                OvertimeMinutes = overtimeMinutes,
                LossOfPay = lossOfPay
            };

            decimal grossEarnings = 0;
            decimal totalDeductions = 0;

            // Compute each assigned component
            foreach (var sc in structure.SalaryStructureComponents)
            {
                decimal amount = 0;
                if (sc.CalculationType == "Fixed")
                {
                    amount = sc.Amount;
                }
                else // Percentage
                {
                    var baseVal = (sc.PercentageBase == "Gross") ? structure.GrossSalary : basicAmount;
                    amount = Math.Round(baseVal * (sc.Amount / 100m), 2);
                }

                if (sc.Component.Type == "Earning")
                {
                    grossEarnings += amount;
                }
                else if (sc.Component.Type == "Deduction")
                {
                    totalDeductions += amount;
                }

                detail.PayrollComponentDetails.Add(new PayrollComponentDetail
                {
                    ComponentId = sc.ComponentId,
                    Amount = amount
                });
            }

            // Overtime Pay calculation
            if (overtimeMinutes > 0 && req.WorkingDays > 0)
            {
                var hourlyRate = basicAmount / (req.WorkingDays * 8m);
                var overtimePay = Math.Round((overtimeMinutes / 60m) * (hourlyRate * 1.5m), 2);
                grossEarnings += overtimePay;

                var bonusComp = componentsList.FirstOrDefault(c => c.Code == "BONUS");
                if (bonusComp != null)
                {
                    detail.PayrollComponentDetails.Add(new PayrollComponentDetail
                    {
                        ComponentId = bonusComp.Id,
                        Amount = overtimePay
                    });
                }
            }

            // Tax (TDS) calculation
            var taxResult = CalculateTax(structure.GrossSalary);
            if (taxResult.Data != null && taxResult.Data.MonthlyTds > 0)
            {
                var taxComp = componentsList.FirstOrDefault(c => c.Code == "TAX");
                if (taxComp != null && !detail.PayrollComponentDetails.Any(d => d.ComponentId == taxComp.Id))
                {
                    totalDeductions += taxResult.Data.MonthlyTds;
                    detail.PayrollComponentDetails.Add(new PayrollComponentDetail
                    {
                        ComponentId = taxComp.Id,
                        Amount = taxResult.Data.MonthlyTds
                    });
                }
            }

            // Adjust for loss of pay
            if (lossOfPay > 0)
            {
                grossEarnings = Math.Max(0, grossEarnings - lossOfPay);
            }

            detail.GrossEarnings = Math.Round(grossEarnings, 2);
            detail.TotalDeductions = Math.Round(totalDeductions, 2);
            detail.NetPay = Math.Round(Math.Max(0, grossEarnings - totalDeductions), 2);

            db.PayrollDetails.Add(detail);
        }

        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "PROCESS_PAYROLL",
            "PayrollRun",
            run.Id.ToString(),
            null,
            new { run.Id, req.Month, req.Year, EmployeeCount = employees.Count });

        return await GetPayrollRunByIdAsync(run.Id);
    }

    public async Task<ApiResponse<List<PayrollRunSummaryDto>>> GetPayrollRunsAsync(int? year, int? month)
    {
        var q = db.PayrollRuns
            .Include(r => r.Branch)
            .Include(r => r.PayrollDetails)
            .AsQueryable();

        if (year.HasValue) q = q.Where(r => r.Year == year.Value);
        if (month.HasValue) q = q.Where(r => r.Month == month.Value);

        var list = await q.OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Month)
            .Select(r => new PayrollRunSummaryDto
            {
                Id = r.Id,
                Month = r.Month,
                Year = r.Year,
                BranchId = r.BranchId,
                BranchName = r.Branch != null ? r.Branch.Name : "All Branches",
                Status = r.Status,
                WorkingDays = r.WorkingDays,
                TotalEmployees = r.PayrollDetails.Count,
                TotalGross = r.PayrollDetails.Sum(d => d.GrossEarnings),
                TotalDeductions = r.PayrollDetails.Sum(d => d.TotalDeductions),
                TotalNetPay = r.PayrollDetails.Sum(d => d.NetPay),
                ProcessedAt = r.ProcessedAt,
                ApprovedAt = r.ApprovedAt,
                PaidAt = r.PaidAt,
                Remarks = r.Remarks
            })
            .ToListAsync();

        return ApiResponse<List<PayrollRunSummaryDto>>.Ok(list);
    }

    public async Task<ApiResponse<PayrollRunSummaryDto>> GetPayrollRunByIdAsync(int id)
    {
        var r = await db.PayrollRuns
            .Include(r => r.Branch)
            .Include(r => r.PayrollDetails)
                .ThenInclude(d => d.Employee)
                    .ThenInclude(e => e.Department)
            .Include(r => r.PayrollDetails)
                .ThenInclude(d => d.Employee)
                    .ThenInclude(e => e.Designation)
            .Include(r => r.PayrollDetails)
                .ThenInclude(d => d.Employee)
                    .ThenInclude(e => e.EmployeeBankAccounts)
            .Include(r => r.PayrollDetails)
                .ThenInclude(d => d.PayrollComponentDetails)
                    .ThenInclude(cd => cd.Component)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (r == null)
            return ApiResponse<PayrollRunSummaryDto>.Fail("Payroll run not found.");

        var details = r.PayrollDetails.Select(d => new PayrollDetailDto
        {
            Id = d.Id,
            PayrollRunId = d.PayrollRunId,
            EmployeeId = d.EmployeeId,
            EmployeeName = d.Employee.FullName,
            EmployeeCode = d.Employee.EmployeeCode,
            DepartmentName = d.Employee.Department?.Name,
            DesignationTitle = d.Employee.Designation?.Title,
            BankName = d.Employee.EmployeeBankAccounts.FirstOrDefault(b => b.IsPrimary)?.BankName,
            AccountNumber = d.Employee.EmployeeBankAccounts.FirstOrDefault(b => b.IsPrimary)?.AccountNumber,
            WorkingDays = d.WorkingDays,
            PresentDays = d.PresentDays,
            PaidLeaveDays = d.PaidLeaveDays,
            UnpaidLeaveDays = d.UnpaidLeaveDays,
            OvertimeMinutes = d.OvertimeMinutes,
            GrossEarnings = d.GrossEarnings,
            TotalDeductions = d.TotalDeductions,
            NetPay = d.NetPay,
            LossOfPay = d.LossOfPay,
            Components = d.PayrollComponentDetails.Select(cd => new PayrollComponentDetailDto
            {
                ComponentId = cd.ComponentId,
                ComponentName = cd.Component.Name,
                ComponentCode = cd.Component.Code,
                ComponentType = cd.Component.Type,
                Amount = cd.Amount
            }).ToList()
        }).ToList();

        var summary = new PayrollRunSummaryDto
        {
            Id = r.Id,
            Month = r.Month,
            Year = r.Year,
            BranchId = r.BranchId,
            BranchName = r.Branch != null ? r.Branch.Name : "All Branches",
            Status = r.Status,
            WorkingDays = r.WorkingDays,
            TotalEmployees = details.Count,
            TotalGross = details.Sum(d => d.GrossEarnings),
            TotalDeductions = details.Sum(d => d.TotalDeductions),
            TotalNetPay = details.Sum(d => d.NetPay),
            ProcessedAt = r.ProcessedAt,
            ApprovedAt = r.ApprovedAt,
            PaidAt = r.PaidAt,
            Remarks = r.Remarks,
            Details = details
        };

        return ApiResponse<PayrollRunSummaryDto>.Ok(summary);
    }

    public async Task<ApiResponse<PayrollRunSummaryDto>> ApprovePayrollRunAsync(int id, int currentUserId)
    {
        var run = await db.PayrollRuns.FirstOrDefaultAsync(r => r.Id == id);
        if (run == null)
            return ApiResponse<PayrollRunSummaryDto>.Fail("Payroll run not found.");

        if (run.Status == "Paid")
            return ApiResponse<PayrollRunSummaryDto>.Fail("Payroll run is already paid and locked.");

        run.Status = "Approved";
        run.ApprovedAt = DateTime.UtcNow;
        run.ApprovedBy = currentUserId;
        run.UpdatedAt = DateTime.UtcNow;
        run.UpdatedBy = currentUserId;

        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "APPROVE_PAYROLL",
            "PayrollRun",
            run.Id.ToString(),
            null,
            new { run.Id, run.Month, run.Year, currentUserId });

        return await GetPayrollRunByIdAsync(id);
    }

    public async Task<ApiResponse<PayrollRunSummaryDto>> MarkPayrollRunAsPaidAsync(int id, int currentUserId)
    {
        var run = await db.PayrollRuns.FirstOrDefaultAsync(r => r.Id == id);
        if (run == null)
            return ApiResponse<PayrollRunSummaryDto>.Fail("Payroll run not found.");

        run.Status = "Paid";
        run.PaidAt = DateTime.UtcNow;
        run.PaidBy = currentUserId;
        run.LockedAt = DateTime.UtcNow;
        run.LockedBy = currentUserId;
        run.UpdatedAt = DateTime.UtcNow;
        run.UpdatedBy = currentUserId;

        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "PAY_PAYROLL",
            "PayrollRun",
            run.Id.ToString(),
            null,
            new { run.Id, run.Month, run.Year, currentUserId });

        return await GetPayrollRunByIdAsync(id);
    }

    // ==========================================
    // 5. PAYSLIP GENERATION (QuestPDF)
    // ==========================================
    public async Task<byte[]> GeneratePayslipPdfAsync(int payrollDetailId)
    {
        var detail = await db.PayrollDetails
            .Include(d => d.PayrollRun)
            .Include(d => d.Employee)
                .ThenInclude(e => e.Department)
            .Include(d => d.Employee)
                .ThenInclude(e => e.Designation)
            .Include(d => d.Employee)
                .ThenInclude(e => e.EmployeeBankAccounts)
            .Include(d => d.PayrollComponentDetails)
                .ThenInclude(cd => cd.Component)
            .FirstOrDefaultAsync(d => d.Id == payrollDetailId);

        if (detail == null)
            throw new KeyNotFoundException($"Payroll detail record with ID {payrollDetailId} not found.");

        var detailDto = new PayrollDetailDto
        {
            Id = detail.Id,
            PayrollRunId = detail.PayrollRunId,
            EmployeeId = detail.EmployeeId,
            EmployeeName = detail.Employee.FullName,
            EmployeeCode = detail.Employee.EmployeeCode,
            DepartmentName = detail.Employee.Department?.Name,
            DesignationTitle = detail.Employee.Designation?.Title,
            BankName = detail.Employee.EmployeeBankAccounts.FirstOrDefault(b => b.IsPrimary)?.BankName,
            AccountNumber = detail.Employee.EmployeeBankAccounts.FirstOrDefault(b => b.IsPrimary)?.AccountNumber,
            WorkingDays = detail.WorkingDays,
            PresentDays = detail.PresentDays,
            PaidLeaveDays = detail.PaidLeaveDays,
            UnpaidLeaveDays = detail.UnpaidLeaveDays,
            OvertimeMinutes = detail.OvertimeMinutes,
            GrossEarnings = detail.GrossEarnings,
            TotalDeductions = detail.TotalDeductions,
            NetPay = detail.NetPay,
            LossOfPay = detail.LossOfPay,
            Components = detail.PayrollComponentDetails.Select(cd => new PayrollComponentDetailDto
            {
                ComponentId = cd.ComponentId,
                ComponentName = cd.Component.Name,
                ComponentCode = cd.Component.Code,
                ComponentType = cd.Component.Type,
                Amount = cd.Amount
            }).ToList()
        };

        var runDto = new PayrollRunSummaryDto
        {
            Id = detail.PayrollRun.Id,
            Month = detail.PayrollRun.Month,
            Year = detail.PayrollRun.Year,
            Status = detail.PayrollRun.Status,
            WorkingDays = detail.PayrollRun.WorkingDays
        };

        var doc = new PayslipDocument(detailDto, runDto);
        return doc.GeneratePdf();
    }

    public async Task<ApiResponse<List<PayrollDetailDto>>> GetEmployeePayslipsAsync(int employeeId, int? year)
    {
        var q = db.PayrollDetails
            .Include(d => d.PayrollRun)
            .Include(d => d.Employee)
                .ThenInclude(e => e.Department)
            .Include(d => d.Employee)
                .ThenInclude(e => e.Designation)
            .Include(d => d.Employee)
                .ThenInclude(e => e.EmployeeBankAccounts)
            .Include(d => d.PayrollComponentDetails)
                .ThenInclude(cd => cd.Component)
            .Where(d => d.EmployeeId == employeeId);

        if (year.HasValue)
            q = q.Where(d => d.PayrollRun.Year == year.Value);

        var list = await q.OrderByDescending(d => d.PayrollRun.Year)
            .ThenByDescending(d => d.PayrollRun.Month)
            .Select(d => new PayrollDetailDto
            {
                Id = d.Id,
                PayrollRunId = d.PayrollRunId,
                EmployeeId = d.EmployeeId,
                EmployeeName = d.Employee.FullName,
                EmployeeCode = d.Employee.EmployeeCode,
                DepartmentName = d.Employee.Department != null ? d.Employee.Department.Name : null,
                DesignationTitle = d.Employee.Designation != null ? d.Employee.Designation.Title : null,
                BankName = d.Employee.EmployeeBankAccounts.FirstOrDefault(b => b.IsPrimary) != null ? d.Employee.EmployeeBankAccounts.FirstOrDefault(b => b.IsPrimary)!.BankName : null,
                AccountNumber = d.Employee.EmployeeBankAccounts.FirstOrDefault(b => b.IsPrimary) != null ? d.Employee.EmployeeBankAccounts.FirstOrDefault(b => b.IsPrimary)!.AccountNumber : null,
                WorkingDays = d.WorkingDays,
                PresentDays = d.PresentDays,
                PaidLeaveDays = d.PaidLeaveDays,
                UnpaidLeaveDays = d.UnpaidLeaveDays,
                OvertimeMinutes = d.OvertimeMinutes,
                GrossEarnings = d.GrossEarnings,
                TotalDeductions = d.TotalDeductions,
                NetPay = d.NetPay,
                LossOfPay = d.LossOfPay,
                Components = d.PayrollComponentDetails.Select(cd => new PayrollComponentDetailDto
                {
                    ComponentId = cd.ComponentId,
                    ComponentName = cd.Component.Name,
                    ComponentCode = cd.Component.Code,
                    ComponentType = cd.Component.Type,
                    Amount = cd.Amount
                }).ToList()
            })
            .ToListAsync();

        return ApiResponse<List<PayrollDetailDto>>.Ok(list);
    }

    private static SalaryStructureDto MapStructureToDto(SalaryStructure s)
    {
        return new SalaryStructureDto
        {
            Id = s.Id,
            EmployeeId = s.EmployeeId,
            EmployeeName = s.Employee?.FullName ?? "",
            EmployeeCode = s.Employee?.EmployeeCode ?? "",
            GrossSalary = s.GrossSalary,
            EffectiveFrom = s.EffectiveFrom,
            EffectiveTo = s.EffectiveTo,
            IsCurrent = s.IsCurrent,
            Remarks = s.Remarks,
            Components = s.SalaryStructureComponents.Select(sc =>
            {
                decimal calculated = 0;
                if (sc.CalculationType == "Fixed")
                {
                    calculated = sc.Amount;
                }
                else
                {
                    var basicAmount = s.GrossSalary * 0.50m;
                    var baseVal = (sc.PercentageBase == "Gross") ? s.GrossSalary : basicAmount;
                    calculated = Math.Round(baseVal * (sc.Amount / 100m), 2);
                }

                return new SalaryStructureComponentDto
                {
                    Id = sc.Id,
                    ComponentId = sc.ComponentId,
                    ComponentName = sc.Component?.Name ?? "",
                    ComponentCode = sc.Component?.Code ?? "",
                    ComponentType = sc.Component?.Type ?? "",
                    CalculationType = sc.CalculationType,
                    Amount = sc.Amount,
                    PercentageBase = sc.PercentageBase,
                    CalculatedMonthlyAmount = calculated
                };
            }).ToList()
        };
    }
}
