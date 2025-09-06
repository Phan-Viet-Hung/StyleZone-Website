using DAL_Empty.Models;

namespace API.Domain.Request.VoucherRequest
{
    public class ChangeStatusRequest
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = default!;
    }
}
