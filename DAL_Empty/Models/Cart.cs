using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public class Cart
    {
        [Key]
        public Guid Id { get; set; }

        [Column("CreatedAt")]
        public DateTime? CreateAt { get; set; }

        [ForeignKey("Customer")]
        public Guid? CustomerId { get; set; }

        public virtual Customer? Customer { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
