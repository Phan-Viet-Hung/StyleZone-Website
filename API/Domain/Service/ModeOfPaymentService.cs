using API.Domain.DTOs;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Service
{
    public class ModeOfPaymentService : IModeOfPaymentService
    {
        private readonly DbContextApp _context;

        public ModeOfPaymentService(DbContextApp context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ModeOfPaymentDto>> GetAllAsync()
        {
            return await _context.ModeOfPayments
                .Select(m => new ModeOfPaymentDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Creator = m.Creator,
                    Fixer = m.Fixer,
                    Status = m.Status,
                    CreationDate = m.CreationDate,
                    EditDate = m.EditDate
                })
                .ToListAsync();
        }
    }
}
