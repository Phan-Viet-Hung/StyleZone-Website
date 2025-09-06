using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.SupplierRequest
{
    public class UpdateSupplierRequest : CreateSupplierRequest
    {
        [Required]
        public Guid Id { get; set; }
    }
}
