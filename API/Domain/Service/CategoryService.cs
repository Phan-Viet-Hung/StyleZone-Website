using API.Domain.DTOs;
using API.Domain.Request.CategoryRequest;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly DbContextApp _context;

        public CategoryService(DbContextApp context)
        {
            _context = context;
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == request.Name))
                throw new Exception("Tên danh mục đã tồn tại.");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.Now
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                QuantityProduct = 0,
                CreatedAt = category.CreatedAt
            };
        }


        public async Task<CategoryDto> UpdateAsync(UpdateCategoryRequest request)
        {
            var category = await _context.Categories.FindAsync(request.Id);
            if (category == null)
                throw new Exception("Danh mục không tồn tại.");

            if (await _context.Categories.AnyAsync(c => c.Name == request.Name && c.Id != request.Id))
                throw new Exception("Tên danh mục đã được sử dụng.");

            category.Name = request.Name;
            category.Description = request.Description;
            category.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            return await _context.Categories.OrderBy(p=>p.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    QuantityProduct = _context.Products.Count(p => p.CategoryId == c.Id),
                    TotalQuantity = _context.ProductDetails
                        .Where(pd => _context.Products
                            .Where(p => p.CategoryId == c.Id)
                            .Select(p => p.Id)
                            .Contains(pd.ProductId))
                        .Sum(pd => (int?)pd.Quantity) ?? 0,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();
        }



        public async Task<CategoryDto?> GetByIdAsync(Guid id)
        {
            var c = await _context.Categories.FindAsync(id);
            if (c == null) return null;

            var quantity = await _context.Products.CountAsync(p => p.CategoryId == c.Id);

            var productIds = await _context.Products
                .Where(p => p.CategoryId == c.Id)
                .Select(p => p.Id)
                .ToListAsync();

            var totalQuantity = await _context.ProductDetails
                .Where(pd => productIds.Contains(pd.ProductId))
                .SumAsync(pd => (int?)pd.Quantity) ?? 0;

            return new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                QuantityProduct = quantity,
                TotalQuantity = totalQuantity,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }


    }
}
