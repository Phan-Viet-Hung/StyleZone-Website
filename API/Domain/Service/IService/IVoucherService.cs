
using API.Domain.DTOs;
using API.Domain.Request.VoucherRequest;

namespace API.Domain.Service.IService
{
    public interface IVoucherService
    {
        Task<List<VoucherDto>> GetAllAsync();
        Task<VoucherDto?> GetByIdAsync(Guid id);
        Task<VoucherDto?> CreateAsync(CreateVoucherRequest request);
        Task<VoucherDto?> UpdateAsync(UpdateVoucherRequest request);
        Task<bool> ChangeStatusAsync(ChangeStatusRequest request);
        Task<bool> BulkChangeStatusAsync(BulkStatusChangeRequest request);


    }
}
