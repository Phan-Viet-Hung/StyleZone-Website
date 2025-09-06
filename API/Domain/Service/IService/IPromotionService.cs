using API.Domain.DTOs;
using API.Domain.Request.PromotionRequest;
using API.Domain.Request.VoucherRequest;
using DAL_Empty.Models;

namespace API.Domain.Service.IService
{
    public interface IPromotionService
    {
        Task<PromotionDto> CreateAsync(CreatePromotionRequest request);
        Task<PromotionDto> UpdateAsync(UpdatePromotionRequest request);
        Task<PromotionDto?> GetByIdAsync(Guid id);
        Task<List<PromotionDto>> GetAllAsync(); 
        Task<PromotionDetailDto> GetDetailByIdAsync(Guid id);
        Task<List<ProductDetailDto>> GetAllProductsAsync();
        Task ChangePromotionStatusAsync(Guid promotionId, VoucherStatus newStatus);
        Task<bool> BulkChangePromotionStatusAsync(BulkStatusChangeRequest request);


    }
}
