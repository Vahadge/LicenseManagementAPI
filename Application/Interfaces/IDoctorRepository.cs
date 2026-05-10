using LicenseManagementAPI.Application.DTOs;

namespace LicenseManagementAPI.Application.Interfaces;

public interface IDoctorRepository
{
    Task<PagedResult<DoctorDto>> GetDoctorsAsync(DoctorListRequest request);
    Task<DoctorDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateDoctorRequest request);
    Task UpdateAsync(int id, UpdateDoctorRequest request);
    Task UpdateStatusAsync(int id, string status);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> LicenseNumberExistsAsync(string licenseNumber, int? excludeId = null);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<IEnumerable<DoctorDto>> GetExpiredDoctorsAsync();
    Task<int> MarkExpiredAsync();
}
