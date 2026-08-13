using System.ComponentModel.DataAnnotations;

namespace Auth.Model.DTOs.Courses
{
    public class CreateCourseDto
    {
        [Required]
        public string CourseName { get; set; } = string.Empty;
        [Required]
        public string CourseDescription { get; set; } = string.Empty;
        [Required]
        public int CourseDuration { get; set; }

    }
}
