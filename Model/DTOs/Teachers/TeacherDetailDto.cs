using Auth.Model.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Model.DTOs.Teachers
{
    public class TeacherDetailDto
    {
        public Guid Teacher_id { get; set; }
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Fullname { get; set; } = string.Empty;
        [Required]
        [StringLength(15)]
        public string CNIC { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Qualification { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string IdentificationNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public DateTime HireDate { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        public decimal Salary { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>Primary role for display/actions (e.g. Teacher, HOD).</summary>
        public string Role { get; set; } = "Teacher";

        /// <summary>Distinct section labels, e.g. "XI - Pre-Engineering (A)".</summary>
        public List<string> AssignedSections { get; set; } = new();

        /// <summary>Distinct course names assigned to this teacher.</summary>
        public List<string> AssignedCourses { get; set; } = new();

        /// <summary>Course · section pairs for display, e.g. "Physics · XI - Pre-Engineering (A)".</summary>
        public List<string> Assignments { get; set; } = new();
    }
}
