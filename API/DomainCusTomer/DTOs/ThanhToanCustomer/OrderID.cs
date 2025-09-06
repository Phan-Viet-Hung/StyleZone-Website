using System.Text.Json.Serialization;

namespace API.DomainCusTomer.DTOs.ThanhToanCustomer
{
    public class OrderID
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
    }
}
