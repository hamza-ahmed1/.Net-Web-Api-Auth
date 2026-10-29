using System.ComponentModel.DataAnnotations;

namespace Auth.Model.DTOs.Lookups
{
    /// <summary>A cohort or campus with the number of students assigned to it.</summary>
    public class LookupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int StudentCount { get; set; }
    }

    public class SaveLookupDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
