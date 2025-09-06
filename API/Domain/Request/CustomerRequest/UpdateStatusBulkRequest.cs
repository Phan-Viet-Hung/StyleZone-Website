namespace API.Domain.Request.CustomerRequest
{
    public class UpdateStatusBulkRequest
    {
        public List<Guid> Ids { get; set; } = new();
        public string Status { get; set; } = string.Empty;
    }
}
