using System.ComponentModel.DataAnnotations;

namespace Auth.Model.DTOs.Section
{
    public class CreateSectionDTO
    {
        [Required]
        public string SectionName { get; set; } = string.Empty;
        [Required]
        public string IntermediateClass { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
