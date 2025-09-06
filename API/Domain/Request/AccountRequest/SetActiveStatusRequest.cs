
namespace API.Domain.Request.AccountRequest
{
    public class SetActiveStatusRequest
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
    }
}
