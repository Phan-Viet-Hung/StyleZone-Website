using API.Domain.DTOs;
using DAL_Empty.Models;

public static class VoucherExtentions
{
    public static VoucherDto ToDto(this Voucher v) => new()
    {
        Id = v.Id,
        Code = v.Code,
        Description = v.Description,
        ImageUrl = v.ImageUrl,
        DiscountType = v.DiscountType.ToString(),
        DiscountValue = v.DiscountValue,
        MinOrderAmount = v.MinOrderAmount = 1000,
        TotalUsageLimit = v.TotalUsageLimit,
        MaxUsagePerCustomer = v.MaxUsagePerCustomer = 1,
        Status = v.Status.ToString(),
        StartDate = v.StartDate,
        EndDate = v.EndDate,
        CreatedAt = v.CreatedAt,
        UpdatedAt = v.UpdatedAt
    };

}
