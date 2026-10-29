using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    /// <summary>Campus a student studies at, e.g. "Qayyum-Abad Campus". Managed by the Admin.</summary>
    public class Campus : INamedLookup
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }

    /// <summary>Simple admin-managed list item (cohorts, campuses) that students are assigned to.</summary>
    public interface INamedLookup
    {
        Guid Id { get; set; }
        string Name { get; set; }
        bool IsActive { get; set; }
        DateTime CreatedAt { get; set; }
    }
}
