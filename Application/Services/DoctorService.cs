using LicenseManagementAPI.Application.DTOs;
using LicenseManagementAPI.Application.Interfaces;
using LicenseManagementAPI.Application.Mapping;
using LicenseManagementAPI.Common.Exceptions;

namespace LicenseManagementAPI.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _repository;

    public DoctorService(IDoctorRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<DoctorDto>> GetDoctorsAsync(DoctorListRequest request)
    {
        var paged = await _repository.GetDoctorsAsync(request);
        return new PagedResult<DoctorDto>
        {
            Data       = DoctorMapper.ToDtoList(paged.Data).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize   = paged.PageSize,
        };
    }

    public async Task<DoctorDto> GetByIdAsync(int id)
    {
        var doctor = await _repository.GetByIdAsync(id);
        if (doctor is null)
            throw new NotFoundException($"Doctor with ID {id} was not found.");
        return DoctorMapper.ToDto(doctor);
    }

    public async Task<DoctorDto> CreateAsync(CreateDoctorRequest request)
    {
        if (await _repository.LicenseNumberExistsAsync(request.LicenseNumber))
            throw new ConflictException($"License number '{request.LicenseNumber}' is already registered.");

        if (await _repository.EmailExistsAsync(request.Email))
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        var id = await _repository.CreateAsync(request);
        var doctor = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve the created doctor.");
        return DoctorMapper.ToDto(doctor);
    }

    public async Task<DoctorDto> UpdateAsync(int id, UpdateDoctorRequest request)
    {
        if (!await _repository.ExistsAsync(id))
            throw new NotFoundException($"Doctor with ID {id} was not found.");

        if (await _repository.LicenseNumberExistsAsync(request.LicenseNumber, excludeId: id))
            throw new ConflictException($"License number '{request.LicenseNumber}' is already registered to another doctor.");

        if (await _repository.EmailExistsAsync(request.Email, excludeId: id))
            throw new ConflictException($"Email '{request.Email}' is already registered to another doctor.");

        await _repository.UpdateAsync(id, request);
        var doctor = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve the updated doctor.");
        return DoctorMapper.ToDto(doctor);
    }

    public async Task UpdateStatusAsync(int id, UpdateStatusRequest request)
    {
        if (!await _repository.ExistsAsync(id))
            throw new NotFoundException($"Doctor with ID {id} was not found.");

        await _repository.UpdateStatusAsync(id, request.Status);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            throw new NotFoundException($"Doctor with ID {id} was not found.");

        await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<DoctorDto>> GetExpiredDoctorsAsync()
    {
        var doctors = await _repository.GetExpiredDoctorsAsync();
        return DoctorMapper.ToDtoList(doctors);
    }
}
