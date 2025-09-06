
using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.DTOs.TheThao;
using API.DomainCusTomer.DTOs.ThoiTrang;
using API.DomainCusTomer.ExTentions;
using API.DomainCusTomer.Request;
using API.DomainCusTomer.Request.ThoiTrang;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.DomainCusTomer.Services
{
    public class ThoiTrangCustomerServices : IThoiTrangCustomerServices
    {
        private readonly DbContextApp _context;

        public ThoiTrangCustomerServices(DbContextApp context)
        {
            _context = context;
        }
        public async Task<PageBADFIVE> GetAllBADFIVE(BADFIVEFilterRequst filter)
        {
            var allowedCategories = new[] { "BADFIVE" };

            var query = _context.ProductDetails
            .Include(p => p.Color)
            .Include(p => p.Size)
            .Include(p => p.Material)
            .Include(p => p.Origin)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
            .Where(p => p.Status == ProductDetailStatus.Active &&
                p.Product != null && p.Quantity > 0 &&
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

            return new PageBADFIVE
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

        public async Task<PageBeGai> GetAllBeGai(BeGaiFilterRequst filter)
        {
            var allowedCategories = new[] { "Bé Gái (7-14 tuổi)" };

            var query = _context.ProductDetails
            .Include(p => p.Color)
            .Include(p => p.Size)
            .Include(p => p.Material)
            .Include(p => p.Origin)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
            .Where(p => p.Status == ProductDetailStatus.Active &&
                p.Product != null && p.Quantity > 0 &&
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

            return new PageBeGai
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

        public async Task<PageBeTrai> GetAllBeTrai(BeTraiFilterRequst filter)
        {
            var allowedCategories = new[] { "Bé Trai (7-14 tuổi)" };

            var query = _context.ProductDetails
            .Include(p => p.Color)
            .Include(p => p.Size)
            .Include(p => p.Material)
            .Include(p => p.Origin)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
            .Where(p => p.Status == ProductDetailStatus.Active &&
                p.Product != null && p.Quantity > 0 &&
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

            return new PageBeTrai
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

        public async Task<PageISAAC> GetAllISAAC(ISAACFilterRequst filter)
        {
            var allowedCategories = new[] { "ISAAC" };

            var query = _context.ProductDetails
            .Include(p => p.Color)
            .Include(p => p.Size)
            .Include(p => p.Material)
            .Include(p => p.Origin)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
            .Where(p => p.Status == ProductDetailStatus.Active &&
                p.Product != null && p.Quantity > 0 &&
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

            return new PageISAAC
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

        public async Task<PageLIFESTYLE> GetAllLIFESTYLE(LIFESTYLEFilterRequst filter)
        {
            var allowedCategories = new[] { "LIFESTYLE" };

            var query = _context.ProductDetails
            .Include(p => p.Color)
            .Include(p => p.Size)
            .Include(p => p.Material)
            .Include(p => p.Origin)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
            .Where(p => p.Status == ProductDetailStatus.Active &&
                p.Product != null && p.Quantity > 0 &&
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

            return new PageLIFESTYLE
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

        public async Task<PageOUTLET> GetAllOUTLET(OUTLETFilterRequst filter)
        {
            var allowedCategories = new[] { "OUTLET", "OUTLETPICKLEBALL" };

            var query = _context.ProductDetails
            .Include(p => p.Color)
            .Include(p => p.Size)
            .Include(p => p.Material)
            .Include(p => p.Origin)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
            .Where(p => p.Status == ProductDetailStatus.Active &&
                p.Product != null && p.Quantity > 0 &&
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

            return new PageOUTLET
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

        public async Task<PageOUTLETPICKLEBALL> GetAllOUTLETPICKLEBALL(OUTLETPICKLEBALLFilterRequst filter)
        {
            var allowedCategories = new[] { "OUTLETPICKLEBALL" };

            var query = _context.ProductDetails
            .Include(p => p.Color)
            .Include(p => p.Size)
            .Include(p => p.Material)
            .Include(p => p.Origin)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
            .Where(p => p.Status == ProductDetailStatus.Active &&
                p.Product != null && p.Quantity > 0 &&
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

            return new PageOUTLETPICKLEBALL
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

        public async Task<PageThoiTrang> GetALLThoiTrang(ThoiTrangFilterRequst filter)
        {
            var allowedCategories = new[] { "Thời Trang","wade","badfive","lifestyle","isaac" };

            var query = _context.ProductDetails
            .Include(p => p.Color)
            .Include(p => p.Size)
            .Include(p => p.Material)
            .Include(p => p.Origin)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
            .Where(p => p.Status == ProductDetailStatus.Active &&
                p.Product != null && p.Quantity > 0 &&
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

            return new PageThoiTrang
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

        public async Task<PageWaDe> GetAllWaDe(WadeFilterRequst filter)
        {
            var allowedCategories = new[] { "WaDe" };

            var query = _context.ProductDetails
            .Include(p => p.Color)
            .Include(p => p.Size)
            .Include(p => p.Material)
            .Include(p => p.Origin)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
            .Where(p => p.Status == ProductDetailStatus.Active &&
                p.Product != null && p.Quantity > 0 &&
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

            return new PageWaDe
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

        public async Task<PageYOUNG> GetAllYOUNG(YOUNGFilterRequst filter)
        {
            var allowedCategories = new[] { "YOUNG", "Bé Trai (7-14 tuổi)", "Bé Gái (7-14 tuổi)" };

            var query = _context.ProductDetails
            .Include(p => p.Color)
            .Include(p => p.Size)
            .Include(p => p.Material)
            .Include(p => p.Origin)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .ThenInclude(p => p.Category).Include(p => p.PromotionProducts)
            .ThenInclude(p => p.Promotion)
            .Where(p => p.Status == ProductDetailStatus.Active &&
                p.Product != null && p.Quantity > 0 &&
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

            return new PageYOUNG
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
    }
}
