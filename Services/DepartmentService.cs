using Auth.Data;
using Auth.Model.DTOs.Announcements;
using Auth.Model.DTOs.Departments;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;

        public DepartmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentDto>> GetAllAsync(bool activeOnly)
        {
            var query = _context.Departments.AsNoTracking();
            if (activeOnly)
                query = query.Where(d => d.IsActive);

            return await query
                .OrderBy(d => d.Name)
                .Select(ToDto())
                .ToListAsync();
        }

        public async Task<DepartmentDto?> GetByIdAsync(Guid departmentId)
        {
            return await _context.Departments
                .AsNoTracking()
                .Where(d => d.DepartmentId == departmentId)
                .Select(ToDto())
                .FirstOrDefaultAsync();
        }

        public async Task<Result<DepartmentDto>> CreateAsync(SaveDepartmentDto dto)
        {
            var name = dto.Name?.Trim() ?? string.Empty;
            if (name.Length == 0)
                return Result<DepartmentDto>.Failure("Department name is required.");

            if (await NameTakenAsync(name, null))
                return Result<DepartmentDto>.Failure($"A department named \"{name}\" already exists.");

            var department = new Department
            {
                DepartmentId = Guid.NewGuid(),
                Name = name,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                IsActive = dto.IsActive
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return Result<DepartmentDto>.Ok((await GetByIdAsync(department.DepartmentId))!);
        }

        public async Task<Result<DepartmentDto>> UpdateAsync(Guid departmentId, SaveDepartmentDto dto)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
                return Result<DepartmentDto>.NotFound("Department not found.");

            var name = dto.Name?.Trim() ?? string.Empty;
            if (name.Length == 0)
                return Result<DepartmentDto>.Failure("Department name is required.");

            if (await NameTakenAsync(name, departmentId))
                return Result<DepartmentDto>.Failure($"A department named \"{name}\" already exists.");

            department.Name = name;
            department.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            department.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            return Result<DepartmentDto>.Ok((await GetByIdAsync(departmentId))!);
        }

        public async Task<Result<bool>> DeleteAsync(Guid departmentId)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
                return Result<bool>.NotFound("Department not found.");

            var inUse = await _context.Teachers.AnyAsync(t => t.DepartmentId == departmentId)
                || await _context.Students.AnyAsync(s => s.DepartmentId == departmentId);
            if (inUse)
                return Result<bool>.Failure("This department still has teachers or students. Move them first, or deactivate the department instead.");

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }

        /// <summary>True when an active or inactive department other than <paramref name="exceptId"/> uses the name.</summary>
        public async Task<bool> NameTakenAsync(string name, Guid? exceptId)
        {
            // SQL Server's default collation compares case-insensitively, matching the unique index
            return await _context.Departments.AnyAsync(d => d.Name == name && d.DepartmentId != exceptId);
        }

        private static System.Linq.Expressions.Expression<Func<Department, DepartmentDto>> ToDto()
        {
            return d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                Name = d.Name,
                Description = d.Description,
                IsActive = d.IsActive,
                TeacherCount = d.Teachers.Count(),
                StudentCount = d.Students.Count()
            };
        }
    }
}
