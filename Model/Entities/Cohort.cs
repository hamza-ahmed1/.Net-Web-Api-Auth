using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    /// <summary>Intake group students belong to, e.g. "Cohort-2025". Managed by the Admin.</summary>
    public class Cohort : INamedLookup
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
