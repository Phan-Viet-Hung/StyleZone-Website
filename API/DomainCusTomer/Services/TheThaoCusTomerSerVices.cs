
using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.DTOs.TheThao;
using API.DomainCusTomer.ExTentions;
using API.DomainCusTomer.Request.TheThao;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;


namespace API.DomainCusTomer.Services
{
    public class TheThaoCusTomerSerVices : ITheThaoCustomerServices
    {
        private readonly DbContextApp _context;

        public TheThaoCusTomerSerVices(DbContextApp context)
        {
            _context = context;
        }
        public async Task<PagedProductResponse> TheThao(ProductFilterRequest filter)
        {
            var allowedCategories = new[]  {  "thể thao", "pickleball", "chạy bộ", "tập luyện", "bóng rổ", "cầu lông", "golf","bóng đá","giày bóng đá", "giày chạy bộ", "giày cầu lông", "giày bóng rổ", "bộ quẩn áo bóng rổ", "bộ quần áo cầu lông"};

            var query = _context.ProductDetails
            .Include(p => p.Color)
            .Include(p => p.Size)
            .Include(p => p.Material)
            .Include(p => p.Origin)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p=>p.PromotionProducts)
            .ThenInclude(p=>p.Promotion)
            .Include(p => p.Product)
            .ThenInclude(p => p.Category)
            .Include(p => p.Product)
            .ThenInclude(p => p.Brand)
            .Where(p => p.Status == ProductDetailStatus.Active &&
                p.Product != null && p.Quantity >0 &&
                p.Product.Category != null &&
                allowedCategories.Contains(p.Product.Category.Name.Trim().ToLower()));

            // Lọc theo tên sản phẩm 
            if (filter.Product?.Any(p => !string.IsNullOrWhiteSpace(p)) == true)
            {
                var keywords = filter.Product
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim().ToLower())
                    .ToList();

                query = query.Where(p =>
                    keywords.Any(k => p.Product.Name.ToLower().Contains(k))
                );
            }

            // Lọc theo màu
            if (filter.Colors?.Any(c => !string.IsNullOrWhiteSpace(c)) == true)
            {
                var color = filter.Colors.Select(c => c.Trim().ToLower()).ToList();
                query = query.Where(p => color.Contains(p.Color.Name.ToLower()));
            }

            // Lọc theo size
            if (filter.Sizes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true)
            {
                var size = filter.Sizes.Select(s => s.Trim().ToLower()).ToList();
                query = query.Where(p => size.Contains(p.Size.Code.ToLower()));
            }

            // Lọc theo giới tính
            if (filter.Genders?.Any(g => !string.IsNullOrWhiteSpace(g)) == true)
            {
                // Chuyển list string từ filter.Genders sang list enum
                var genderEnums = filter.Genders
                    .Select(g =>
                    {
                        if (Enum.TryParse<GenderEnum>(g, true, out var result)) // true = ignoreCase
                            return (GenderEnum?)result;
                        return null;
                    })
                    .Where(e => e.HasValue)
                    .Select(e => e.Value)
                    .ToList();

                query = query.Where(p => genderEnums.Contains(p.Product.Gender));
            }


            // Thực hiện lấy danh sách sau khi áp dụng filter (chưa phân trang)
            var allFiltered = await query.ToListAsync();

            // Lấy thông tin bộ lọc (từ toàn bộ danh sách đã lọc)
            var categories = allFiltered.Select(p => p.Product.Name).OrderBy(name => name).Distinct().ToList();
            var sizes = allFiltered.Select(p => p.Size.Code).Distinct().ToList();
            var colors = allFiltered.Select(p => p.Color.Name).Distinct().ToList();
            var genders = allFiltered.Select(p => p.Product.Gender).Distinct().ToList();

