using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    public class FeeCategory
    {
        public Guid FeeCategoryId { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        public ICollection<FeeType> FeeTypes { get; set; }
    }
}
