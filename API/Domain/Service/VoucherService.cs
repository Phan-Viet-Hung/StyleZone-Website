using API.Domain.DTOs;
using API.Domain.Request.VoucherRequest;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace API.Domain.Service
{
    public class VoucherService : IVoucherService
    {
        private readonly DbContextApp _context;
        private readonly IWebHostEnvironment _env;
        private readonly IStringLocalizer<VoucherService> _localizer;

        public VoucherService(DbContextApp context, IWebHostEnvironment env, IStringLocalizer<VoucherService> localizer)
        {
            _context = context;
            _env = env;
            _localizer = localizer;
        }

        // ==================== CREATE ====================
        public async Task<VoucherDto> CreateAsync(CreateVoucherRequest request)
        {
            if (await _context.Vouchers.AnyAsync(v => v.Code == request.Code))
                throw new Exception(_localizer["Mã voucher này đã tồn tại."]);

            ValidateVoucherData(request);

            var voucherId = Guid.NewGuid();
            string imagePath = await SaveImageAsync(request.ImageFile, request.ImageUrl, voucherId);

            var now = DateTime.Now;
            var status = request.Status;

            // Nếu ngày bắt đầu ở tương lai => set Inactive
            if (request.StartDate > now)
                status = VoucherStatus.Inactive;

            var voucher = new Voucher
            {
                Id = voucherId,
                Code = request.Code,
                Description = request.Description,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue.Value,
                MinOrderAmount = request.MinOrderAmount,
                TotalUsageLimit = request.TotalUsageLimit ?? 0,
                MaxUsagePerCustomer = request.MaxUsagePerCustomer ?? 1,
                Status = status,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = now,
                ImageUrl = imagePath
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return voucher.ToDto();
        }

        // ==================== UPDATE ====================
        public async Task<VoucherDto> UpdateAsync(UpdateVoucherRequest request)
        {
            var voucher = await _context.Vouchers.FindAsync(request.Id);
            if (voucher == null)
                throw new Exception(_localizer["Không tìm thấy voucher."]);

            bool isStartDateChanged = voucher.StartDate != request.StartDate;
            ValidateVoucherData(request, isUpdate: true, isStartDateChanged: isStartDateChanged);

            string imagePath = await SaveImageAsync(request.ImageFile, request.ImageUrl, request.Id, isUpdate: true);

            var now = DateTime.Now;
            var status = request.Status;

            if (request.StartDate > now)
                status = VoucherStatus.Inactive;

            voucher.Code = request.Code;
            voucher.Description = request.Description;
            voucher.DiscountType = request.DiscountType;
            voucher.DiscountValue = request.DiscountValue.Value;
            voucher.MinOrderAmount = request.MinOrderAmount;
            voucher.TotalUsageLimit = request.TotalUsageLimit ?? 0;
            voucher.MaxUsagePerCustomer = request.MaxUsagePerCustomer ?? 1;
            voucher.Status = status;
            voucher.StartDate = request.StartDate;
            voucher.EndDate = request.EndDate;
            voucher.UpdatedAt = now;
            voucher.ImageUrl = imagePath;

            await _context.SaveChangesAsync();

            return voucher.ToDto();
        }

        // ==================== VALIDATE ====================
        private void ValidateVoucherData(CreateVoucherRequest request, bool isUpdate = false, bool isStartDateChanged = true)
        {
            var start = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Local);
            var end = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Local);
            var now = DateTime.Now;

            if (start >= end)
                throw new Exception(_localizer["Ngày bắt đầu phải trước ngày kết thúc."]);

            if (end < now)
                throw new Exception(_localizer["Ngày kết thúc phải ở tương lai."]);

            if (!isUpdate || (isUpdate && isStartDateChanged))
            {
                if (start < now)
                    throw new Exception(_localizer["Ngày bắt đầu không được ở quá khứ."]);
            }

            if (!Enum.IsDefined(typeof(DiscountType), request.DiscountType))
                throw new Exception(_localizer["Loại giảm giá không hợp lệ."]);

            if (!Enum.IsDefined(typeof(VoucherStatus), request.Status))
                throw new Exception(_localizer["Trạng thái không hợp lệ."]);

            if (request.DiscountType == DiscountType.Percentage && (request.DiscountValue <= 0 || request.DiscountValue > 100))
                throw new Exception(_localizer["Giá trị phần trăm giảm giá phải từ 1 đến 100."]);

            if (!string.IsNullOrEmpty(request.ImageUrl) && request.ImageFile != null)
                throw new Exception(_localizer["OnlyOneImageAllowed"]);

            if (string.IsNullOrWhiteSpace(request.Code))
                throw new Exception(_localizer["Mã voucher không được để trống."]);

            if (string.IsNullOrWhiteSpace(request.Description))
                throw new Exception(_localizer["Mô tả không được để trống."]);

            if (request.DiscountValue <= 0)
                throw new Exception(_localizer["Giá trị giảm giá phải là số dương."]);

            if (request.MinOrderAmount < 0)
                throw new Exception(_localizer["Giá trị đơn hàng tối thiểu không được âm."]);
        }

        // ==================== SAVE IMAGE ====================
        private async Task<string> SaveImageAsync(IFormFile? file, string? imageUrl, Guid voucherId, bool isUpdate = false)
        {
            if (!string.IsNullOrEmpty(imageUrl))
                return imageUrl;

            if (file == null || file.Length == 0)
                throw new Exception(_localizer["InvalidImage"]);

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(webRoot, "uploads", "vouchers");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"{voucherId}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(folderPath, fileName);

            if (isUpdate && File.Exists(filePath))
                File.Delete(filePath);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/vouchers/{fileName}";
        }

        // ==================== GET ====================
        public async Task<VoucherDto?> GetByIdAsync(Guid id)
        {
            var v = await _context.Vouchers.FindAsync(id);
            if (v == null) return null;

            bool updated = AutoUpdateVoucherStatus(v);

            if (updated)
            {
                _context.Vouchers.Update(v);
                await _context.SaveChangesAsync();
            }

            return v.ToDto();
        }


        // ==================== GET ====================
        public async Task<List<VoucherDto>> GetAllAsync()
        {
            var vouchers = await _context.Vouchers.ToListAsync();

            foreach (var v in vouchers)
            {
                AutoUpdateVoucherStatus(v);
            }

            await _context.SaveChangesAsync();

            var dtoList = vouchers.Select(v => v.ToDto());

            // 👉 Thêm phần sắp xếp ở đây
            var sortedList = dtoList
                .OrderBy(d => Enum.TryParse<VoucherStatus>(d.Status, out var status) ? status : VoucherStatus.Inactive)
                .ThenBy(d => d.StartDate)
                .ThenBy(d => Enum.TryParse<DiscountType>(d.DiscountType, out var type) ? type : DiscountType.Percentage)
                .ToList();

            return sortedList;
        }


        // ==================== CHANGE STATUS ====================
        public async Task<bool> ChangeStatusAsync(ChangeStatusRequest request)
        {
            var voucher = await _context.Vouchers.FindAsync(request.Id);
            if (voucher == null) return false;

            if (!Enum.TryParse<VoucherStatus>(request.Status, true, out var newStatus))
                throw new Exception(_localizer["Trạng thái không hợp lệ."]);

            var now = DateTime.Now;
            if (voucher.EndDate < now)
            {
                voucher.StartDate = now;
                voucher.EndDate = now.AddDays(1);
            }

            voucher.Status = newStatus;
            voucher.UpdatedAt = now;

            _context.Vouchers.Update(voucher);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> BulkChangeStatusAsync(BulkStatusChangeRequest request)
        {
            if (!Enum.TryParse<VoucherStatus>(request.Status, true, out var newStatus))
                throw new Exception(_localizer["Trạng thái không hợp lệ."]);

            var vouchers = await _context.Vouchers
                .Where(v => request.Ids.Contains(v.Id))
                .ToListAsync();

            if (!vouchers.Any()) return false;

            var now = DateTime.Now;
            foreach (var v in vouchers)
            {
                if (v.EndDate < now)
                {
                    v.StartDate = now;
                    v.EndDate = now.AddDays(1);
                }

                v.Status = newStatus;
                v.UpdatedAt = now;
            }

            _context.Vouchers.UpdateRange(vouchers);
            await _context.SaveChangesAsync();

            return true;
        }
        private bool AutoUpdateVoucherStatus(Voucher voucher)
        {
            var now = DateTime.Now;
            bool updated = false;

            // Chỉ auto update nếu status hiện tại là Active hoặc Inactive
            if (voucher.Status == VoucherStatus.Active || voucher.Status == VoucherStatus.Inactive)
            {
                if (voucher.EndDate <= now || voucher.TotalUsageLimit == 0)
                {
                    voucher.Status = VoucherStatus.Expired;
                    voucher.UpdatedAt = now;
                    updated = true;
                }
                else if (voucher.StartDate <= now && voucher.Status == VoucherStatus.Inactive && voucher.EndDate >= now)
                {
                    voucher.Status = VoucherStatus.Active;
                    voucher.UpdatedAt = now;
                    updated = true;
                }
                else if (voucher.StartDate > now && voucher.Status == VoucherStatus.Active)
                {
                    voucher.Status = VoucherStatus.Inactive;
                    voucher.UpdatedAt = now;
                    updated = true;
                }
            }
            // Nếu status là Expired thì không thay đổi gì

            return updated;
        }
    }
}
