namespace Auth.Model.DTOs.Student
{
    public class StudentDto
    {
        public Guid StudentId { get; set; }
        public string UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }

        public string Section { get; set; } = string.Empty;
        public Guid? SectionId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string Department { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string CNIC { get; set; } = string.Empty;
    }

    public class StudentCreateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public string CNIC { get; set; } = string.Empty;
        public Guid SectionId { get; set; }   // ← this was missing, causing CS1061
        public Guid DepartmentId { get; set; }
    }

    public class StudentUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string CNIC { get; set; } = string.Empty;
        public Guid? SectionId { get; set; }
        /// <summary>Optional; when sent, moves the student to this (active) department.</summary>
        public Guid? DepartmentId { get; set; }
    }

    class StudentMarksDto
    {
        public Guid StudentId { get; set; }

        public Guid EnrollmentId { get; set; }
        public string FullName { get; set; } = string.Empty;

    }
}