            // Sắp xếp
            switch (filter.SortBy?.ToLower())
            {
                case "name":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.Name).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.Name).ToList();
                    break;

                case "price":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList()
                        : allFiltered.OrderByDescending(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList();
                    break;

                case "createdat":
                default:
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.CreatedAt).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.CreatedAt).ToList();
                    break;
            }

            // Phân trang
            var totalItems = allFiltered.Count;
            var skip = (filter.Page - 1) * filter.PageSize;
            var data = allFiltered.Skip(skip).Take(filter.PageSize).ToList();
            var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

            return new PagedProductResponse
            {
                Items = data.Select(p => p.ToCustomerDto()).ToList(),
                TotalPages = totalPages,
                CurrentPage = filter.Page,
                Categories = categories,
                Sizes = sizes,
                Colors = colors,
                Genders = genders
            };
        }

        public async Task<PagePickleball> GetALLPICKLEBALL(PickleballFilterRequest filter)
        {
            var allowedCategories = new[]
           { "pickleball" };

            var query = _context.ProductDetails .Include(p => p.Color) .Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier) .Include(p => p.Images).Include(p => p.PromotionProducts)
                 .ThenInclude(p => p.Promotion).Include(p => p.Product) .ThenInclude(p => p.Category)
                .Where(p => p.Product.Category != null  && p.Quantity > 0 && p.Status == ProductDetailStatus.Active && allowedCategories
                .Contains(p.Product.Category.Name.Trim().ToLower())).AsQueryable();

            // Lọc theo tên sản phẩm
            if (filter.Product?.Any(p => !string.IsNullOrWhiteSpace(p)) == true)
            {
                var keywords = filter.Product
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim().ToLower())
                    .ToList();

                query = query.Where(p =>
                    keywords.Any(k => p.Product.Name.ToLower().Contains(k))
                );
            }

            // Lọc theo màu
            if (filter.Colors?.Any(c => !string.IsNullOrWhiteSpace(c)) == true)
            {
                var color = filter.Colors.Select(c => c.Trim().ToLower()).ToList();
                query = query.Where(p => color.Contains(p.Color.Name.ToLower()));
            }

            // Lọc theo size
            if (filter.Sizes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true)
            {
                var size = filter.Sizes.Select(s => s.Trim().ToLower()).ToList();
                query = query.Where(p => size.Contains(p.Size.Code.ToLower()));
            }

            // Lọc theo giới tính
            if (filter.Genders?.Any(g => !string.IsNullOrWhiteSpace(g)) == true)
            {
                // Chuyển list string từ filter.Genders sang list enum
                var genderEnums = filter.Genders
                    .Select(g =>
                    {
                        if (Enum.TryParse<GenderEnum>(g, true, out var result)) // true = ignoreCase
                            return (GenderEnum?)result;
                        return null;
                    })
                    .Where(e => e.HasValue)
                    .Select(e => e.Value)
                    .ToList();

                query = query.Where(p => genderEnums.Contains(p.Product.Gender));
            }


            // Thực hiện lấy danh sách sau khi áp dụng filter (chưa phân trang)
            var allFiltered = await query.ToListAsync();

            // Lấy thông tin bộ lọc (từ toàn bộ danh sách đã lọc)
           var categories = allFiltered.Select(p => p.Product.Name).OrderBy(name => name).Distinct().ToList();
            var sizes = allFiltered.Select(p => p.Size.Code).Distinct().ToList();
            var colors = allFiltered.Select(p => p.Color.Name).Distinct().ToList();
            var genders = allFiltered.Select(p => p.Product.Gender).Distinct().ToList();

            // Sắp xếp
            switch (filter.SortBy?.ToLower())
            {
                case "name":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.Name).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.Name).ToList();
                    break;

                case "price":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList()
                        : allFiltered.OrderByDescending(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList();
                    break;

                case "createdat":
                default:
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.CreatedAt).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.CreatedAt).ToList();
                    break;
            }

            // Phân trang
            var totalItems = allFiltered.Count;
            var skip = (filter.Page - 1) * filter.PageSize;
            var data = allFiltered.Skip(skip).Take(filter.PageSize).ToList();
            var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

            return new PagePickleball
            {
                Items = data.Select(p => p.ToCustomerDto()).ToList(),
                TotalPages = totalPages,
                CurrentPage = filter.Page,
                Categories = categories,
                Sizes = sizes,
                Colors = colors,
                Genders = genders
            };
        }


        public async Task<PageBongDa> GetAllBongDa(BongDaFilterRequest filter)
        {
            var allowedCategories = new[]
           { "Bóng Đá" , "giày bóng đá"};

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && p.Status == ProductDetailStatus.Active && allowedCategories 
                .Contains(p.Product.Category.Name.Trim().ToLower())).AsQueryable();

            // Lọc theo tên sản phẩm
            if (filter.Product?.Any(p => !string.IsNullOrWhiteSpace(p)) == true)
            {
                var keywords = filter.Product
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim().ToLower())
                    .ToList();

                query = query.Where(p =>
                    keywords.Any(k => p.Product.Name.ToLower().Contains(k))
                );
            }

            // Lọc theo màu
            if (filter.Colors?.Any(c => !string.IsNullOrWhiteSpace(c)) == true)
            {
                var color = filter.Colors.Select(c => c.Trim().ToLower()).ToList();
                query = query.Where(p => color.Contains(p.Color.Name.ToLower()));
            }

            // Lọc theo size
            if (filter.Sizes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true)
            {
                var size = filter.Sizes.Select(s => s.Trim().ToLower()).ToList();
                query = query.Where(p => size.Contains(p.Size.Code.ToLower()));
            }

            // Lọc theo giới tính
            if (filter.Genders?.Any(g => !string.IsNullOrWhiteSpace(g)) == true)
            {
                // Chuyển list string từ filter.Genders sang list enum
                var genderEnums = filter.Genders
                    .Select(g =>
                    {
                        if (Enum.TryParse<GenderEnum>(g, true, out var result)) // true = ignoreCase
                            return (GenderEnum?)result;
                        return null;
                    })
                    .Where(e => e.HasValue)
                    .Select(e => e.Value)
                    .ToList();

                query = query.Where(p => genderEnums.Contains(p.Product.Gender));
            }

            // Thực hiện lấy danh sách sau khi áp dụng filter (chưa phân trang)
            var allFiltered = await query.ToListAsync();

            // Lấy thông tin bộ lọc (từ toàn bộ danh sách đã lọc)
            var categories = allFiltered.Select(p => p.Product.Name).OrderBy(name => name).Distinct().ToList();
            var sizes = allFiltered.Select(p => p.Size.Code).Distinct().ToList();
            var colors = allFiltered.Select(p => p.Color.Name).Distinct().ToList();
            var genders = allFiltered.Select(p => p.Product.Gender).Distinct().ToList();

            // Sắp xếp
            switch (filter.SortBy?.ToLower())
            {
                case "name":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.Name).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.Name).ToList();
                    break;

                case "price":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList()
                        : allFiltered.OrderByDescending(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList();
                    break;

                case "createdat":
                default:
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.CreatedAt).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.CreatedAt).ToList();
                    break;
            }

            // Phân trang
            var totalItems = allFiltered.Count;
            var skip = (filter.Page - 1) * filter.PageSize;
            var data = allFiltered.Skip(skip).Take(filter.PageSize).ToList();
            var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

            return new PageBongDa
            {
                Items = data.Select(p => p.ToCustomerDto()).ToList(),
                TotalPages = totalPages,
                CurrentPage = filter.Page,
                Categories = categories,
                Sizes = sizes,
                Colors = colors,
                Genders = genders
            };
        }

        public async Task<PageBongRo> GetAllBongRo(BongRoFilterRequest filter)
        {
            var allowedCategories = new[]
          { "Bóng Rổ", "giày bóng rổ", "bộ quần áo bóng rổ" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && p.Status == ProductDetailStatus.Active && allowedCategories
                .Contains(p.Product.Category.Name.Trim().ToLower())).AsQueryable();

            // Lọc theo tên sản phẩm
            if (filter.Product?.Any(p => !string.IsNullOrWhiteSpace(p)) == true)
            {
                var keywords = filter.Product
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim().ToLower())
                    .ToList();

                query = query.Where(p =>
                    keywords.Any(k => p.Product.Name.ToLower().Contains(k))
                );
            }

            // Lọc theo màu
            if (filter.Colors?.Any(c => !string.IsNullOrWhiteSpace(c)) == true)
            {
                var color = filter.Colors.Select(c => c.Trim().ToLower()).ToList();
                query = query.Where(p => color.Contains(p.Color.Name.ToLower()));
            }

            // Lọc theo size
            if (filter.Sizes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true)
            {
                var size = filter.Sizes.Select(s => s.Trim().ToLower()).ToList();
                query = query.Where(p => size.Contains(p.Size.Code.ToLower()));
            }

            // Lọc theo giới tính
            if (filter.Genders?.Any(g => !string.IsNullOrWhiteSpace(g)) == true)
            {
                // Chuyển list string từ filter.Genders sang list enum
                var genderEnums = filter.Genders
                    .Select(g =>
                    {
                        if (Enum.TryParse<GenderEnum>(g, true, out var result)) // true = ignoreCase
                            return (GenderEnum?)result;
                        return null;
                    })
                    .Where(e => e.HasValue)
                    .Select(e => e.Value)
                    .ToList();

                query = query.Where(p => genderEnums.Contains(p.Product.Gender));
            }

            // Thực hiện lấy danh sách sau khi áp dụng filter (chưa phân trang)
            var allFiltered = await query.ToListAsync();

            // Lấy thông tin bộ lọc (từ toàn bộ danh sách đã lọc)
           var categories = allFiltered.Select(p => p.Product.Name).OrderBy(name => name).Distinct().ToList();
            var sizes = allFiltered.Select(p => p.Size.Code).Distinct().ToList();
            var colors = allFiltered.Select(p => p.Color.Name).Distinct().ToList();
            var genders = allFiltered.Select(p => p.Product.Gender).Distinct().ToList();

            // Sắp xếp
            switch (filter.SortBy?.ToLower())
            {
                case "name":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.Name).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.Name).ToList();
                    break;

                case "price":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList()
                        : allFiltered.OrderByDescending(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList();
                    break;

                case "createdat":
                default:
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.CreatedAt).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.CreatedAt).ToList();
                    break;
            }

            // Phân trang
            var totalItems = allFiltered.Count;
            var skip = (filter.Page - 1) * filter.PageSize;
            var data = allFiltered.Skip(skip).Take(filter.PageSize).ToList();
            var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

            return new PageBongRo
            {
                Items = data.Select(p => p.ToCustomerDto()).ToList(),
                TotalPages = totalPages,
                CurrentPage = filter.Page,
                Categories = categories,
                Sizes = sizes,
                Colors = colors,
                Genders = genders
            };
        }

        public async Task<PageCauLong> GetAllCauLong(CauLongFilterRequest filter)
        {
            var allowedCategories = new[]
          { "Cầu Lông" ,"giày cầu lông", "bộ quần áo cầu lông" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && p.Status == ProductDetailStatus.Active && allowedCategories
                .Contains(p.Product.Category.Name.Trim().ToLower())).AsQueryable();

            // Lọc theo tên sản phẩm
            if (filter.Product?.Any(p => !string.IsNullOrWhiteSpace(p)) == true)
            {
                var keywords = filter.Product
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim().ToLower())
                    .ToList();

                query = query.Where(p =>
                    keywords.Any(k => p.Product.Name.ToLower().Contains(k))
                );
            }

            // Lọc theo màu
            if (filter.Colors?.Any(c => !string.IsNullOrWhiteSpace(c)) == true)
            {
                var color = filter.Colors.Select(c => c.Trim().ToLower()).ToList();
                query = query.Where(p => color.Contains(p.Color.Name.ToLower()));
            }

            // Lọc theo size
            if (filter.Sizes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true)
            {
                var size = filter.Sizes.Select(s => s.Trim().ToLower()).ToList();
                query = query.Where(p => size.Contains(p.Size.Code.ToLower()));
            }

            // Lọc theo giới tính
            if (filter.Genders?.Any(g => !string.IsNullOrWhiteSpace(g)) == true)
            {
                // Chuyển list string từ filter.Genders sang list enum
                var genderEnums = filter.Genders
                    .Select(g =>
                    {
                        if (Enum.TryParse<GenderEnum>(g, true, out var result)) // true = ignoreCase
                            return (GenderEnum?)result;
                        return null;
                    })
                    .Where(e => e.HasValue)
                    .Select(e => e.Value)
                    .ToList();

                query = query.Where(p => genderEnums.Contains(p.Product.Gender));
            }
            // Thực hiện lấy danh sách sau khi áp dụng filter (chưa phân trang)
            var allFiltered = await query.ToListAsync();

            // Lấy thông tin bộ lọc (từ toàn bộ danh sách đã lọc)
           var categories = allFiltered.Select(p => p.Product.Name).OrderBy(name => name).Distinct().ToList();
            var sizes = allFiltered.Select(p => p.Size.Code).Distinct().ToList();
            var colors = allFiltered.Select(p => p.Color.Name).Distinct().ToList();
            var genders = allFiltered.Select(p => p.Product.Gender).Distinct().ToList();

            // Sắp xếp
            switch (filter.SortBy?.ToLower())
            {
                case "name":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.Name).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.Name).ToList();
                    break;

                case "price":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList()
                        : allFiltered.OrderByDescending(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList();
                    break;

                case "createdat":
                default:
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.CreatedAt).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.CreatedAt).ToList();
                    break;
            }

            // Phân trang
            var totalItems = allFiltered.Count;
            var skip = (filter.Page - 1) * filter.PageSize;
            var data = allFiltered.Skip(skip).Take(filter.PageSize).ToList();
            var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

            return new PageCauLong
            {
                Items = data.Select(p => p.ToCustomerDto()).ToList(),
                TotalPages = totalPages,
                CurrentPage = filter.Page,
                Categories = categories,
                Sizes = sizes,
                Colors = colors,
                Genders = genders
            };
        }

        public async Task<PagaChayBo> GetAllChayBo(ChayBoFilterRequest filter)
        {
            var allowedCategories = new[]
          { "Chạy Bộ", "giày chạy bộ" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && p.Status == ProductDetailStatus.Active && allowedCategories
                .Contains(p.Product.Category.Name.Trim().ToLower())).AsQueryable();

            // Lọc theo tên sản phẩm
            if (filter.Product?.Any(p => !string.IsNullOrWhiteSpace(p)) == true)
            {
                var keywords = filter.Product
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim().ToLower())
                    .ToList();

                query = query.Where(p =>
                    keywords.Any(k => p.Product.Name.ToLower().Contains(k))
                );
            }

            // Lọc theo màu
            if (filter.Colors?.Any(c => !string.IsNullOrWhiteSpace(c)) == true)
            {
                var color = filter.Colors.Select(c => c.Trim().ToLower()).ToList();
                query = query.Where(p => color.Contains(p.Color.Name.ToLower()));
            }

            // Lọc theo size
            if (filter.Sizes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true)
            {
                var size = filter.Sizes.Select(s => s.Trim().ToLower()).ToList();
                query = query.Where(p => size.Contains(p.Size.Code.ToLower()));
            }

            // Lọc theo giới tính
            if (filter.Genders?.Any(g => !string.IsNullOrWhiteSpace(g)) == true)
            {
                // Chuyển list string từ filter.Genders sang list enum
                var genderEnums = filter.Genders
                    .Select(g =>
                    {
                        if (Enum.TryParse<GenderEnum>(g, true, out var result)) // true = ignoreCase
                            return (GenderEnum?)result;
                        return null;
                    })
                    .Where(e => e.HasValue)
                    .Select(e => e.Value)
                    .ToList();

                query = query.Where(p => genderEnums.Contains(p.Product.Gender));
            }

            // Thực hiện lấy danh sách sau khi áp dụng filter (chưa phân trang)
            var allFiltered = await query.ToListAsync();

            // Lấy thông tin bộ lọc (từ toàn bộ danh sách đã lọc)
           var categories = allFiltered.Select(p => p.Product.Name).OrderBy(name => name).Distinct().ToList();
            var sizes = allFiltered.Select(p => p.Size.Code).Distinct().ToList();
            var colors = allFiltered.Select(p => p.Color.Name).Distinct().ToList();
            var genders = allFiltered.Select(p => p.Product.Gender).Distinct().ToList();

            // Sắp xếp
            switch (filter.SortBy?.ToLower())
            {
                case "name":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.Name).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.Name).ToList();
                    break;

                case "price":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList()
                        : allFiltered.OrderByDescending(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList();
                    break;

                case "createdat":
                default:
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.CreatedAt).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.CreatedAt).ToList();
                    break;
            }

            // Phân trang
            var totalItems = allFiltered.Count;
            var skip = (filter.Page - 1) * filter.PageSize;
            var data = allFiltered.Skip(skip).Take(filter.PageSize).ToList();
            var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

            return new PagaChayBo
            {
                Items = data.Select(p => p.ToCustomerDto()).ToList(),
                TotalPages = totalPages,
                CurrentPage = filter.Page,
                Categories = categories,
                Sizes = sizes,
                Colors = colors,
                Genders = genders
            };
        }

        public async Task<PageGolf> GetAllGolf(Golf filter)
        {
            var allowedCategories = new[]
          { "Golf" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && p.Status == ProductDetailStatus.Active && allowedCategories
                .Contains(p.Product.Category.Name.Trim().ToLower())).AsQueryable();

            // Lọc theo tên sản phẩm
            if (filter.Product?.Any(p => !string.IsNullOrWhiteSpace(p)) == true)
            {
                var keywords = filter.Product
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim().ToLower())
                    .ToList();

                query = query.Where(p =>
                    keywords.Any(k => p.Product.Name.ToLower().Contains(k))
                );
            }

            // Lọc theo màu
            if (filter.Colors?.Any(c => !string.IsNullOrWhiteSpace(c)) == true)
            {
                var color = filter.Colors.Select(c => c.Trim().ToLower()).ToList();
                query = query.Where(p => color.Contains(p.Color.Name.ToLower()));
            }

            // Lọc theo size
            if (filter.Sizes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true)
            {
                var size = filter.Sizes.Select(s => s.Trim().ToLower()).ToList();
                query = query.Where(p => size.Contains(p.Size.Code.ToLower()));
            }

            // Lọc theo giới tính
            if (filter.Genders?.Any(g => !string.IsNullOrWhiteSpace(g)) == true)
            {
                // Chuyển list string từ filter.Genders sang list enum
                var genderEnums = filter.Genders
                    .Select(g =>
                    {
                        if (Enum.TryParse<GenderEnum>(g, true, out var result)) // true = ignoreCase
                            return (GenderEnum?)result;
                        return null;
                    })
                    .Where(e => e.HasValue)
                    .Select(e => e.Value)
                    .ToList();

                query = query.Where(p => genderEnums.Contains(p.Product.Gender));
            }

            // Thực hiện lấy danh sách sau khi áp dụng filter (chưa phân trang)
            var allFiltered = await query.ToListAsync();

            // Lấy thông tin bộ lọc (từ toàn bộ danh sách đã lọc)
           var categories = allFiltered.Select(p => p.Product.Name).OrderBy(name => name).Distinct().ToList();
            var sizes = allFiltered.Select(p => p.Size.Code).Distinct().ToList();
            var colors = allFiltered.Select(p => p.Color.Name).Distinct().ToList();
            var genders = allFiltered.Select(p => p.Product.Gender).Distinct().ToList();

            // Sắp xếp
            switch (filter.SortBy?.ToLower())
            {
                case "name":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.Name).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.Name).ToList();
                    break;

                case "price":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList()
                        : allFiltered.OrderByDescending(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList();
                    break;

                case "createdat":
                default:
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p => p.Product.CreatedAt).ToList()
                        : allFiltered.OrderByDescending(p => p.Product.CreatedAt).ToList();
                    break;
            }

            // Phân trang
            var totalItems = allFiltered.Count;
            var skip = (filter.Page - 1) * filter.PageSize;
            var data = allFiltered.Skip(skip).Take(filter.PageSize).ToList();
            var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

            return new PageGolf
            {
                Items = data.Select(p => p.ToCustomerDto()).ToList(),
                TotalPages = totalPages,
                CurrentPage = filter.Page,
                Categories = categories,
                Sizes = sizes,
                Colors = colors,
                Genders = genders
            };
        }

