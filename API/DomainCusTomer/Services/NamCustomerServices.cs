using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.DTOs.Nam;
using API.DomainCusTomer.DTOs.Nu;
using API.DomainCusTomer.ExTentions;
using API.DomainCusTomer.Request;
using API.DomainCusTomer.Request.Nam;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.DomainCusTomer.Services
{
    public class NamCustomerServices : INamCustomer
    {
        private readonly DbContextApp _context;

        public NamCustomerServices(DbContextApp context)
        {
            _context = context;
        }

        public async Task<PageAoDaiTayNam> AoDaiTayNam(AoDaiTayNamFilterRequest filter)
        {
            var allowedCategories = new[]
          { "áo dài tay" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && ((p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAoDaiTayNam
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

        public async Task<PageAoGioNam> AoGioNam(AoGioNamFilterRequest filter)
        {
            var allowedCategories = new[]
           { "áo gió" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAoGioNam
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

        public async Task<PageAoLongVuNam> AoLongVuNam(AoLongVuNamFilterRequest filter)
        {
            var allowedCategories = new[]
          { "áo lông vũ" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAoLongVuNam
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

        public async Task<PageAonam> AoNam(AoNamFilterRequest filter)
        {
            var allowedCategories = new[]
           { "áo dài tay","ao t-shirt","áo polo", "áo gió","áo nỉ","áo dài tay","áo lông vũ"};

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAonam
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

        public async Task<PageAoNiNam> AoNiNam(AoNiNamFilterRequest filter)
        {
            var allowedCategories = new[]
          { "áo nỉ" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAoNiNam
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

        public async Task<PageAoPoLoNam> AoPoLoNam(AoPoLoNamFilterRequest filter)
        {
            var allowedCategories = new[]
          { "áo Polo" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAoPoLoNam
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

        public async Task<PageAoTShirtNam> AoTShirtNam(AoTShirtNamFilterRequest filter)
        {
            var allowedCategories = new[]
           { "Áo T-Shirt" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAoTShirtNam
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

        public async Task<PageBoQuanAoBongRoNam> BoQuanAoBongRoNam(BoQuanAoBongRoNamFilterRequest filter)
        {
            var allowedCategories = new[]
          { "bộ quần áo bóng rổ" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageBoQuanAoBongRoNam
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

        public async Task<PageBoQuanAoCauLongNam> BoQuanAoCauLongNam(BoQuanAoCauLongNamFilterRequest filter)
        {
            var allowedCategories = new[]
          { "bộ quần áo cầu lông" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageBoQuanAoCauLongNam
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

        public async Task<PageBoQuanAoNam> BoQuanAoNam(BoQuanAoNamFilterRequest filter)
        {
            var allowedCategories = new[]
          { "bộ quần áo cầu lông", "bộ quần áo bóng rổ" ,"bộ quần áo"};

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageBoQuanAoNam
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

        public async Task<PageDoNamCustomer> DoNamCustomer(DoNamCustomerFilterRequest filter)
        {
        

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active).AsQueryable();

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

            return new PageDoNamCustomer
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

        public Task<ProductDetailCustomerDto> GetId(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<PageGiayBongDaNam> GiayBongDaNam(GiayBongDaNamFilterRequest filter)
        {
            var allowedCategories = new[]
          { "giày bóng đá" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageGiayBongDaNam
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

        public async Task<PageGiayBongRoNam> GiayBongRoNam(GiayBongRoNamFilterRequest filter)
        {
            var allowedCategories = new[]
          { "giày bóng rổ" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageGiayBongRoNam
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

        public async Task<PageGiayCauLongNam> GiayCauLongNam(GiayCauLongNamFilterRequest filter)
        {
            var allowedCategories = new[]
           { "giày cầu lông" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageGiayCauLongNam
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

        public async Task<PageGiayChayBoNam> GiayChayBoNam(GiayChayBoNamFilterRequest filter)
        {
            var allowedCategories = new[]
           { "giày chạy bộ" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageGiayChayBoNam
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

        public async Task<PageGiayNam> GiayNam(GiayNamFilterRequest filter)
        {
            var allowedCategories = new[]
           { "giày nam","giày thời trang","giày chạy bộ", "giày cầu lông","giày bóng rổ","giày bóng đá" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageGiayNam
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

        public async Task<PageGiayThoiTrangNam> GiayThoiTrangNam(GiayThoiTrangNamFilterRequest filter)
        {
            var allowedCategories = new[]
          { "giày thời trang" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageGiayThoiTrangNam
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

        public async Task<PageQuanGioNam> QuanGioNam(QuanGioNamFilterRequest filter)
        {
            var allowedCategories = new[]
           { "quần gió" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageQuanGioNam
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

        public async Task<PageQuanNam> QuanNam(QuanNamFilterRequest filter)
        {
            var allowedCategories = new[]
          { "quần","quần short","quần gió","quần nỉ" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageQuanNam
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

        public async Task<PageQuanNiNam> QuanNiNam(QuanNiNamFilterRequest filter)
        {
            var allowedCategories = new[]
           { "quần nỉ" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageQuanNiNam
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

        public async Task<PageQuanShortNam> QuanShortNam(QuanShortNamFilterRequest filter)
        {
            var allowedCategories = new[]
          { "quần short" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nam || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageQuanShortNam
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
    }
}
