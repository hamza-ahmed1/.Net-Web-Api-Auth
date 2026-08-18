namespace Auth.Model.DTOs.Student
{
    public class StudentDto
    {
        public Guid StudentId { get; set; }
        public string UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
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

        public Guid sectionId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public string CNIC { get; set; } = string.Empty;
    }

    public class StudentUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string CNIC { get; set; } = string.Empty;
    }
}