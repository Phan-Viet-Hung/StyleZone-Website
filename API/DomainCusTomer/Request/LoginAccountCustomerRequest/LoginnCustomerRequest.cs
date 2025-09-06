using System.ComponentModel.DataAnnotations;

namespace API.DomainCusTomer.Request.AccountCustomerRequest
{
    public class LoginnCustomerRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
