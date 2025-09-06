using API.Domain.DTOs;
using API.DomainCusTomer.Services.IServices;

using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.DomainCusTomer.Services
{
    public class OrderHistoryService : IOrderHistoryService
    {
        private readonly DbContextApp _context;

        public OrderHistoryService(DbContextApp context)
        {
            _context = context;
        }

        public async Task<List<OrderHistoryDto>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.OrderHistories
                .Where(h => h.BillId == orderId)
                .OrderByDescending(h => h.updateAt)
                .Select(h => new OrderHistoryDto
                {
                    Id = h.Id,
                    BillId = h.BillId,
                    Description = h.Description,
                    Amount = h.amount,
                    CreateAt = h.createAt,
                    UpdateAt = h.updateAt,
                })
                .ToListAsync();
        }
    }
}
