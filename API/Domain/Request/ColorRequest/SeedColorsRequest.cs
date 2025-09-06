using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Request.ColorRequest
{
    public class SeedColorsRequest
    {
        public static async Task SeedColorsAsync(DbContextApp context)
        {
            if (await context.Colors.AnyAsync())
                return;

            var colors = new List<Color>
        {
            new() { Id = Guid.NewGuid(), Code = "#FF0000", Name = "Red" },
            new() { Id = Guid.NewGuid(), Code = "#FFA500", Name = "Orange" },
            new() { Id = Guid.NewGuid(), Code = "#FFFF00", Name = "Yellow" },
            new() { Id = Guid.NewGuid(), Code = "#008000", Name = "Green" },
            new() { Id = Guid.NewGuid(), Code = "#0000FF", Name = "Blue" },
            new() { Id = Guid.NewGuid(), Code = "#4B0082", Name = "Indigo" },
            new() { Id = Guid.NewGuid(), Code = "#EE82EE", Name = "Violet" },
            new() { Id = Guid.NewGuid(), Code = "#FFFFFF", Name = "White" },
            new() { Id = Guid.NewGuid(), Code = "#000000", Name = "Black" },
        };

            await context.Colors.AddRangeAsync(colors);
            await context.SaveChangesAsync();
        }
    }
}
