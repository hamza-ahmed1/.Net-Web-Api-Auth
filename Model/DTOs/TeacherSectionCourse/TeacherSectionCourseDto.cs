namespace Auth.Model.DTOs.TeacherSectionCourse
{
    // Used for GET (single + list) responses
    public class TeacherSectionCourseDto
    {
        public Guid TeacherSectionCourseId { get; set; }

        public Guid TeacherId { get; set; }
        // lightweight nested DTO to avoid circular graphs
        public TeacherInfoDto? Teacher { get; set; }

        public Guid SectionId { get; set; }
        public SectionInfoDto? Section { get; set; }

        public Guid CourseId { get; set; }
        public CourseInfoDto? Course { get; set; }

        public DateTime AssignedDate { get; set; }

        public DateTime? RemovedDate { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // lightweight teacher info (no back references)
    public class TeacherInfoDto
    {
        public Guid Teacher_Id { get; set; }
        public string CNIC { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime HireDate { get; set; }
        public string Address { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public bool IsActive { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class SectionInfoDto
    {
        public Guid SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string IntermediateClass { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CourseInfoDto
    {
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseDescription { get; set; } = string.Empty;
        public int CourseDuration { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Used for POST (create) — no Id, navigation props not needed
    public class TeacherSectionCourseCreateDto
    {
        public Guid TeacherId { get; set; }
        public Guid SectionId { get; set; }
        public Guid CourseId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

    // Used for PUT (update) — Id comes from route, not body
    public class TeacherSectionCourseUpdateDto
    {
        public Guid TeacherId { get; set; }
        public Guid SectionId { get; set; }
        public Guid CourseId { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? RemovedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
