namespace API.Domain.DTOs.MoMo
{
    public class MomoExecuteResponseModel
    {
        public string OrderId { get; set; }
        public string Amount { get; set; }
        public string FullName { get; set; }
        public string OrderInfo { get; set; }
        public string ErrorCode { get; set; }      // Bắt buộc
        public string TransId { get; set; }        // Bắt buộc
        public string Signature { get; set; }      // Bắt buộc
        public string Message { get; set; }        // Khuyến khích (log lỗi)
        public string LocalMessage { get; set; }   // Khuyến khích (log lỗi)
        public string ResultCode { get; set; }

    }
}
