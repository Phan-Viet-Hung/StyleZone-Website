using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Request.SizeRequest
{
    public static class SeedSizesRequest
    {
        public static async Task SeedSizesAsync(DbContextApp context)
        {
            if (await context.Sizes.AnyAsync())
                return; // Nếu đã có dữ liệu rồi thì không thêm nữa

            var sizes = new List<Size>
            {
                new Size { Id = Guid.NewGuid(), Code = "S", Name = "Small", CreatedAt = DateTime.Now },
                new Size { Id = Guid.NewGuid(), Code = "M", Name = "Medium", CreatedAt = DateTime.Now },
                new Size { Id = Guid.NewGuid(), Code = "L", Name = "Large", CreatedAt = DateTime.Now },
                new Size { Id = Guid.NewGuid(), Code = "XL", Name = "Extra Large", CreatedAt = DateTime.Now },
                new Size { Id = Guid.NewGuid(), Code = "XXL", Name = "Double Extra Large", CreatedAt = DateTime.Now },
                new Size { Id = Guid.NewGuid(), Code = "XXXL", Name = "Triple Extra Large", CreatedAt = DateTime.Now }
            };

            context.Sizes.AddRange(sizes);
            await context.SaveChangesAsync();
        }
    }
}
