using LicenseManagementAPI.Application.DTOs;
using LicenseManagementAPI.Domain.Entities;

namespace LicenseManagementAPI.Application.Interfaces;

public interface IDoctorRepository
{
    Task<PagedResult<Doctor>> GetDoctorsAsync(DoctorListRequest request);
    Task<Doctor?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateDoctorRequest request);
    Task UpdateAsync(int id, UpdateDoctorRequest request);
    Task UpdateStatusAsync(int id, string status);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> LicenseNumberExistsAsync(string licenseNumber, int? excludeId = null);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<IEnumerable<Doctor>> GetExpiredDoctorsAsync();
    Task<int> MarkExpiredAsync();
}
