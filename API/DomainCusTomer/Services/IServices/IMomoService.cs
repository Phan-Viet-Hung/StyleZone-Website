using API.DomainCusTomer.DTOs.MoMo;

namespace API.DomainCusTomer.Services.IServices
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);

        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
        MomoExecuteResponseModel PaymentExecuteAsync(IFormCollection collection);

        bool ValidateSignature(IQueryCollection collection);
        bool ValidateSignature(IFormCollection collection);
    }
}
