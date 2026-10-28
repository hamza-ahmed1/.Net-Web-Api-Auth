using Auth.Model.DTOs.Announcements;
using Auth.Model.DTOs.Departments;

namespace Auth.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAllAsync(bool activeOnly);

        Task<DepartmentDto?> GetByIdAsync(Guid departmentId);

        Task<Result<DepartmentDto>> CreateAsync(SaveDepartmentDto dto);

        Task<Result<DepartmentDto>> UpdateAsync(Guid departmentId, SaveDepartmentDto dto);

        Task<Result<bool>> DeleteAsync(Guid departmentId);
    }
}
