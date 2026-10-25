using FluentValidation;
using HRMS.DTOs;

namespace HRMS.Validators;

public class CheckInRequestValidator : AbstractValidator<CheckInRequest>
{
    public CheckInRequestValidator()
    {
        RuleFor(x => x.Remarks)
            .MaximumLength(500).WithMessage("Remarks cannot exceed 500 characters.");
    }
}

public class CheckOutRequestValidator : AbstractValidator<CheckOutRequest>
{
    public CheckOutRequestValidator()
    {
        RuleFor(x => x.Remarks)
            .MaximumLength(500).WithMessage("Remarks cannot exceed 500 characters.");
    }
}

public class ManualAttendanceRequestValidator : AbstractValidator<ManualAttendanceRequest>
{
    public ManualAttendanceRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("Valid EmployeeId is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => new[] { "Present", "Late", "HalfDay", "Absent", "OnLeave", "Holiday", "Weekend" }.Contains(s))
            .WithMessage("Status must be Present, Late, HalfDay, Absent, OnLeave, Holiday, or Weekend.");

        RuleFor(x => x.Remarks)
            .MaximumLength(500).WithMessage("Remarks cannot exceed 500 characters.");
    }
}

public class RegularizationRequestValidator : AbstractValidator<RegularizationRequestDto>
{
    public RegularizationRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters.");
    }
}

public class RegularizationReviewValidator : AbstractValidator<RegularizationReviewDto>
{
    public RegularizationReviewValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => s == "Approved" || s == "Rejected")
            .WithMessage("Status must be either 'Approved' or 'Rejected'.");

        RuleFor(x => x.Remarks)
            .MaximumLength(500).WithMessage("Remarks cannot exceed 500 characters.");
    }
}
