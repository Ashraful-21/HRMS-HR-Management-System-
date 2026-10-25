using FluentValidation;
using HRMS.DTOs;

namespace HRMS.Validators;

public class CreateSalaryComponentValidator : AbstractValidator<CreateSalaryComponentRequest>
{
    public CreateSalaryComponentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Component name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Component code is required.")
            .MaximumLength(20);

        RuleFor(x => x.Type)
            .Must(t => t == "Earning" || t == "Deduction")
            .WithMessage("Component type must be either 'Earning' or 'Deduction'.");
    }
}

public class AssignSalaryStructureValidator : AbstractValidator<AssignSalaryStructureRequest>
{
    public AssignSalaryStructureValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("Valid EmployeeId is required.");

        RuleFor(x => x.GrossSalary)
            .GreaterThan(0).WithMessage("Gross salary must be greater than 0.");

        RuleFor(x => x.Components)
            .NotEmpty().WithMessage("At least one salary component is required in the structure.");
    }
}

public class ProcessPayrollValidator : AbstractValidator<ProcessPayrollRequest>
{
    public ProcessPayrollValidator()
    {
        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");

        RuleFor(x => x.Year)
            .InclusiveBetween(2020, 2100).WithMessage("Valid year is required.");

        RuleFor(x => x.WorkingDays)
            .InclusiveBetween(1, 31).WithMessage("Working days must be between 1 and 31.");
    }
}
