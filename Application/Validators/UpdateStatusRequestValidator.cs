using FluentValidation;
using LicenseManagementAPI.Application.DTOs;
using LicenseManagementAPI.Domain.Enums;

namespace LicenseManagementAPI.Application.Validators;

public class UpdateStatusRequestValidator : AbstractValidator<UpdateStatusRequest>
{
    public UpdateStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(DoctorStatus.IsValid)
            .WithMessage($"Status must be one of: {string.Join(", ", DoctorStatus.All)}.");
    }
}
