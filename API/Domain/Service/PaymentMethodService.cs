using API.Domain.DTOs;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Service
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly DbContextApp _context;

        public PaymentMethodService(DbContextApp context)
        {
            _context = context;
        }

        public async Task<List<PaymentMethodDto>> GetAllAsync()
        {
            return await _context.PaymentMethods
                .Select(p => new PaymentMethodDto
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();
        }
    }
}
