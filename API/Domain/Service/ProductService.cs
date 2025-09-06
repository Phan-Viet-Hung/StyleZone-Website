using API.Domain.DTOs;
using API.Domain.Extentions;
using API.Domain.Request.ProductDetailRequest;
using API.Domain.Request.ProductRequest;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Service
{
    public class ProductService : IProductService
    {
        private readonly DbContextApp _context;
        public ProductService(DbContextApp context)
        {
            _context = context;
        }

        public async Task<List<ProductDto>> GetAllAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductDetails)
                .OrderByDescending(p => p.CreatedAt) // <- sắp xếp trước Select
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Gender = p.Gender,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    CreatedBy = p.CreatedBy,
                    UpdatedBy = p.UpdatedBy,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    BrandId = p.BrandId,
                    BrandName = p.Brand.Name,
                    TotalQuantity = p.ProductDetails.Sum(pd => (int?)pd.Quantity) ?? 0,
                    ProductDetails = p.ProductDetails.Select(pd => new ProductDetailDto
                    {
                        // ánh xạ thông tin cần thiết nếu cần
                    }).ToList()
                })
                .ToListAsync();

            return products;
        }


        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductDetails)
                    .ThenInclude(pd => pd.Color)
                .Include(p => p.ProductDetails)
                    .ThenInclude(pd => pd.Size)
                .Include(p => p.ProductDetails)
                    .ThenInclude(pd => pd.Material)
                .Include(p => p.ProductDetails)
                    .ThenInclude(pd => pd.Origin)
                .Include(p => p.ProductDetails)
                    .ThenInclude(pd => pd.Supplier)
                .Include(p => p.ProductDetails)
                    .ThenInclude(pd => pd.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return null;

            var dto = product.ToDto();

            // 🔹 Sắp xếp ProductDetails theo trạng thái: Active -> Inactive -> OutOfStock
            dto.ProductDetails = product.ProductDetails
                .OrderBy(pd => pd.Status) // enum sẽ sort theo 1,2,3
                .Select(pd => pd.ToDto())
                .ToList();

            dto.TotalQuantity = product.ProductDetails.Sum(pd => pd.Quantity ?? 0);
            return dto;
        }



        public async Task<ProductDto> CreateAsync(CreateProductRequest request, Guid userId)
        {
            // 🔹 Kiểm tra trùng tên trong cùng Category
            var exists = await _context.Products
                .AnyAsync(p => p.Name.ToLower() == request.Name.ToLower()
                            && p.CategoryId == request.CategoryId);

            if (exists)
                throw new Exception($"Đã tồn tại sản phẩm '{request.Name}' trong danh mục này.");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Gender = request.Gender,
                CreatedAt = DateTime.Now,
                CreatedBy = userId,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(product.Id))!;
        }


        public async Task<ProductDto> UpdateAsync(UpdateProductRequest request, Guid userId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id);
            if (product == null) throw new Exception("Không tìm thấy sản phẩm");

            // 🔹 Kiểm tra trùng tên trong cùng Category (ngoại trừ chính nó)
            var exists = await _context.Products
                .AnyAsync(p => p.Id != request.Id
                            && p.Name.ToLower() == request.Name.ToLower()
                            && p.CategoryId == request.CategoryId);

            if (exists)
                throw new Exception($"Đã tồn tại sản phẩm '{request.Name}' trong danh mục này.");

            product.Name = request.Name;
            product.Description = request.Description;
            product.Gender = request.Gender;
            product.CategoryId = request.CategoryId;
            product.BrandId = request.BrandId;
            product.UpdatedAt = DateTime.Now;
            product.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return (await GetByIdAsync(product.Id))!;
        }
    }
}
