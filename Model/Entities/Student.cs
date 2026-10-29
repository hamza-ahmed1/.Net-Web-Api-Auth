using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Model.Entities
{
    public class Student
    {

        [Key]
        public Guid StudentId { get; set; }
        // Foreign Key to Identity User
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public string CNIC { get; set; } = string.Empty;

        /// <summary>Institution-issued student code: unique, 1-10 digits (e.g. "2001061").</summary>
        [Required]
        [StringLength(10)]
        public string StudentCode { get; set; } = string.Empty;

        /// <summary>Inactive students cannot sign in.</summary>
        public bool IsActive { get; set; } = true;

        public Guid? CohortId { get; set; }

        [ForeignKey(nameof(CohortId))]
        public Cohort? Cohort { get; set; }

        public Guid? CampusId { get; set; }

        [ForeignKey(nameof(CampusId))]
        public Campus? Campus { get; set; }

        public string MajorSubject { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        public string FatherName { get; set; } = string.Empty;

        public string CurrentAdress { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty;

        public Guid? DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department? Department { get; set; }
        public ICollection<StudentEnrollments> StudentEnrollments { get; set; } = new List<StudentEnrollments>();

    }
}
