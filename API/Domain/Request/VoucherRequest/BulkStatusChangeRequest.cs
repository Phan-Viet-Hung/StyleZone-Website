using DAL_Empty.Models;

namespace API.Domain.Request.VoucherRequest
{
    public class BulkStatusChangeRequest
    {
        public List<Guid> Ids { get; set; } = new();
        public string Status { get; set; } = default!;
    }
}
