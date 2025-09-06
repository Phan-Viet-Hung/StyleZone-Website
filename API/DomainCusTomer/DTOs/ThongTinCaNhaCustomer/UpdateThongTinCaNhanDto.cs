using DAL_Empty.Models;
using DAL_Empty.Validations;
using System.ComponentModel.DataAnnotations;

namespace API.DomainCusTomer.DTOs.ThongTinCaNhaCustomer
{
    public class UpdateThongTinCaNhanDto
    {
        public string? Fullname { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserName { get; set; }
        public GenderEnum? Gender { get; set; }
        
    }
}