public async Task<PageTapLuyen> GetAllTapLuyen(TapLuyenFilterRequest filter)
{
        var allowedCategories = new[]
              { "Tập Luyện" };

        var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
            .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
            .Where(p => p.Product.Category != null && p.Quantity > 0 && p.Status == ProductDetailStatus.Active && allowedCategories
            .Contains(p.Product.Category.Name.Trim().ToLower())).AsQueryable();

        // Lọc theo tên sản phẩm
        if (filter.Product?.Any(p => !string.IsNullOrWhiteSpace(p)) == true)
        {
            var keywords = filter.Product
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim().ToLower())
                .ToList();

            query = query.Where(p =>
                keywords.Any(k => p.Product.Name.ToLower().Contains(k))
            );
        }

        // Lọc theo màu
        if (filter.Colors?.Any(c => !string.IsNullOrWhiteSpace(c)) == true)
        {
            var color = filter.Colors.Select(c => c.Trim().ToLower()).ToList();
            query = query.Where(p => color.Contains(p.Color.Name.ToLower()));
        }

        // Lọc theo size
        if (filter.Sizes?.Any(s => !string.IsNullOrWhiteSpace(s)) == true)
        {
            var size = filter.Sizes.Select(s => s.Trim().ToLower()).ToList();
            query = query.Where(p => size.Contains(p.Size.Code.ToLower()));
        }

           
            // Lọc theo giới tính
            if (filter.Genders?.Any(g => !string.IsNullOrWhiteSpace(g)) == true)
            {
                // Chuyển list string từ filter.Genders sang list enum
                var genderEnums = filter.Genders
                    .Select(g =>
                    {
                        if (Enum.TryParse<GenderEnum>(g, true, out var result)) // true = ignoreCase
                            return (GenderEnum?)result;
                        return null;
                    })
                    .Where(e => e.HasValue)
                    .Select(e => e.Value)
                    .ToList();

                query = query.Where(p => genderEnums.Contains(p.Product.Gender));
            }

            // Thực hiện lấy danh sách sau khi áp dụng filter (chưa phân trang)
            var allFiltered = await query.ToListAsync();

        // Lấy thông tin bộ lọc (từ toàn bộ danh sách đã lọc)
       var categories = allFiltered.Select(p => p.Product.Name).OrderBy(name => name).Distinct().ToList();
        var sizes = allFiltered.Select(p => p.Size.Code).Distinct().ToList();
        var colors = allFiltered.Select(p => p.Color.Name).Distinct().ToList();
        var genders = allFiltered.Select(p => p.Product.Gender).Distinct().ToList();

        // Sắp xếp
        switch (filter.SortBy?.ToLower())
        {
            case "name":
                allFiltered = filter.SortOrder == "asc"
                    ? allFiltered.OrderBy(p => p.Product.Name).ToList()
                    : allFiltered.OrderByDescending(p => p.Product.Name).ToList();
                break;

                case "price":
                    allFiltered = filter.SortOrder == "asc"
                        ? allFiltered.OrderBy(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList()
                        : allFiltered.OrderByDescending(p =>
                        {
                            var promo = p.PromotionProducts?
                                .FirstOrDefault(pp => pp.Promotion != null && pp.Promotion.Status == VoucherStatus.Active);
                            return promo != null ? promo.Priceafterduction : p.Price;
                        }).ToList();
                    break;

                case "createdat":
            default:
                allFiltered = filter.SortOrder == "asc"
                    ? allFiltered.OrderBy(p => p.Product.CreatedAt).ToList()
                    : allFiltered.OrderByDescending(p => p.Product.CreatedAt).ToList();
                break;
        }

        // Phân trang
        var totalItems = allFiltered.Count;
        var skip = (filter.Page - 1) * filter.PageSize;
        var data = allFiltered.Skip(skip).Take(filter.PageSize).ToList();
        var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

        return new PageTapLuyen
        {
            Items = data.Select(p => p.ToCustomerDto()).ToList(),
            TotalPages = totalPages,
            CurrentPage = filter.Page,
            Categories = categories,
            Sizes = sizes,
            Colors = colors,
            Genders = genders
        };
    }

     
        public async Task<ProductDetailCustomerDto> GetId(Guid id)
        {
            var detail = await _context.ProductDetails
                .Include(p => p.Color)
                .Include(p => p.Size)
                .Include(p => p.Material)
                .Include(p=>p.Images)
                .Include(p => p.Origin)
                .Include(p => p.Supplier)
                .Include(p=> p.Product).ThenInclude(p=> p.Category)
                .Include(pp => pp.Product).ThenInclude(pp => pp.Brand)
                .Include(p => p.PromotionProducts).ThenInclude(p => p.Promotion)
                .FirstOrDefaultAsync(p => p.Id == id);
            return detail?.ToCustomerDto();
        }
    }
}
