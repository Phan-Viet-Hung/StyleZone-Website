using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.DTOs.Nam;
using API.DomainCusTomer.DTOs.Nu;
using API.DomainCusTomer.DTOs.TheThao;
using API.DomainCusTomer.ExTentions;
using API.DomainCusTomer.Request;
using API.DomainCusTomer.Request.Nu;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.DomainCusTomer.Services
{
    public class NuCustomerservices : INuCustomer
    {
        private readonly DbContextApp _context;
        
        public NuCustomerservices(DbContextApp context)
        {
            _context = context;
        }
        public async Task<PageAoDaiTayNu> AoDaiTayNu(AoDaiTayNuFilterRequest filter)
        {

            var allowedCategories = new[]
           { "áo dài tay" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null&& p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAoDaiTayNu
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

        public async Task<PageAoGioNu> AoGioNu(AoGioNuFilterRequest filter)
        {
            var allowedCategories = new[]
           { "Áo Gió" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAoGioNu
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

        public async Task<PageAoLongVuNu> AoLongVuNu(AoLongVuNuFilterRequest filter)
        {
            var allowedCategories = new[]{ "áo lông vũ" };
            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAoLongVuNu
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

        public async Task<PageAoNiNu> AoNiNu(AoNiNuFilterRequest filter)
        {
            var allowedCategories = new[]
{ "áo nỉ" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAoNiNu
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

        public async Task<PageAoPoLoNu> AoPoLoNu(AoPoLoNuFilterRequest filter)
        {
            var allowedCategories = new[]{ "áo Polo" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAoPoLoNu
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

        public async Task<PageAoTShirtNu> AoTShirtNu(AoTShirtNuFilterRequest filter)
        {
            var allowedCategories = new[]
{ "Áo T-Shirt" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAoTShirtNu
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

        public async Task<PageBoQuanAoBongRoNu> BoQuanAoBongRoNu(BoQuanAoBongRoNuFilterRequest filter)
        {
            var allowedCategories = new[]
{ "bộ quần áo bóng rổ" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageBoQuanAoBongRoNu
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

        public async Task<PageBoQuanAoCauLongNu> BoQuanAoCauLongNu(BoQuanAoCauLongNuFilterRequest filter)
        {
            var allowedCategories = new[]
{ "bộ quần áo cầu lông" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageBoQuanAoCauLongNu
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

        public async Task<PageBoQuanAoNu> BoQuanAoNu(BoQuanAoNuFilterRequest filter)
        {
            var allowedCategories = new[]{ "bộ quần áo", "bộ quần áo bóng rổ", "bộ quần áo cầu lông" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageBoQuanAoNu
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

        public async Task<PageGiayBongDaNu> GiayBongDaNu(GiayBongDaNuFilterRequest filter)
        {
            var allowedCategories = new[]{ "giày bóng đá" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageGiayBongDaNu
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

        public async Task<PageGiayBongRoNu> GiayBongRoNu(GiayBongRoNuFilterRequest filter)
        {
            var allowedCategories = new[]{ "giày bóng rổ" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageGiayBongRoNu
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

        public async Task<PageGiayCauLongNu> GiayCauLongNu(GiayCauLongNuFilterRequest filter)
        {
            var allowedCategories = new[]
{ "áo dài" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageGiayCauLongNu
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

        public async Task<PageGiayChayBoNu> GiayChayBoNu(GiayChayBoNuFilterRequest filter)
        {
            var allowedCategories = new[]{ "giày chạy bộ" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageGiayChayBoNu
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

        public async Task<PageGiayNu> GiayNu(GiayNuFilterRequest filter)
        {
            var allowedCategories = new[]{ "Giày", "giày thời trang","giày chạy bộ","giày cầu lông","giày bóng rổ","giày bóng đá" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageGiayNu
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

        public async Task<PageGiayThoiTrangNu> GiayThoiTrangNu(GiayThoiTrangNuFilterRequest filter)
        {
            var allowedCategories = new[] {  "giày thời trang"};

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageGiayThoiTrangNu
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

        public async Task<PageQuanGioNu> QuanGioNu(QuanGioNuFilterRequest filter)
        {
            var allowedCategories = new[] { "quần gió" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageQuanGioNu
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

        public async Task<PageQuanNiNu> QuanNiNu(QuanNiNuFilterRequest filter)
        {
            var allowedCategories = new[] { "quần nỉ"};

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageQuanNiNu
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

        public async Task<PageQuanNu> QuanNu(QuanNuFilterRequest filter)
        {
            var allowedCategories = new[] { "quần nỉ",  "quần", "quần gió", "quần short" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageQuanNu
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

        public async Task<PageQuanShortNu> QuanShortNu(QuanShortNuFilterRequest filter)
        {
            var allowedCategories = new[] { "quần short" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageQuanShortNu
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

        public async Task<PageDoNuCustome> DoNuCustomer(DoNuCustomerFilterRequest filter)
        {
            //var allowedCategories = new[] { "quần nỉ", "quần", "quần gió", "quần short" };

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active ).AsQueryable() ;

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

            return new PageDoNuCustome
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

        public async Task<PageAoNu> AoNu(AoNuFilterRequest filter)
        {
            var allowedCategories = new[]
          { "áo dài tay","ao t-shirt","áo polo", "áo gió","áo nỉ","áo dài tay","áo lông vũ"};

            var query = _context.ProductDetails.Include(p => p.Color).Include(p => p.Size).Include(p => p.Material).Include(p => p.Origin)
                .Include(p => p.Supplier).Include(p => p.Images).Include(p => p.Product).ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
                .Where(p => p.Product.Category != null && p.Quantity > 0 && (p.Product.Gender == GenderEnum.Nu || p.Product.Gender==GenderEnum.Khac) && p.Status == ProductDetailStatus.Active && allowedCategories
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

            return new PageAoNu
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
