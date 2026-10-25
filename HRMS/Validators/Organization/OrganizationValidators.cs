using FluentValidation;
using HRMS.Dtos.Organization;

namespace HRMS.Validators.Organization;

public class CreateBranchRequestValidator : AbstractValidator<CreateBranchRequest>
{
    public CreateBranchRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Branch name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Branch code is required.")
            .MaximumLength(20);

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format.");
    }
}

public class UpdateBranchRequestValidator : AbstractValidator<UpdateBranchRequest>
{
    public UpdateBranchRequestValidator()
    {
        Include(new CreateBranchRequestValidator());
    }
}

public class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Department code is required.")
            .MaximumLength(20);

        RuleFor(x => x.BranchId)
            .GreaterThan(0).WithMessage("Valid branch must be selected.");
    }
}

public class UpdateDepartmentRequestValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentRequestValidator()
    {
        Include(new CreateDepartmentRequestValidator());
    }
}

public class CreateDesignationRequestValidator : AbstractValidator<CreateDesignationRequest>
{
    public CreateDesignationRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Designation title is required.")
            .MaximumLength(150);

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Designation code is required.")
            .MaximumLength(20);

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Valid department must be selected.");
    }
}

public class UpdateDesignationRequestValidator : AbstractValidator<UpdateDesignationRequest>
{
    public UpdateDesignationRequestValidator()
    {
        Include(new CreateDesignationRequestValidator());
    }
}

public class CreateGradeRequestValidator : AbstractValidator<CreateGradeRequest>
{
    public CreateGradeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Grade name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Grade code is required.")
            .MaximumLength(20);

        RuleFor(x => x.MinSalary)
            .GreaterThanOrEqualTo(0).When(x => x.MinSalary.HasValue)
            .WithMessage("Minimum salary cannot be negative.");

        RuleFor(x => x.MaxSalary)
            .GreaterThanOrEqualTo(x => x.MinSalary ?? 0).When(x => x.MaxSalary.HasValue)
            .WithMessage("Maximum salary must be greater than or equal to minimum salary.");
    }
}

public class UpdateGradeRequestValidator : AbstractValidator<UpdateGradeRequest>
{
    public UpdateGradeRequestValidator()
    {
        Include(new CreateGradeRequestValidator());
    }
}

public class CreateShiftRequestValidator : AbstractValidator<CreateShiftRequest>
{
    public CreateShiftRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Shift name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Shift code is required.")
            .MaximumLength(20);

        RuleFor(x => x.GraceMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Grace minutes cannot be negative.");

        RuleFor(x => x.WorkingHours)
            .GreaterThan(0).WithMessage("Working hours must be greater than 0.")
            .LessThanOrEqualTo(24).WithMessage("Working hours cannot exceed 24 hours.");
    }
}

public class UpdateShiftRequestValidator : AbstractValidator<UpdateShiftRequest>
{
    public UpdateShiftRequestValidator()
    {
        Include(new CreateShiftRequestValidator());
    }
}

public class CreateHolidayRequestValidator : AbstractValidator<CreateHolidayRequest>
{
    public CreateHolidayRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Holiday name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Holiday type is required.")
            .MaximumLength(50);
    }
}

public class UpdateHolidayRequestValidator : AbstractValidator<UpdateHolidayRequest>
{
    public UpdateHolidayRequestValidator()
    {
        Include(new CreateHolidayRequestValidator());
    }
}
