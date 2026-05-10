using System.ComponentModel.DataAnnotations;

namespace LicenseManagementAPI.Application.DTOs;

public class UpdateStatusRequest
{
    [Required(ErrorMessage = "Status is required.")]
    [RegularExpression("^(Active|Expired|Suspended)$",
        ErrorMessage = "Status must be one of: Active, Expired, Suspended.")]
    public string Status { get; set; } = string.Empty;
}
