namespace Auth.Model.Entities
{
    public class FeeCategory
    {
        public Guid FeeCategoryId { get; set; }
        public string Name { get; set; }  

        public ICollection<FeeType> FeeTypes { get; set; }
    }
}
