using Auth.Data;
using Auth.Model.DTOs.Courses;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        // CREATE
        public async Task<Course> CreateCourse(CreateCourseDto dto)
        {
            var course = new Course
            {
                CourseName = dto.CourseName,
                CourseCode = NormalizeCode(dto.CourseCode),
                CourseDescription = dto.CourseDescription,
                CourseDuration = dto.CourseDuration
            };

            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            return course;
        }

        // GET BY ID
        public async Task<Course?> GetCourseById(Guid courseId)
        {
            return await _context.Courses
                .FindAsync(courseId);
        }

        // GET ALL
        public async Task<List<Course>> GetAllCourses()
        {
            return await _context.Courses
                .ToListAsync();
        }

        // UPDATE
        public async Task<Course?> UpdateCourse(
            Guid courseId,
            CreateCourseDto dto)
        {
            var course = await _context.Courses
                .FindAsync(courseId);

            if (course == null)
            {
                return null;
            }

            course.CourseName = dto.CourseName;
            course.CourseCode = NormalizeCode(dto.CourseCode);
            course.CourseDescription = dto.CourseDescription;
            course.CourseDuration = dto.CourseDuration;

            await _context.SaveChangesAsync();

            return course;
        }

        // Codes are stored trimmed and upper-cased; blank means "no code"
        private static string? NormalizeCode(string? code)
        {
            return string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
        }

        // DELETE
        public async Task<bool> DeleteCourse(Guid courseId)
        {
            var course = await _context.Courses
                .FindAsync(courseId);

            if (course == null)
            {
                return false;
            }

            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
