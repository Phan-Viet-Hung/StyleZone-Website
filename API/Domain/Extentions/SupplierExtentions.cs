using API.Domain.DTOs;
using DAL_Empty.Models;

namespace API.Domain.Extentions
{
    public static class SupplierExtentions
    {
        public static SupplierDto ToDto(this Supplier supplier)
        {
            return new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Contact = supplier.Contact,
                Email = supplier.Email,
                Address = supplier.Address
            };
        }
    }
}
