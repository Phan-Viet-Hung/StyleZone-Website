using DAL_Empty.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.DomainCusTomer.DTOs.QuanLyDonHangCustomerDto
{
    public class QuanLyDonHangCustomerDto
    {
            public Guid? CustomerId { get; set; }
            public Guid? OrderId { get; set; }
            public string? CustomerName { get; set; }
            public string? Address { get; set; }
            public string? PhoneNumber { get; set; }
            public decimal? TotalAmount { get; set; }
            public OrderStatus Status { get; set; }
           
        public List<OrderDetail> Details { get; set; }
        }
}
