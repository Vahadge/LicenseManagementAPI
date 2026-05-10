using System.ComponentModel.DataAnnotations;

namespace LicenseManagementAPI.Application.DTOs;

public class DoctorListRequest
{
    public string? Search { get; set; }

    [RegularExpression("^(Active|Expired|Suspended)$",
        ErrorMessage = "Status must be one of: Active, Expired, Suspended.")]
    public string? Status { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be at least 1.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;
}
