using LicenseManagementAPI.Application.DTOs;
using LicenseManagementAPI.Domain.Entities;

namespace LicenseManagementAPI.Application.Mapping;

public static class DoctorMapper
{
    public static DoctorDto ToDto(Doctor doctor) => new()
    {
        Id                 = doctor.Id,
        FullName           = doctor.FullName,
        Email              = doctor.Email,
        Specialization     = doctor.Specialization,
        LicenseNumber      = doctor.LicenseNumber,
        LicenseExpiryDate  = doctor.LicenseExpiryDate,
        Status             = doctor.Status,
        CreatedDate        = doctor.CreatedDate,
        ModifiedDate       = doctor.ModifiedDate,
    };

    public static IEnumerable<DoctorDto> ToDtoList(IEnumerable<Doctor> doctors) =>
        doctors.Select(ToDto);
}
