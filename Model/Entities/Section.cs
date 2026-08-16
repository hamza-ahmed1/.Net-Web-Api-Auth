using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    public class Section
    {
        [Key]
        public Guid SectionId { get; set; }

        [Required]
        public string SectionName { get; set; } = string.Empty;
        [Required]
        public string IntermediateClass { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<TeacherSectionCourse> TeacherAssignments { get; set; }
            = new List<TeacherSectionCourse>();
    }
}
