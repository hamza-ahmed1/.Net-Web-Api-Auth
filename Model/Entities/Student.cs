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
        public ICollection<StudentEnrollments> StudentEnrollments { get; set; } = new List<StudentEnrollments>();

    }
}
