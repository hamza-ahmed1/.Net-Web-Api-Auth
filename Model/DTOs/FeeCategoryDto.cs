using System.ComponentModel.DataAnnotations;

namespace Auth.Model.DTOs
{
    public class FeeCategoryDto
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name can be at most 100 characters.")]
        public string categoryName { get; set; }
    }

    public class FeeCategoryDetailsDto
    {
        public Guid categoryId { get; set; }
        public string categoryName { get; set; }

    }
}
