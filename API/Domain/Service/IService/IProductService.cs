using API.Domain.DTOs;
using API.Domain.Request.ProductDetailRequest;
using API.Domain.Request.ProductRequest;

namespace API.Domain.Service.IService
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(Guid id);
        Task<ProductDto> CreateAsync(CreateProductRequest request, Guid userId);
        Task<ProductDto> UpdateAsync(UpdateProductRequest request, Guid userId);
    }
}
