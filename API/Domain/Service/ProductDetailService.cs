using API.Domain.DTOs;
using API.Domain.Extentions;
using API.Domain.Request.ProductDetailRequest;
using API.Domain.Request.VoucherRequest;
using API.Domain.Service.IService;
using ClosedXML.Excel;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Service
{
    public class ProductDetailService : IProductDetailService
    {
        private readonly DbContextApp _context;

        public ProductDetailService(DbContextApp context)
        {
            _context = context;
        }

        public async Task<ProductDetailDto> CreateAsync(CreateProductDetailRequest request)
        {
            // Validate: product must exist
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);
            if (product == null)
                throw new Exception("Sản phẩm cha không tồn tại.");

            // Tự động tạo tên theo format: "Tên sản phẩm cha - Code"
            var autoName = $"{product.Name} - {request.Code}";

            // Kiểm tra xem đã tồn tại chi tiết sản phẩm với tên giống rồi chưa
            var existingDetailByName = await _context.ProductDetails
                .FirstOrDefaultAsync(p => p.Name == autoName);

            if (existingDetailByName != null)
            {
                // Nếu có rồi thì cộng dồn số lượng
                existingDetailByName.Quantity += request.Quantity;
                await _context.SaveChangesAsync();

                // Load lại chi tiết sản phẩm kèm liên kết
                var loadedDetail = await _context.ProductDetails
                    .Include(x => x.Product).ThenInclude(p => p.Brand)
                    .Include(x => x.Product).ThenInclude(p => p.Category)
                    .Include(x => x.Color)
                    .Include(x => x.Size)
                    .Include(x => x.Material)
                    .Include(x => x.Origin)
                    .Include(x => x.Supplier)
                    .Include(x => x.Images)
                    .FirstOrDefaultAsync(x => x.Id == existingDetailByName.Id);

                return loadedDetail!.ToDto();
            }

            // Kiểm tra trùng Code
            bool isCodeDuplicate = await _context.ProductDetails.AnyAsync(x => x.Code == request.Code);
            if (isCodeDuplicate)
                throw new Exception("Code chi tiết sản phẩm đã tồn tại.");

            // Tạo chi tiết sản phẩm mới
            var detail = new ProductDetail
            {
                Id = Guid.NewGuid(),
                Name = autoName,
                Code = request.Code,
                Price = request.Price,
                Quantity = request.Quantity,
                ProductId = request.ProductId,
                ColorId = request.ColorId,
                SizeId = request.SizeId,
                MaterialId = request.MaterialId,
                OriginId = request.OriginId,
                SupplierId = request.SupplierId,
                Status = request.Status,
            };
            UpdateStatusByQuantity(detail);
            _context.ProductDetails.Add(detail);
            await _context.SaveChangesAsync();

            // Load lại chi tiết mới tạo
            var newLoadedDetail = await _context.ProductDetails
                .Include(x => x.Product).ThenInclude(p => p.Brand)
                .Include(x => x.Product).ThenInclude(p => p.Category)
                .Include(x => x.Color)
                .Include(x => x.Size)
                .Include(x => x.Material)
                .Include(x => x.Origin)
                .Include(x => x.Supplier)
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == detail.Id);

            return newLoadedDetail!.ToDto();
        }




        public async Task<ProductDetailDto> UpdateAsync(UpdateProductDetailRequest request)
        {
            // Lấy ProductDetail bao gồm thông tin Product
            var detail = await _context.ProductDetails
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.Id == request.Id);

            if (detail == null)
                throw new Exception("Chi tiết sản phẩm không tồn tại.");

            // Kiểm tra trùng mã (nếu thay đổi)
            if (detail.Code != request.Code)
            {
                bool isCodeDuplicate = await _context.ProductDetails
                    .AnyAsync(x => x.Id != detail.Id && x.Code == request.Code);
                if (isCodeDuplicate)
                    throw new Exception("Mã chi tiết sản phẩm đã tồn tại.");
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == detail.ProductId);
            if (product == null)
                throw new Exception("Sản phẩm chính không tồn tại.");

            // Kiểm tra trùng hoàn toàn để gộp
            var duplicate = await _context.ProductDetails
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.Id != detail.Id &&
                    x.ProductId == detail.ProductId &&
                    x.ColorId == request.ColorId &&
                    x.SizeId == request.SizeId &&
                    x.MaterialId == request.MaterialId &&
                    x.OriginId == request.OriginId &&
                    x.SupplierId == request.SupplierId &&
                    x.Product.BrandId == product.BrandId &&
                    x.Product.CategoryId == product.CategoryId
                );

            if (duplicate != null)
            {
                duplicate.Quantity += request.Quantity;
                _context.ProductDetails.Remove(detail);
                await _context.SaveChangesAsync();

                var merged = await _context.ProductDetails
                    .Include(x => x.Product).ThenInclude(p => p.Brand)
                    .Include(x => x.Product).ThenInclude(p => p.Category)
                    .Include(x => x.Color)
                    .Include(x => x.Size)
                    .Include(x => x.Material)
                    .Include(x => x.Origin)
                    .Include(x => x.Supplier)
                    .Include(x => x.Images)
                    .FirstOrDefaultAsync(x => x.Id == duplicate.Id);

                return merged!.ToDto();
            }

            // Nếu không gộp -> update như bình thường
            detail.Code = request.Code;
            detail.Price = request.Price;
            detail.Quantity = request.Quantity;
            detail.ColorId = request.ColorId;
            detail.SizeId = request.SizeId;
            detail.MaterialId = request.MaterialId;
            detail.OriginId = request.OriginId;
            detail.SupplierId = request.SupplierId;
            detail.Status = request.Status;
            detail.Name = $"{product.Name} - {request.Code}";
            UpdateStatusByQuantity(detail);
            // 🔹 Cập nhật PriceAfterDiscount trong các PromotionProduct liên quan
            var relatedPromotions = await _context.PromotionProducts
                .Include(pp => pp.Promotion)
                .Where(pp => pp.ProductDetailId == detail.Id)
                .ToListAsync();

            foreach (var pp in relatedPromotions)
            {
                // Chỉ cập nhật nếu Promotion còn hiệu lực
                if (pp.Promotion != null &&
                    pp.Promotion.Status == VoucherStatus.Active &&
                    pp.Promotion.StartDate <= DateTime.Now &&
                    pp.Promotion.EndDate >= DateTime.Now)
                {
                    pp.Pricebeforereduction = detail.Price;
                    pp.Priceafterduction = CalculatePriceAfterDiscount(
                        detail.Price,
                        pp.Promotion.DiscountType,
                        pp.Promotion.DiscountValue ?? 0
                    );
                }
            }

            await _context.SaveChangesAsync();
            return detail.ToDto();
        }

        /// <summary>
        /// Tính giá sau giảm
        /// </summary>
        private decimal CalculatePriceAfterDiscount(decimal originalPrice, DiscountType discountType, decimal discountValue)
        {
            return discountType switch
            {
                DiscountType.Percentage => Math.Max(0, originalPrice * (1 - discountValue / 100)),
                DiscountType.FixedAmount => Math.Max(0, originalPrice - discountValue),
                _ => originalPrice
            };
        }




        public async Task<List<ProductDetailDto>> GetAllAsync(Guid? productId = null)
        {
            var query = _context.ProductDetails
                .Include(p => p.Product).ThenInclude(p => p.Category)
                .Include(p => p.Product).ThenInclude(p => p.Brand)
                .Include(p => p.Color)
                .Include(p => p.Size)
                .Include(p => p.Material)
                .Include(p => p.Origin)
                .Include(p => p.Supplier)
                .Include(p => p.Images)
                .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(p => p.ProductId == productId.Value);
            }

            // 🔹 Sắp xếp theo trạng thái: Active -> Inactive -> OutOfStock
            query = query.OrderBy(p => p.Status);

            var list = await query.ToListAsync();
            return list.Select(p => p.ToDto()).ToList();
        }


        public async Task<ProductDetailDto?> GetByIdAsync(Guid id)
        {
            var p = await _context.ProductDetails
                .Include(x => x.Product).ThenInclude(p => p.Brand)
                .Include(x => x.Product).ThenInclude(p => p.Category)
                .Include(x => x.Color)
                .Include(x => x.Size)
                .Include(x => x.Material)
                .Include(x => x.Origin)
                .Include(x => x.Supplier)
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return null;

            return p.ToDto(); // <-- dùng extension method, sẽ có Images đầy đủ
        }

        public async Task<bool> ChangeStatusAsync(ChangeStatusRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Yêu cầu không được null.");

            if (string.IsNullOrWhiteSpace(request.Status))
                throw new ArgumentException("Trạng thái không được để trống.", nameof(request.Status));

            var product = await _context.ProductDetails.FindAsync(request.Id);
            if (product == null)
                throw new Exception($"Không tìm thấy sản phẩm với Id = {request.Id}");

            if (!Enum.TryParse<ProductDetailStatus>(request.Status, true, out var newStatus))
                throw new Exception($"Trạng thái không hợp lệ: {request.Status}");

            // 🔹 Kiểm tra số lượng > 0 thì không cho phép chuyển sang OutOfStock
            if (newStatus == ProductDetailStatus.OutOfStock && product.Quantity > 0)
                throw new Exception("Không thể chuyển sang trạng thái 'Hết hàng' khi số lượng sản phẩm vẫn còn.");

            product.Status = newStatus;

            _context.ProductDetails.Update(product);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> BulkChangeStatusAsync(BulkStatusChangeRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Yêu cầu không được null.");

            if (request.Ids == null || !request.Ids.Any())
                throw new ArgumentException("Danh sách Id sản phẩm không được để trống.", nameof(request.Ids));

            if (string.IsNullOrWhiteSpace(request.Status))
                throw new ArgumentException("Trạng thái không được để trống.", nameof(request.Status));

            if (!Enum.TryParse<ProductDetailStatus>(request.Status, true, out var newStatus))
                throw new Exception($"Trạng thái không hợp lệ: {request.Status}");

            var products = await _context.ProductDetails
                .Where(p => request.Ids.Contains(p.Id))
                .ToListAsync();

            if (!products.Any())
                throw new Exception("Không tìm thấy sản phẩm nào phù hợp với danh sách Id đã cung cấp.");

            // 🔹 Kiểm tra tất cả sản phẩm
            if (newStatus == ProductDetailStatus.OutOfStock && products.Any(p => p.Quantity > 0))
                throw new Exception("Không thể chuyển sang trạng thái 'Hết hàng' cho sản phẩm có số lượng > 0.");

            foreach (var p in products)
            {
                p.Status = newStatus;
            }

            _context.ProductDetails.UpdateRange(products);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ProductDetailDto>> GetByIdsAsync(List<Guid> ids)
        {
            var products = await _context.ProductDetails
                .Include(p => p.Images)
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            return products.Select(p => p.ToDto()).ToList(); // <-- dùng ToDto()
        }

        public async Task UpdateProductQuantityAfterOrderAsync(List<OrderDetail> orderDetails)
        {
            // Bước 1: Gộp các sản phẩm giống nhau theo ProductDetailId
            var groupedDetails = orderDetails
                .GroupBy(d => d.ProductDetailId)
                .Select(g => new
                {
                    ProductDetailId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            // Bước 2: Lấy danh sách ProductDetail từ DB
            var productDetailIds = groupedDetails
                .Where(g => g.ProductDetailId.HasValue)
                .Select(g => g.ProductDetailId.Value)
                .ToList();

            var productDetails = await _context.ProductDetails
                .Where(p => productDetailIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // Bước 3: Cập nhật số lượng tồn kho
            foreach (var item in groupedDetails)
            {
                if (!item.ProductDetailId.HasValue)
                    throw new Exception("ProductDetailId không hợp lệ (null).");

                if (!productDetails.TryGetValue(item.ProductDetailId.Value, out var productDetail))
                    throw new Exception($"Không tìm thấy sản phẩm với ID: {item.ProductDetailId}");

                if (productDetail.Quantity < item.TotalQuantity)
                    throw new Exception($"Sản phẩm {productDetail.Id} không đủ hàng trong kho.");

                productDetail.Quantity -= Convert.ToInt32(item.TotalQuantity);
                UpdateStatusByQuantity(productDetail);
            }

            // Bước 4: Lưu thay đổi
            await _context.SaveChangesAsync();
        }
        public async Task<List<ProductDetailDto>> GetAllWithDisplayPriceAsync(Guid? productId = null)
        {
            var query = _context.ProductDetails
                .Include(p => p.Color)
                .Include(p => p.Size)
                .Include(p => p.Material)
                .Include(p => p.Origin)
                .Include(p => p.Supplier)
                .Include(p => p.PromotionProducts)
                    .ThenInclude(pp => pp.Promotion)
                .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(p => p.ProductId == productId.Value);
            }

            var list = await query.ToListAsync();

            return list.Select(p =>
            {
                // 🔍 Tìm khuyến mãi hợp lệ: Promotion active + PromotionProduct active + thời gian hợp lệ
                var activePromotion = p.PromotionProducts?
                    .FirstOrDefault(pp =>
                        //pp.IsActive && // PromotionProduct active
                        pp.Promotion != null &&
                        pp.Promotion.Status == VoucherStatus.Active && // Promotion active
                        pp.Promotion.StartDate <= DateTime.Now &&
                        pp.Promotion.EndDate >= DateTime.Now
                    );

                var displayPrice = activePromotion != null
                    ? activePromotion.Priceafterduction
                    : p.Price;

                var dto = p.ToDto();
                dto.DisplayPrice = displayPrice;

                return dto;
            }).ToList();
        }
        /// <summary>
        /// Lấy danh sách ProductDetail chưa nằm trong bất kỳ Promotion nào
        /// </summary>
        /// <param name="promotionIdToExclude">ID promotion hiện tại nếu muốn loại bỏ chính promotion đó (ví dụ khi edit)</param>
        public async Task<List<ProductDetailDto>> GetAvailableForPromotionAsync(Guid? promotionIdToExclude = null)
        {
            var now = DateTime.Now;

            // Lấy tất cả ProductDetail đang được gán Promotion active
            var usedProductIdsQuery = _context.PromotionProducts
                .Include(pp => pp.Promotion)
                .Where(pp => pp.Promotion != null &&
                             pp.Promotion.Status != VoucherStatus.Expired); // Chỉ loại trừ những Promotion chưa Expired

            if (promotionIdToExclude.HasValue)
            {
                // Nếu edit, loại bỏ chính promotion hiện tại
                usedProductIdsQuery = usedProductIdsQuery.Where(pp => pp.PromotionId != promotionIdToExclude.Value);
            }

            var usedProductIds = await usedProductIdsQuery
                .Select(pp => pp.ProductDetailId)
                .Distinct()
                .ToListAsync();

            // Lấy ProductDetail chưa thuộc promotion nào
            var availableProducts = await _context.ProductDetails
                .Include(p => p.Product)
                .Include(p => p.Color)
                .Include(p => p.Size)
                .Include(p => p.Material)
                .Include(p => p.Origin)
                .Include(p => p.Supplier)
                .Include(p => p.Images)
                .Where(p => !usedProductIds.Contains(p.Id))
                .ToListAsync();

            return availableProducts.Select(p => p.ToDto()).ToList();
        }
        public async Task<List<ProductDetailDto>> GetByPromotionIdAsync(Guid promotionId)
        {
            if (promotionId == Guid.Empty)
                throw new ArgumentException("PromotionId không hợp lệ.", nameof(promotionId));

            // Lấy danh sách ProductDetail được gán cho Promotion
            var productDetails = await _context.PromotionProducts
                .Include(pp => pp.ProductDetail)
                    .ThenInclude(pd => pd.Product)
                .Include(pp => pp.ProductDetail)
                    .ThenInclude(pd => pd.Color)
                .Include(pp => pp.ProductDetail)
                    .ThenInclude(pd => pd.Size)
                .Include(pp => pp.ProductDetail)
                    .ThenInclude(pd => pd.Material)
                .Include(pp => pp.ProductDetail)
                    .ThenInclude(pd => pd.Origin)
                .Include(pp => pp.ProductDetail)
                    .ThenInclude(pd => pd.Supplier)
                .Include(pp => pp.ProductDetail)
                    .ThenInclude(pd => pd.Images)
                .Where(pp => pp.PromotionId == promotionId)
                .Select(pp => pp.ProductDetail!)
                .ToListAsync();

            return productDetails.Select(pd => pd.ToDto()).ToList();
        }
        public async Task<string> ImportProductDetailFromExcelAsync(string filePath, Guid productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
                throw new KeyNotFoundException($"ProductId {productId} không tồn tại.");

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.First();

            var rows = worksheet.RowsUsed().Skip(1);
            var allErrors = new List<string>();
            int rowIndex = 2;

            foreach (var row in rows)
            {
                try
                {
                    var codeCell = row.Cell(1).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(codeCell))
                    {
                        allErrors.Add($"Dòng {rowIndex}: Code không được để trống.");
                        rowIndex++;
                        continue;
                    }

                    if (!decimal.TryParse(row.Cell(2).GetString(), out var price) || price < 0)
                    {
                        allErrors.Add($"Dòng {rowIndex}: Price không hợp lệ.");
                        rowIndex++;
                        continue;
                    }

                    if (!int.TryParse(row.Cell(3).GetString(), out var quantity) || quantity < 0)
                    {
                        allErrors.Add($"Dòng {rowIndex}: Quantity không hợp lệ.");
                        rowIndex++;
                        continue;
                    }

                    var existingDetail = await _context.ProductDetails
                        .FirstOrDefaultAsync(p => p.ProductId == productId && p.Code == codeCell);

                    if (existingDetail != null)
                    {
                        existingDetail.Quantity += quantity; // cộng dồn
                        await _context.SaveChangesAsync();
                        rowIndex++;
                        continue;
                    }

                    var detail = new ProductDetail
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        Code = codeCell,
                        Name = $"{product.Name} - {codeCell}",
                        Price = price,
                        Quantity = quantity,
                        Status = ProductDetailStatus.Active
                    };
                    UpdateStatusByQuantity(detail);
                    _context.ProductDetails.Add(detail);
                    await _context.SaveChangesAsync();
                    rowIndex++;
                }
                catch (Exception ex)
                {
                    allErrors.Add($"Dòng {rowIndex}: Lỗi '{ex.Message}'");
                    rowIndex++;
                }
            }

            if (allErrors.Any())
                return "Hoàn tất nhưng có lỗi:\n" + string.Join("\n", allErrors);

            return "Import thành công.";
        }
        /// <summary>
        /// Tự động cập nhật trạng thái dựa trên số lượng tồn kho
        /// </summary>
        private void UpdateStatusByQuantity(ProductDetail detail)
        {
            if (detail.Quantity <= 0)
            {
                detail.Status = ProductDetailStatus.OutOfStock;
            }
            else if (detail.Status == ProductDetailStatus.OutOfStock)
            {
                // Nếu số lượng > 0 mà đang "Hết hàng" thì trả về trạng thái Active
                detail.Status = ProductDetailStatus.Active;
            }
        }

    }
}
