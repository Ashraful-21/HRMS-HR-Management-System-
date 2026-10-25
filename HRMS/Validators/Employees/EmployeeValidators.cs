using FluentValidation;
using HRMS.Dtos.Employees;

namespace HRMS.Validators.Employees;

public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .Must(g => g is "Male" or "Female" or "Other")
            .WithMessage("Gender must be 'Male', 'Female', or 'Other'.");

        RuleFor(x => x.WorkEmail)
            .NotEmpty().WithMessage("Work email is required.")
            .EmailAddress().WithMessage("A valid corporate email is required.")
            .MaximumLength(200);

        RuleFor(x => x.PersonalEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.PersonalEmail))
            .WithMessage("A valid personal email is required.");

        RuleFor(x => x.BranchId)
            .GreaterThan(0).WithMessage("Branch must be selected.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Department must be selected.");

        RuleFor(x => x.DesignationId)
            .GreaterThan(0).WithMessage("Designation must be selected.");

        RuleFor(x => x.EmploymentType)
            .NotEmpty().WithMessage("Employment type is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.");
    }
}

public class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator()
    {
        Include(new CreateEmployeeRequestValidator());
    }
}

public class UpsertEmployeeAddressRequestValidator : AbstractValidator<UpsertEmployeeAddressRequest>
{
    public UpsertEmployeeAddressRequestValidator()
    {
        RuleFor(x => x.AddressType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(250);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}

public class UpsertEmergencyContactRequestValidator : AbstractValidator<UpsertEmergencyContactRequest>
{
    public UpsertEmergencyContactRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Relationship).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
    }
}

public class UpsertBankAccountRequestValidator : AbstractValidator<UpsertBankAccountRequest>
{
    public UpsertBankAccountRequestValidator()
    {
        RuleFor(x => x.BankName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.AccountNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AccountTitle).NotEmpty().MaximumLength(150);
    }
}

public class UpsertEducationRequestValidator : AbstractValidator<UpsertEducationRequest>
{
    public UpsertEducationRequestValidator()
    {
        RuleFor(x => x.Degree).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Institution).NotEmpty().MaximumLength(200);
    }
}

public class UpsertExperienceRequestValidator : AbstractValidator<UpsertExperienceRequest>
{
    public UpsertExperienceRequestValidator()
    {
        RuleFor(x => x.Company).NotEmpty().MaximumLength(200);
    }
}

public class CreateLifecycleEventRequestValidator : AbstractValidator<CreateLifecycleEventRequest>
{
    public CreateLifecycleEventRequestValidator()
    {
        RuleFor(x => x.EventType).NotEmpty().MaximumLength(50);
    }
}
