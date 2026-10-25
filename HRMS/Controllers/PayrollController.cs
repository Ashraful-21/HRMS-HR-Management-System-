using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Common;
using HRMS.DTOs;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/payroll")]
[Authorize]
[Produces("application/json")]
public class PayrollController(
    IPayrollService payrollService,
    ICurrentUserService currentUser) : ControllerBase
{
    // ==========================================
    // 1. SALARY COMPONENTS
    // ==========================================
    [HttpGet("components")]
    [ProducesResponseType(typeof(ApiResponse<List<SalaryComponentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComponents()
    {
        var result = await payrollService.GetSalaryComponentsAsync();
        return Ok(result);
    }

    [HttpPost("components")]
    [HasPermission(Permissions.PayrollProcess)]
    [ProducesResponseType(typeof(ApiResponse<SalaryComponentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SalaryComponentDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateComponent([FromBody] CreateSalaryComponentRequest req)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await payrollService.CreateSalaryComponentAsync(req, userId);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }

    // ==========================================
    // 2. SALARY STRUCTURE
    // ==========================================
    [HttpGet("structures/{employeeId:int}")]
    [HasPermission(Permissions.PayrollRead)]
    [ProducesResponseType(typeof(ApiResponse<SalaryStructureDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SalaryStructureDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSalaryStructure([FromRoute] int employeeId)
    {
        var result = await payrollService.GetEmployeeSalaryStructureAsync(employeeId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("structures")]
    [HasPermission(Permissions.PayrollProcess)]
    [ProducesResponseType(typeof(ApiResponse<SalaryStructureDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SalaryStructureDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignSalaryStructure([FromBody] AssignSalaryStructureRequest req)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await payrollService.AssignSalaryStructureAsync(req, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==========================================
    // 3. TAX ESTIMATE CALCULATOR
    // ==========================================
    [HttpGet("tax-estimate")]
    [ProducesResponseType(typeof(ApiResponse<TaxCalculationResultDto>), StatusCodes.Status200OK)]
    public IActionResult EstimateTax([FromQuery] decimal monthlyGross)
    {
        var result = payrollService.CalculateTax(monthlyGross);
        return Ok(result);
    }

    // ==========================================
    // 4. PAYROLL PROCESSING BATCH
    // ==========================================
    [HttpPost("runs")]
    [HasPermission(Permissions.PayrollProcess)]
    [ProducesResponseType(typeof(ApiResponse<PayrollRunSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PayrollRunSummaryDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessPayroll([FromBody] ProcessPayrollRequest req)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await payrollService.ProcessPayrollAsync(req, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("runs")]
    [HasPermission(Permissions.PayrollRead)]
    [ProducesResponseType(typeof(ApiResponse<List<PayrollRunSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollRuns([FromQuery] int? year, [FromQuery] int? month)
    {
        var result = await payrollService.GetPayrollRunsAsync(year, month);
        return Ok(result);
    }

    [HttpGet("runs/{id:int}")]
    [HasPermission(Permissions.PayrollRead)]
    [ProducesResponseType(typeof(ApiResponse<PayrollRunSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PayrollRunSummaryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayrollRunById([FromRoute] int id)
    {
        var result = await payrollService.GetPayrollRunByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("runs/{id:int}/approve")]
    [HasPermission(Permissions.PayrollApprove)]
    [ProducesResponseType(typeof(ApiResponse<PayrollRunSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PayrollRunSummaryDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApprovePayrollRun([FromRoute] int id)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await payrollService.ApprovePayrollRunAsync(id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("runs/{id:int}/pay")]
    [HasPermission(Permissions.PayrollApprove)]
    [ProducesResponseType(typeof(ApiResponse<PayrollRunSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PayrollRunSummaryDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkPayrollAsPaid([FromRoute] int id)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await payrollService.MarkPayrollRunAsPaidAsync(id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==========================================
    // 5. PAYSLIP DOWNLOAD (QuestPDF)
    // ==========================================
    [HttpGet("payslips/{detailId:int}/pdf")]
    [HasPermission(Permissions.PayrollPayslip)]
    [Produces("application/pdf")]
    public async Task<IActionResult> DownloadPayslipPdf([FromRoute] int detailId)
    {
        try
        {
            var pdfBytes = await payrollService.GeneratePayslipPdfAsync(detailId);
            var filename = $"Payslip_{detailId}_{DateTime.UtcNow:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", filename);
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(ApiResponse.Fail(knf.Message));
        }
    }

    [HttpGet("my-payslips")]
    [HasPermission(Permissions.PayrollPayslip)]
    [ProducesResponseType(typeof(ApiResponse<List<PayrollDetailDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPayslips([FromQuery] int? year)
    {
        var employeeId = currentUser.EmployeeId ?? 1;
        var result = await payrollService.GetEmployeePayslipsAsync(employeeId, year);
        return Ok(result);
    }
}
