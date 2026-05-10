using System.ComponentModel.DataAnnotations;

namespace LicenseManagementAPI.Application.DTOs;

public class CreateDoctorRequest
{
    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(200, ErrorMessage = "Full name cannot exceed 200 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Specialization is required.")]
    [MaxLength(200, ErrorMessage = "Specialization cannot exceed 200 characters.")]
    public string Specialization { get; set; } = string.Empty;

    [Required(ErrorMessage = "License number is required.")]
    [MaxLength(100, ErrorMessage = "License number cannot exceed 100 characters.")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "License expiry date is required.")]
    public DateTime LicenseExpiryDate { get; set; }
}
