namespace Auth.Model.DTOs
{
    public class FeeCategoryDto
    {
        public string categoryName { get; set; }
    }

    public class FeeCategoryDetailsDto
    {
        public Guid categoryId { get; set; }
        public string categoryName { get; set; }

    }
}
