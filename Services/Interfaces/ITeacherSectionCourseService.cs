using Auth.Model.DTOs.TeacherSectionCourse;

namespace Auth.Services.Interfaces
{
    public interface ITeacherSectionCourseService
    {
        Task<IEnumerable<TeacherSectionCourseDto>> GetAllAsync();
        Task<TeacherSectionCourseDto?> GetByIdAsync(Guid id);
        Task<TeacherSectionCourseDto> CreateAsync(TeacherSectionCourseCreateDto dto);
        Task<TeacherSectionCourseDto?> UpdateAsync(Guid id, TeacherSectionCourseUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
