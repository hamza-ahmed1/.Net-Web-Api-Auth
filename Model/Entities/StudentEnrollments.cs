using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Model.Entities
{
    public class StudentEnrollments
    {
        [Key]
        public Guid StudentEnrollmentId { get; set; }

        [ForeignKey(nameof(Student))]
        public Guid StudentId { get; set; }
        public Student? Student { get; set; }

        [ForeignKey(nameof(Section))]
        public Guid SectionId { get; set; }
        public Section? EnrolledSection { get; set; }

        public DateTime EnrolledDate { get; set; } = DateTime.UtcNow;

        public DateTime? WithdrawnDate { get; set; }

        public bool IsActive { get; set; } = true;
    }

   
}
