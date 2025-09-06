using API.Domain.DTOs;
using API.Domain.Extentions;
using API.Domain.Request.PromotionRequest;
using API.Domain.Request.VoucherRequest;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace API.Domain.Service
{
    public class PromotionService : IPromotionService
    {
        private readonly DbContextApp _context;
        private readonly IWebHostEnvironment _env;
        private readonly IStringLocalizer<PromotionService> _localizer;

        public PromotionService(DbContextApp context, IWebHostEnvironment env, IStringLocalizer<PromotionService> localizer)
        {
            _context = context;
            _env = env;
            _localizer = localizer;
        }

        //// Hàm tính giá sau giảm theo loại giảm giá
        //private decimal CalculatePriceAfterDiscount(decimal originalPrice, DiscountType discountType, decimal discountValue)
        //{
        //    if (discountType == DiscountType.Percentage)
        //    {
        //        return originalPrice * (1 - discountValue / 100);
        //    }
        //    else if (discountType == DiscountType.FixedAmount)
        //    {
        //        var price = originalPrice - discountValue;
        //        return price > 0 ? price : 0;
        //    }
        //    return originalPrice;
        //}


        public async Task<PromotionDto> CreateAsync(CreatePromotionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            ValidatePromotionData(request.StartDate, request.EndDate, request.DiscountValue, request.DiscountType, request.Status, request.Name, request.Description);

            var promotionId = Guid.NewGuid();

            var promotion = new Promotion
            {
                Id = promotionId,
                Name = request.Name,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status,
                ImageUrl = await SaveImageAsync(request.ImageFile, request.ImageUrl, promotionId)
            };

            await _context.Promotions.AddAsync(promotion);
            await _context.SaveChangesAsync();

            var validProductIds = new List<Guid>();
            var errors = new List<string>();

            if (request.ProductDetailIds?.Any() == true)
            {
                foreach (var productId in request.ProductDetailIds.Distinct())
                {
                    var isUsed = await _context.PromotionProducts
                        .Include(pp => pp.Promotion)
                        .AnyAsync(pp => pp.ProductDetailId == productId
                                        && pp.Promotion.Status != VoucherStatus.Expired);


                    if (isUsed)
                    {
                        errors.Add(_localizer[$"Sản phẩm với ID {productId} đã nằm trong khuyến mãi khác."]);
                        continue;
                    }

                    var product = await _context.ProductDetails.FindAsync(productId);
                    if (product == null)
                    {
                        errors.Add(_localizer[$"Sản phẩm với ID {productId} không tồn tại."]);
                        continue;
                    }

                    var priceAfter = CalculatePriceAfterDiscount(product.Price, request.DiscountType, request.DiscountValue);

                    var promotionProduct = new PromotionProduct
                    {
                        Id = Guid.NewGuid(),
                        PromotionId = promotion.Id,
                        ProductDetailId = productId,
                        Priceafterduction = priceAfter,
                        Pricebeforereduction = product.Price,
                        CreatedAt = DateTime.Now,
                        //IsActive = true
                    };

                    await _context.PromotionProducts.AddAsync(promotionProduct);
                    validProductIds.Add(productId);
                }

                if (errors.Any())
                    throw new Exception(string.Join(" | ", errors));

                await _context.SaveChangesAsync();
            }

            var productNames = await _context.ProductDetails
                .Where(p => validProductIds.Contains(p.Id))
                .Select(p => p.Name)
                .ToListAsync();

            return new PromotionDto
            {
                Id = promotion.Id,
                Name = promotion.Name!,
                DiscountType = promotion.DiscountType.ToString(),
                DiscountValue = promotion.DiscountValue ?? 0,
                Description = promotion.Description,
                StartDate = promotion.StartDate ?? DateTime.Now,
                EndDate = promotion.EndDate ?? DateTime.Now,
                Status = promotion.Status.ToString(),
                ImageUrl = promotion.ImageUrl,
                ProductDetailIds = validProductIds,
                ProductNames = productNames
            };
        }

        public async Task<PromotionDto> UpdateAsync(UpdatePromotionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var promotion = await _context.Promotions.FindAsync(request.Id);
            if (promotion == null)
                throw new Exception(_localizer["Không tìm thấy chương trình khuyến mãi."]);

            var validProductIds = new List<Guid>();
            var originalStartDate = promotion.StartDate;

            // Nếu đổi ngày bắt đầu và nằm trong quá khứ -> báo lỗi
            if (request.StartDate != originalStartDate && request.StartDate < DateTime.Now)
                throw new Exception(_localizer["Ngày bắt đầu không được ở quá khứ."]);

            // Xác định có bỏ qua check ngày bắt đầu quá khứ hay không
            bool ignoreStartDatePastCheck = request.StartDate == originalStartDate;

            // Validate dữ liệu (bỏ qua check ngày bắt đầu quá khứ nếu giữ nguyên)
            ValidatePromotionData(
                request.StartDate,
                request.EndDate,
                request.DiscountValue,
                request.DiscountType,
                request.Status,
                request.Name,
                request.Description,
                ignoreStartDatePastCheck
            );

            // Cập nhật các trường cơ bản
            promotion.Name = request.Name;
            promotion.Description = request.Description;
            promotion.StartDate = request.StartDate;
            promotion.EndDate = request.EndDate;
            promotion.DiscountValue = request.DiscountValue;
            promotion.DiscountType = request.DiscountType;
            promotion.Status = request.Status;
            promotion.ImageUrl = await SaveImageAsync(request.ImageFile, request.ImageUrl, request.Id, true);

            _context.Promotions.Update(promotion);

            // Xóa PromotionProducts cũ
            var oldPromotionProducts = _context.PromotionProducts.Where(pp => pp.PromotionId == promotion.Id);
            _context.PromotionProducts.RemoveRange(oldPromotionProducts);

            // Thêm PromotionProducts mới nếu có
            if (request.ProductDetailIds?.Any() == true)
            {
                var errors = new List<string>();

                foreach (var productId in request.ProductDetailIds.Distinct())
                {
                    // Kiểm tra sản phẩm có nằm trong Promotion khác (Active/Inactive) hay không
                    var isUsed = await _context.PromotionProducts
                        .Include(pp => pp.Promotion)
                        .AnyAsync(pp => pp.ProductDetailId == productId
                                        && pp.PromotionId != promotion.Id
                                        && pp.Promotion.Status != VoucherStatus.Expired);

                    if (isUsed)
                    {
                        errors.Add(_localizer[$"Sản phẩm với ID {productId} đã nằm trong khuyến mãi khác."]);
                        continue;
                    }

                    var product = await _context.ProductDetails.FindAsync(productId);
                    if (product == null)
                    {
                        errors.Add(_localizer[$"Sản phẩm với ID {productId} không tồn tại."]);
                        continue;
                    }

                    var priceAfter = CalculatePriceAfterDiscount(product.Price, request.DiscountType, request.DiscountValue);

                    var promotionProduct = new PromotionProduct
                    {
                        Id = Guid.NewGuid(),
                        PromotionId = promotion.Id,
                        ProductDetailId = productId,
                        Pricebeforereduction = product.Price,
                        Priceafterduction = priceAfter,
                        CreatedAt = DateTime.Now
                    };

                    _context.PromotionProducts.Add(promotionProduct);
                    validProductIds.Add(productId);
                }

                if (errors.Any())
                    throw new Exception(string.Join(" | ", errors));
            }

            await _context.SaveChangesAsync();

            var productNames = await _context.ProductDetails
                .Where(p => validProductIds.Contains(p.Id))
                .Select(p => p.Name)
                .ToListAsync();

            return new PromotionDto
            {
                Id = promotion.Id,
                Name = promotion.Name!,
                DiscountType = promotion.DiscountType.ToString(),
                DiscountValue = promotion.DiscountValue ?? 0,
                Description = promotion.Description,
                StartDate = promotion.StartDate ?? DateTime.Now,
                EndDate = promotion.EndDate ?? DateTime.Now,
                Status = promotion.Status.ToString(),
                ImageUrl = promotion.ImageUrl,
                ProductDetailIds = validProductIds,
                ProductNames = productNames,
                ProductCount = validProductIds.Count
            };
        }




        private decimal CalculatePriceAfterDiscount(decimal originalPrice, DiscountType discountType, decimal discountValue)
        {
            return discountType switch
            {
                DiscountType.Percentage => Math.Max(0, originalPrice * (1 - discountValue / 100)),
                DiscountType.FixedAmount => Math.Max(0, originalPrice - discountValue),
                _ => originalPrice
            };
        }

        private void ValidatePromotionData(
    DateTime startDate,
    DateTime? endDate,
    decimal? discountValue,
    DiscountType discountType,
    VoucherStatus status,
    string name,
    string description,
    bool ignoreStartDatePastCheck = false)  // thêm cờ
        {
            if (!ignoreStartDatePastCheck && startDate < DateTime.Now.Date)
                throw new Exception("Ngày bắt đầu không được ở quá khứ.");

            if (endDate.HasValue && endDate < startDate)
                throw new Exception("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

            if (discountType == DiscountType.Percentage && (discountValue <= 0 || discountValue > 100))
                throw new Exception("Giá trị giảm giá phần trăm phải từ 1 đến 100.");
            if (discountType == DiscountType.FixedAmount && discountValue <= 0)
                throw new Exception("Giá trị giảm giá cố định phải lớn hơn 0.");

            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Tên chương trình khuyến mãi không được để trống.");

            if (string.IsNullOrWhiteSpace(description))
                throw new Exception("Mô tả chương trình khuyến mãi không được để trống.");
        }


        private async Task<string> SaveImageAsync(IFormFile? file, string? imageUrl, Guid id, bool isUpdate = false)
        {
            if (!string.IsNullOrEmpty(imageUrl))
                return imageUrl;

            if (file == null || file.Length == 0)
                throw new Exception(_localizer["Ảnh không hợp lệ."]);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                throw new Exception(_localizer["Định dạng ảnh không hợp lệ. Chỉ chấp nhận jpg, jpeg, png, gif, webp."]);

            if (file.Length > 5 * 1024 * 1024)
                throw new Exception(_localizer["Kích thước ảnh vượt quá giới hạn cho phép (tối đa 5MB)."]);

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(webRoot, "uploads", "promotions");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"{id}{extension}";
            string filePath = Path.Combine(folderPath, fileName);

            if (isUpdate && File.Exists(filePath))
                File.Delete(filePath);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/promotions/{fileName}";
        }

        /// <summary>
        /// Auto update promotion statuses based on dates - only applies to Active and Inactive statuses
        /// </summary>
        public async Task UpdatePromotionStatusesAsync()
        {
            var now = DateTime.Now;

            // Chỉ lấy các promotion có status là Active hoặc Inactive để auto update
            var promotions = await _context.Promotions
                .Where(p => p.Status == VoucherStatus.Active || p.Status == VoucherStatus.Inactive)
                .ToListAsync();

            foreach (var p in promotions)
            {
                // Chỉ auto update trạng thái cho Active và Inactive
                if (p.Status == VoucherStatus.Active || p.Status == VoucherStatus.Inactive)
                {
                    if (p.EndDate < now)
                        p.Status = VoucherStatus.Expired;
                    else if (p.StartDate > now)
                        p.Status = VoucherStatus.Inactive;
                    else if (p.StartDate <= now && p.EndDate >= now)
                        p.Status = VoucherStatus.Active;
                }
                // Expired status sẽ không bị thay đổi
            }

            if (promotions.Any())
            {
                _context.Promotions.UpdateRange(promotions);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Helper method to auto update status for a single promotion
        /// </summary>
        private void AutoUpdatePromotionStatus(Promotion promotion)
        {
            var now = DateTime.Now;

            // Chỉ auto update nếu status hiện tại là Active hoặc Inactive
            if (promotion.Status == VoucherStatus.Active || promotion.Status == VoucherStatus.Inactive)
            {
                if (promotion.EndDate < now)
                {
                    promotion.Status = VoucherStatus.Expired;
                }
                else if (promotion.StartDate > now)
                {
                    promotion.Status = VoucherStatus.Inactive;
                }
                else if (promotion.StartDate <= now && promotion.EndDate >= now)
                {
                    promotion.Status = VoucherStatus.Active;
                }
            }
            // Nếu status là Expired thì không thay đổi gì
        }

        public async Task<List<PromotionDto>> GetAllAsync()
        {
            var promotions = await _context.Promotions.ToListAsync();

            var promotionIds = promotions.Select(p => p.Id).ToList();

            // Auto update status cho từng promotion
            foreach (var p in promotions)
            {
                AutoUpdatePromotionStatus(p);
            }

            _context.Promotions.UpdateRange(promotions);
            await _context.SaveChangesAsync();

            var promotionProducts = await _context.PromotionProducts
                .Where(pp => promotionIds.Contains(pp.PromotionId))
                .Include(pp => pp.ProductDetail)
                .ThenInclude(pd => pd.Product)
                .ToListAsync();

            var dtoList = promotions.Select(p =>
            {
                var productDetails = promotionProducts
                    .Where(pp => pp.PromotionId == p.Id)
                    .Select(pp => pp.ProductDetail)
                    .ToList();

                var productNames = productDetails
                    .Select(pd => pd?.Name ?? "Không biết")
                    .ToList();

                return new PromotionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    DiscountType = p.DiscountType.ToString(),
                    DiscountValue = p.DiscountValue ?? 0,
                    Description = p.Description,
                    StartDate = p.StartDate ?? DateTime.Now,
                    EndDate = p.EndDate ?? DateTime.Now,
                    Status = p.Status.ToString(),
                    ImageUrl = p.ImageUrl,
                    ProductNames = productNames,
                    ProductDetailIds = productDetails.Select(d => d?.Id ?? Guid.Empty).ToList(),
                    ProductCount = productDetails.Count
                };
            });

            // 👉 Thêm phần sắp xếp ở đây
            var sortedList = dtoList
                .OrderBy(d => Enum.TryParse<VoucherStatus>(d.Status, out var status) ? status : VoucherStatus.Inactive)
                .ThenBy(d => d.StartDate)
                .ThenBy(d => Enum.TryParse<DiscountType>(d.DiscountType, out var type) ? type : DiscountType.Percentage)
                .ToList();

            return sortedList;
        }


        public async Task<PromotionDto?> GetByIdAsync(Guid id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return null;

            // Auto update status
            AutoUpdatePromotionStatus(promotion);

            _context.Promotions.Update(promotion);
            await _context.SaveChangesAsync();

            var promotionProducts = await _context.PromotionProducts
                .Where(pp => pp.PromotionId == id)
                .Include(pp => pp.ProductDetail)
                .ToListAsync();

            var productNames = promotionProducts
                .Select(pp => pp.ProductDetail?.Name ?? "Không biết")
                .ToList();

            var productDetailIds = promotionProducts
                .Select(pp => pp.ProductDetail?.Id ?? Guid.Empty)
                .ToList();

            return new PromotionDto
            {
                Id = promotion.Id,
                Name = promotion.Name,
                DiscountType = promotion.DiscountType.ToString(),
                DiscountValue = promotion.DiscountValue ?? 0,
                Description = promotion.Description,
                StartDate = promotion.StartDate ?? DateTime.Now,
                EndDate = promotion.EndDate ?? DateTime.Now,
                Status = promotion.Status.ToString(),
                ImageUrl = promotion.ImageUrl,
                ProductNames = productNames,
                ProductDetailIds = productDetailIds
            };
        }

        public async Task<PromotionDetailDto> GetDetailByIdAsync(Guid id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return null!;

            // Auto update status
            AutoUpdatePromotionStatus(promotion);

            _context.Promotions.Update(promotion);
            await _context.SaveChangesAsync();

            var promotionProducts = await _context.PromotionProducts
                .Where(pp => pp.PromotionId == id)
                .Select(pp => new PromotionProductDto
                {
                    Id = pp.Id,
                    ProductDetailId = pp.ProductDetailId,
                    PriceBeforeDiscount = pp.Pricebeforereduction,
                    PriceAfterDiscount = pp.Priceafterduction,
                })
                .ToListAsync();

            var promotionDto = new PromotionDto
            {
                Id = promotion.Id,
                Name = promotion.Name,
                DiscountType = promotion.DiscountType.ToString(),
                DiscountValue = promotion.DiscountValue ?? 0,
                Description = promotion.Description,
                StartDate = promotion.StartDate ?? DateTime.Now,
                EndDate = promotion.EndDate ?? DateTime.Now,
                Status = promotion.Status.ToString(),
                ImageUrl = promotion.ImageUrl,
                ProductDetailIds = promotionProducts.Select(x => x.ProductDetailId).ToList(),
                ProductCount = promotionProducts.Count
            };

            return new PromotionDetailDto
            {
                Promotion = promotionDto,
                SelectedProductIds = promotionDto.ProductDetailIds ?? new(),
                PromotionProducts = promotionProducts
            };
        }


        public async Task<List<ProductDetailDto>> GetAllProductsAsync()
        {
            var products = await _context.ProductDetails
                .Include(pd => pd.Images)
                .Include(pd => pd.Product)
                .ToListAsync();

            var result = products.Select(p =>
            {
                var mainImage = p.Images.FirstOrDefault(img => img.IsMainImage);

                return new ProductDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Quantity = p.Quantity ?? 0,
                    Price = p.Price,
                    MainImageUrl = mainImage?.Url,
                    ProductId = p.ProductId,
                    ProductName = p.Name ?? string.Empty
                };
            }).ToList();

            return result;
        }

        public async Task ChangePromotionStatusAsync(Guid promotionId, VoucherStatus newStatus)
        {
            var promotion = await _context.Promotions
                .Include(p => p.PromotionProducts)
                .FirstOrDefaultAsync(p => p.Id == promotionId);

            if (promotion == null)
                throw new Exception("Không tìm thấy khuyến mãi.");

            // Nếu khuyến mãi đang Expired mà muốn đổi thành Active hoặc Inactive
            if (promotion.Status == VoucherStatus.Expired
                && (newStatus == VoucherStatus.Active || newStatus == VoucherStatus.Inactive))
            {
                var productIds = promotion.PromotionProducts.Select(pp => pp.ProductDetailId).ToList();

                var conflict = await _context.PromotionProducts
                    .Include(pp => pp.Promotion)
                    .AnyAsync(pp => productIds.Contains(pp.ProductDetailId)
                                    && pp.PromotionId != promotion.Id
                                    && (pp.Promotion.Status == VoucherStatus.Active
                                        || pp.Promotion.Status == VoucherStatus.Inactive));

                if (conflict)
                    throw new Exception("Không thể đổi trạng thái khuyến mãi ngừng áp dụng thành Áp dụng/Chưa áp dụng vì sản phẩm đang thuộc khuyến mãi khác.");
            }

            promotion.Status = newStatus;
            _context.Promotions.Update(promotion);
            await _context.SaveChangesAsync();
        }


        public async Task<bool> BulkChangePromotionStatusAsync(BulkStatusChangeRequest request)
        {
            if (!Enum.TryParse<VoucherStatus>(request.Status, true, out var newStatus))
                throw new Exception(_localizer["Trạng thái không hợp lệ."]);

            var promotions = await _context.Promotions
                .Include(p => p.PromotionProducts)
                .Where(p => request.Ids.Contains(p.Id))
                .ToListAsync();

            if (!promotions.Any()) return false;

            var now = DateTime.Now;

            foreach (var promo in promotions)
            {
                // Rule check: Nếu promo hiện tại đã Expired và muốn đổi thành Active/Inactive
                if (promo.Status == VoucherStatus.Expired
                    && (newStatus == VoucherStatus.Active || newStatus == VoucherStatus.Inactive))
                {
                    var productIds = promo.PromotionProducts.Select(pp => pp.ProductDetailId).ToList();

                    var conflict = await _context.PromotionProducts
                        .Include(pp => pp.Promotion)
                        .AnyAsync(pp => productIds.Contains(pp.ProductDetailId)
                                        && pp.PromotionId != promo.Id
                                        && (pp.Promotion.Status == VoucherStatus.Active
                                            || pp.Promotion.Status == VoucherStatus.Inactive));

                    if (conflict)
                        throw new Exception(_localizer[
                            $"Không thể đổi trạng thái khuyến mãi [{promo.Name}] vì sản phẩm của nó đang thuộc khuyến mãi khác (Áp dụng/Chưa áp dụng)."]);
                }
                // Nếu khuyến mãi đã hết hạn, cập nhật lại StartDate/EndDate để có thể kích hoạt lại
                if (promo.EndDate < now)
                {
                    promo.StartDate = now.Date;
                    promo.EndDate = now.Date.AddDays(1);
                }

                promo.Status = newStatus;
            }

            _context.Promotions.UpdateRange(promotions);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}