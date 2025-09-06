using API.Domain.DTOs;
using API.Domain.Request.ProductDetailRequest;
using API.Domain.Request.VoucherRequest;
using DAL_Empty.Models;

namespace API.Domain.Service.IService
{
    public interface IProductDetailService
    {
        Task<ProductDetailDto> CreateAsync(CreateProductDetailRequest request);
        Task<ProductDetailDto> UpdateAsync(UpdateProductDetailRequest request);
        Task<ProductDetailDto?> GetByIdAsync(Guid id);
        Task<List<ProductDetailDto>> GetAllAsync(Guid? productId = null);
        Task<bool> ChangeStatusAsync(ChangeStatusRequest request);
        Task<bool> BulkChangeStatusAsync(BulkStatusChangeRequest request);
        Task<List<ProductDetailDto>> GetByIdsAsync(List<Guid> ids);
        Task UpdateProductQuantityAfterOrderAsync(List<OrderDetail> orderDetails);
        Task<List<ProductDetailDto>> GetAllWithDisplayPriceAsync(Guid? productId = null);
        Task<List<ProductDetailDto>> GetAvailableForPromotionAsync(Guid? promotionIdToExclude = null);
        Task<List<ProductDetailDto>> GetByPromotionIdAsync(Guid promotionId);
        Task<string> ImportProductDetailFromExcelAsync(string filePath, Guid productId);
    }
}
