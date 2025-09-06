using API.Domain.DTOs;
using API.Domain.Extentions;
using API.Domain.Request.SupplierRequest;
using API.Services;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace API.Domain.Service
{
    public class SupplierService : ISupplierService
    {
        private readonly DbContextApp _context;

        public SupplierService(DbContextApp context)
        {
            _context = context;
        }

        public async Task<List<SupplierDto>> GetAllAsync()
        {
            return await _context.Suppliers
                .Select(s => s.ToDto())
                .ToListAsync();
        }

        public async Task<SupplierDto?> GetByIdAsync(Guid id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            return supplier?.ToDto();
        }

        //public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request)
        //{
        //    var supplier = new Supplier
        //    {
        //        Id = Guid.NewGuid(),
        //        Name = request.Name,
        //        Contact = request.Contact,
        //        Email = request.Email,
        //        Address = request.Address
        //    };

        //    _context.Suppliers.Add(supplier);
        //    await _context.SaveChangesAsync();

        //    return supplier.ToDto();
        //}
        public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request)
        {
            var name = request.Name.ToLower().Trim();
            var email = request.Email.ToLower().Trim();
            var address = request.Address.ToLower().Trim();

            if (await _context.Suppliers.AnyAsync(s => s.Name.ToLower().Trim() == name))
                throw new Exception("Tên nhà cung cấp đã tồn tại.");

            if (await _context.Suppliers.AnyAsync(s => s.Contact == request.Contact))
                throw new Exception("Số điện thoại đã tồn tại.");

            if (await _context.Suppliers.AnyAsync(s => s.Email.ToLower().Trim() == email))
                throw new Exception("Email đã tồn tại.");

            if (await _context.Suppliers.AnyAsync(s => s.Address.ToLower().Trim() == address))
                throw new Exception("Địa chỉ đã tồn tại.");

            var supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Contact = request.Contact,
                Email = request.Email,
                Address = request.Address
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return supplier.ToDto();
        }


        public async Task<bool> UpdateAsync(UpdateSupplierRequest request)
        {
            var supplier = await _context.Suppliers.FindAsync(request.Id);
            if (supplier == null) return false;

            var name = request.Name.ToLower().Trim();
            var email = request.Email.ToLower().Trim();
            var address = request.Address.ToLower().Trim();

            if (await _context.Suppliers.AnyAsync(s => s.Id != request.Id && s.Name.ToLower().Trim() == name))
                throw new Exception("Tên nhà cung cấp đã tồn tại.");

            if (await _context.Suppliers.AnyAsync(s => s.Id != request.Id && s.Contact == request.Contact))
                throw new Exception("Số điện thoại đã tồn tại.");

            if (await _context.Suppliers.AnyAsync(s => s.Id != request.Id && s.Email.ToLower().Trim() == email))
                throw new Exception("Email đã tồn tại.");

            if (await _context.Suppliers.AnyAsync(s => s.Id != request.Id && s.Address.ToLower().Trim() == address))
                throw new Exception("Địa chỉ đã tồn tại.");

            supplier.Name = request.Name;
            supplier.Contact = request.Contact;
            supplier.Email = request.Email;
            supplier.Address = request.Address;

            await _context.SaveChangesAsync();
            return true;
        }

        //public async Task<bool> UpdateAsync(UpdateSupplierRequest request)
        //{
        //    var supplier = await _context.Suppliers.FindAsync(request.Id);
        //    if (supplier == null) return false;

        //    supplier.Name = request.Name;
        //    supplier.Contact = request.Contact;
        //    supplier.Email = request.Email;
        //    supplier.Address = request.Address;

        //    await _context.SaveChangesAsync();
        //    return true;
        //}
    }

}
