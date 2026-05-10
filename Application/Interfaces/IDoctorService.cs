using LicenseManagementAPI.Application.DTOs;

namespace LicenseManagementAPI.Application.Interfaces;

public interface IDoctorService
{
    Task<PagedResult<DoctorDto>> GetDoctorsAsync(DoctorListRequest request);
    Task<DoctorDto> GetByIdAsync(int id);
    Task<DoctorDto> CreateAsync(CreateDoctorRequest request);
    Task<DoctorDto> UpdateAsync(int id, UpdateDoctorRequest request);
    Task UpdateStatusAsync(int id, UpdateStatusRequest request);
    Task DeleteAsync(int id);
    Task<IEnumerable<DoctorDto>> GetExpiredDoctorsAsync();
}
