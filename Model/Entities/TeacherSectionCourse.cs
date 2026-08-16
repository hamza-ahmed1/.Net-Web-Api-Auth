using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    public class TeacherSectionCourse
    {
        [Key]
        public Guid TeacherSectionCourseId { get; set; }

        public Guid TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        public Guid SectionId { get; set; }
        public Section? Section { get; set; }

        public Guid CourseId { get; set; }
        public Course? Course { get; set; }
        public DateTime AssignedDate { get; set; }

        public DateTime? RemovedDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
