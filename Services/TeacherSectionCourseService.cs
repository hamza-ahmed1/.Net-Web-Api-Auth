using Auth.Data;
using Auth.Model.DTOs.TeacherSectionCourse;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class TeacherSectionCourseService: ITeacherSectionCourseService
    {
        private readonly ApplicationDbContext _context;

        public TeacherSectionCourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TeacherSectionCourseDto>> GetAllAsync()
        {
            return await _context.TeacherSectionCourses
                .Include(x => x.Teacher)
                .Include(x => x.Section)
                .Include(x => x.Course)
                .Select(x => MapToDto(x))
                .ToListAsync();
        }

        public async Task<TeacherSectionCourseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _context.TeacherSectionCourses
                .Include(x => x.Teacher)
                .Include(x => x.Section)
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.TeacherSectionCourseId == id);

            return entity is null ? null : MapToDto(entity);
        }

        public async Task<IEnumerable<TeacherSectionCourseDto>> GetAllByTeacherIdAsync(Guid teacherId)
        {
            return await _context.TeacherSectionCourses
                .Include(x => x.Teacher)
                .Include(x => x.Section)
                .Include(x => x.Course)
                .Where(x => x.TeacherId == teacherId)
                .Select(x => MapToDto(x))
                .ToListAsync();
        }

        public async Task<TeacherSectionCourseDto> CreateAsync(TeacherSectionCourseCreateDto dto)
        {
            var entity = new TeacherSectionCourse
            {
                TeacherId = dto.TeacherId,
                SectionId = dto.SectionId,
                CourseId = dto.CourseId,
                AssignedDate = dto.AssignedDate,
                IsActive = dto.IsActive
            };

            _context.TeacherSectionCourses.Add(entity);
            await _context.SaveChangesAsync();

            // reload with navigation properties for the response
            await _context.Entry(entity).Reference(x => x.Teacher).LoadAsync();
            await _context.Entry(entity).Reference(x => x.Section).LoadAsync();
            await _context.Entry(entity).Reference(x => x.Course).LoadAsync();

            return MapToDto(entity);
        }

        public async Task<TeacherSectionCourseDto?> UpdateAsync(Guid id, TeacherSectionCourseUpdateDto dto)
        {
            var entity = await _context.TeacherSectionCourses
                .FirstOrDefaultAsync(x => x.TeacherSectionCourseId == id);

            if (entity is null)
                return null;

            entity.TeacherId = dto.TeacherId;
            entity.SectionId = dto.SectionId;
            entity.CourseId = dto.CourseId;
            entity.AssignedDate = dto.AssignedDate;
            entity.RemovedDate = dto.RemovedDate;
            entity.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            await _context.Entry(entity).Reference(x => x.Teacher).LoadAsync();
            await _context.Entry(entity).Reference(x => x.Section).LoadAsync();
            await _context.Entry(entity).Reference(x => x.Course).LoadAsync();

            return MapToDto(entity);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.TeacherSectionCourses
                .FirstOrDefaultAsync(x => x.TeacherSectionCourseId == id);

            if (entity is null)
                return false;

            _context.TeacherSectionCourses.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static TeacherSectionCourseDto MapToDto(TeacherSectionCourse entity)
        {
            return new TeacherSectionCourseDto
            {
                TeacherSectionCourseId = entity.TeacherSectionCourseId,
                TeacherId = entity.TeacherId,
                Teacher = entity.Teacher == null ? null : new Model.DTOs.TeacherSectionCourse.TeacherInfoDto
                {
                    Teacher_Id = entity.Teacher.Teacher_Id,
                    CNIC = entity.Teacher.CNIC,
                    Qualification = entity.Teacher.Qualification,
                    IdentificationNumber = entity.Teacher.IdentificationNumber,
                    Department = entity.Teacher.Department,
                    DateOfBirth = entity.Teacher.DateOfBirth,
                    HireDate = entity.Teacher.HireDate,
                    Address = entity.Teacher.Address,
                    Salary = entity.Teacher.Salary,
                    IsActive = entity.Teacher.IsActive,
                    UserId = entity.Teacher.UserId
                },
                SectionId = entity.SectionId,
                Section = entity.Section == null ? null : new Model.DTOs.TeacherSectionCourse.SectionInfoDto
                {
                    SectionId = entity.Section.SectionId,
                    SectionName = entity.Section.SectionName,
                    IntermediateClass = entity.Section.IntermediateClass,
                    StartDate = entity.Section.StartDate,
                    IsActive = entity.Section.IsActive,
                    CreatedAt = entity.Section.CreatedAt
                },
                CourseId = entity.CourseId,
                Course = entity.Course == null ? null : new Model.DTOs.TeacherSectionCourse.CourseInfoDto
                {
                    CourseId = entity.Course.CourseId,
                    CourseName = entity.Course.CourseName,
                    CourseDescription = entity.Course.CourseDescription,
                    CourseDuration = entity.Course.CourseDuration,
                    CreatedAt = entity.Course.CreatedAt
                },
                AssignedDate = entity.AssignedDate,
                RemovedDate = entity.RemovedDate,
                IsActive = entity.IsActive
            };
        }
    }

}
