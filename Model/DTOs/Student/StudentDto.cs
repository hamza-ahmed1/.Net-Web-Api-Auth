using System.ComponentModel.DataAnnotations;

namespace Auth.Model.DTOs.Student
{
    public class StudentDto
    {
        public Guid StudentId { get; set; }
        public string UserId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }

        public string Section { get; set; } = string.Empty;
        public Guid? SectionId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string Department { get; set; } = string.Empty;
        public Guid? CohortId { get; set; }
        public string Cohort { get; set; } = string.Empty;
        public Guid? CampusId { get; set; }
        public string Campus { get; set; } = string.Empty;
        public string MajorSubject { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        /// <summary>"Active" or "Inactive", for display.</summary>
        public string Status => IsActive ? "Active" : "Inactive";
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string CNIC { get; set; } = string.Empty;
    }

    /// <summary>Fields every student record must have; validated on create and update.</summary>
    public abstract class StudentProfileFieldsDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Student code is required.")]
        [RegularExpression(@"^\d{1,10}$", ErrorMessage = "Student code must be numeric, up to 10 digits.")]
        public string StudentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\+?[0-9][0-9\- ]{8,14}[0-9]$", ErrorMessage = "Enter a valid phone number, e.g. 0300-1234567.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Father's name is required.")]
        [StringLength(100)]
        public string FatherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required.")]
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Gender must be Male or Female.")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Major subject is required.")]
        [StringLength(100)]
        public string MajorSubject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cohort is required.")]
        public Guid? CohortId { get; set; }

        [Required(ErrorMessage = "Campus is required.")]
        public Guid? CampusId { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string CNIC { get; set; } = string.Empty;
    }

    public class StudentCreateDto : StudentProfileFieldsDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public Guid SectionId { get; set; }   // ← this was missing, causing CS1061
        public Guid DepartmentId { get; set; }
    }

    public class StudentUpdateDto : StudentProfileFieldsDto
    {
        public string? Email { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public Guid? SectionId { get; set; }
        /// <summary>Optional; when sent, moves the student to this (active) department.</summary>
        public Guid? DepartmentId { get; set; }
    }

    public class StudentStatusDto
    {
        public bool IsActive { get; set; }
    }

    class StudentMarksDto
    {
        public Guid StudentId { get; set; }

        public Guid EnrollmentId { get; set; }
        public string FullName { get; set; } = string.Empty;

    }
}
