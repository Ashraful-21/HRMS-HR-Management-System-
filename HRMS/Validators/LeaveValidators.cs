using FluentValidation;
using HRMS.DTOs;

namespace HRMS.Validators;

public class CreateLeaveTypeValidator : AbstractValidator<CreateLeaveTypeRequest>
{
    public CreateLeaveTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Leave type name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Leave type code is required.")
            .MaximumLength(20);

        RuleFor(x => x.DaysAllowed)
            .GreaterThanOrEqualTo(0).WithMessage("Days allowed must be 0 or greater.");

        RuleFor(x => x.MaxCarryForwardDays)
            .GreaterThanOrEqualTo(0).When(x => x.IsCarryForwardAllowed)
            .WithMessage("Max carry forward days must be 0 or greater.");
    }
}

public class UpdateLeaveTypeValidator : AbstractValidator<UpdateLeaveTypeRequest>
{
    public UpdateLeaveTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Leave type name is required.")
            .MaximumLength(100);

        RuleFor(x => x.DaysAllowed)
            .GreaterThanOrEqualTo(0).WithMessage("Days allowed must be 0 or greater.");
    }
}

public class ApplyLeaveValidator : AbstractValidator<ApplyLeaveRequest>
{
    public ApplyLeaveValidator()
    {
        RuleFor(x => x.LeaveTypeId)
            .GreaterThan(0).WithMessage("Valid leave type is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after start date.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters.");

        RuleFor(x => x.HalfDayType)
            .Must(t => string.IsNullOrEmpty(t) || t == "FirstHalf" || t == "SecondHalf")
            .WithMessage("Half day type must be 'FirstHalf' or 'SecondHalf'.");
    }
}

public class ReviewLeaveApplicationValidator : AbstractValidator<ReviewLeaveApplicationRequest>
{
    public ReviewLeaveApplicationValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => s == "Approved" || s == "Rejected")
            .WithMessage("Status must be either 'Approved' or 'Rejected'.");

        RuleFor(x => x.Remarks)
            .MaximumLength(500).WithMessage("Remarks cannot exceed 500 characters.");
    }
}
