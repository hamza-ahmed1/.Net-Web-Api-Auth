using Auth.Model.DTOs.Courses;
using Auth.Model.Entities;

namespace Auth.Services.Interfaces
{
    public interface ICourseService
    {
        Task<Course> CreateCourse(CreateCourseDto dto);

        Task<Course?> GetCourseById(Guid courseId);

        Task<List<Course>> GetAllCourses();

        Task<Course?> UpdateCourse(Guid courseId, CreateCourseDto dto);

        Task<bool> DeleteCourse(Guid courseId);
    }
}

