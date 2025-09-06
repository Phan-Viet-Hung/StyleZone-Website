using API.Domain.DTOs;
using API.Domain.Request.ImageRequest;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Service
{
    public class ImageService : IImageService
    {
        private readonly DbContextApp _context;
        private readonly IWebHostEnvironment _env;

        public ImageService(DbContextApp context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<List<ImageDto>> UploadMultipleImagesAsync(UploadMultipleImagesRequest request)
        {
            var result = new List<Image>();
            int total = (request.Files?.Count ?? 0) + (request.Urls?.Count ?? 0);

            if (request.MainImageIndex is not null && (request.MainImageIndex < 0 || request.MainImageIndex >= total))
                throw new Exception("Chỉ số ảnh chính không hợp lệ.");

            if (request.ProductDetailId == null)
                throw new Exception("Thiếu ProductDetailId khi upload ảnh.");

            int index = 0;

            // Xử lý ảnh từ URL
            if (request.Urls != null)
            {
                foreach (var url in request.Urls)
                {
                    var image = new Image
                    {
                        Id = Guid.NewGuid(),
                        Url = url,
                        FileName = Path.GetFileName(url),
                        ProductDetailId = request.ProductDetailId,
                        IsMainImage = index == request.MainImageIndex,
                        AltText = request.AltTexts?.ElementAtOrDefault(index),
                        CreatedAt = DateTime.Now
                    };
                    result.Add(image);
                    index++;
                }
            }

            // Xử lý ảnh từ File upload
            if (request.Files != null && request.Files.Any())
            {
                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var productFolder = Path.Combine(webRootPath, "uploads", "images", request.ProductDetailId.ToString());

                if (!Directory.Exists(productFolder))
                    Directory.CreateDirectory(productFolder);

                foreach (var file in request.Files)
                {
                    var ext = Path.GetExtension(file.FileName);
                    var imageId = Guid.NewGuid();

                    var fileName = imageId + ext;
                    var filePath = Path.Combine(productFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var relativeUrl = $"/uploads/images/{request.ProductDetailId}/{fileName}";

                    var image = new Image
                    {
                        Id = imageId,
                        Url = relativeUrl,
                        FileName = file.FileName,
                        ProductDetailId = request.ProductDetailId,
                        IsMainImage = index == request.MainImageIndex,
                        AltText = request.AltTexts?.ElementAtOrDefault(index),
                        CreatedAt = DateTime.Now
                    };
                    result.Add(image);
                    index++;
                }
            }

            // Reset các ảnh chính cũ nếu có
            if (request.MainImageIndex != null)
            {
                var oldImages = await _context.Images
                    .Where(x => x.ProductDetailId == request.ProductDetailId && x.IsMainImage)
                    .ToListAsync();

                foreach (var img in oldImages)
                    img.IsMainImage = false;
            }

            _context.Images.AddRange(result);
            await _context.SaveChangesAsync();

            return result.Select(i => new ImageDto
            {
                Id = i.Id,
                Url = i.Url,
                FileName = i.FileName,
                AltText = i.AltText,
                IsMainImage = i.IsMainImage,
                ProductDetailId = i.ProductDetailId,
                CreatedAt = i.CreatedAt
            }).ToList();
        }




        public async Task<bool> UpdateImageAsync(UpdateImageRequest request)
        {
            var image = await _context.Images.FindAsync(request.Id);
            if (image == null)
                throw new Exception("Không tìm thấy ảnh.");

            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            // Nếu người dùng upload file mới
            if (request.File != null)
            {
                if (image.ProductDetailId == null)
                    throw new Exception("Ảnh chưa được liên kết với ProductDetail nên không thể lưu theo thư mục.");

                // Xóa file cũ nếu tồn tại
                if (!string.IsNullOrEmpty(image.Url))
                {
                    var oldFilePath = Path.Combine(webRootPath, image.Url.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var ext = Path.GetExtension(request.File.FileName);

                // Đường dẫn thư mục theo ProductDetailId
                var folderPath = Path.Combine(webRootPath, "uploads", "images", image.ProductDetailId.ToString());
                Directory.CreateDirectory(folderPath); // tạo nếu chưa có

                // Dùng Guid để tạo tên file mới (tránh đè)
                var fileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(folderPath, fileName);

                // Ghi file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                // Cập nhật Url và Tên File
                var relativeUrl = $"/uploads/images/{image.ProductDetailId}/{fileName}";
                image.Url = relativeUrl;
                image.FileName = request.File.FileName;
            }
            else if (!string.IsNullOrWhiteSpace(request.Url))
            {
                // Nếu có URL mới thì xóa file cũ trên ổ đĩa (nếu có)
                if (!string.IsNullOrEmpty(image.Url) && image.Url.StartsWith("/uploads"))
                {
                    var oldFilePath = Path.Combine(webRootPath, image.Url.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Người dùng nhập URL thủ công
                image.Url = request.Url;
                image.FileName = Path.GetFileName(request.Url);
            }

            // Cập nhật AltText nếu có
            if (!string.IsNullOrWhiteSpace(request.AltText))
                image.AltText = request.AltText;

            // Cập nhật FileName thủ công nếu có
            if (!string.IsNullOrWhiteSpace(request.FileName))
                image.FileName = request.FileName;

            // Xử lý ảnh chính
            if (request.IsMainImage.HasValue)
            {
                if (request.IsMainImage.Value)
                {
                    var currentMain = await _context.Images
                        .Where(x => x.ProductDetailId == image.ProductDetailId && x.IsMainImage && x.Id != image.Id)
                        .ToListAsync();

                    foreach (var img in currentMain)
                        img.IsMainImage = false;

                    image.IsMainImage = true;
                }
                else
                {
                    image.IsMainImage = false;
                }
            }

            image.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllImagesByProductDetailIdAsync(Guid productDetailId)
        {
            if (productDetailId == Guid.Empty)
                throw new Exception("ProductDetailId không hợp lệ.");

            var images = await _context.Images
                .Where(x => x.ProductDetailId == productDetailId)
                .ToListAsync();

            if (!images.Any())
                return true; // Không có ảnh, coi như thành công

            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            foreach (var image in images)
            {
                // Xóa file trên ổ đĩa nếu tồn tại
                if (!string.IsNullOrEmpty(image.Url) && image.Url.StartsWith("/uploads"))
                {
                    var filePath = Path.Combine(webRootPath, image.Url.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }

            // Xóa record trong database
            _context.Images.RemoveRange(images);
            await _context.SaveChangesAsync();

            return true;
        }



        public async Task<bool> SetMainImageAsync(Guid imageId, Guid productDetailId)
        {
            var image = await _context.Images.FindAsync(imageId);
            if (image == null)
                throw new Exception("Ảnh không tồn tại.");

            if (image.ProductDetailId != productDetailId)
                throw new Exception("Ảnh không thuộc ProductDetailId đã cung cấp.");

            // Đặt tất cả ảnh của ProductDetailId này về IsMainImage = false
            var images = await _context.Images
                .Where(x => x.ProductDetailId == productDetailId)
                .ToListAsync();

            foreach (var img in images)
                img.IsMainImage = false;

            // Gán ảnh được chọn là ảnh chính
            image.IsMainImage = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ImageDto>> GetAllAsync()
        {
            return await _context.Images
                .Include(i => i.ProductDetail)
                .Select(i => new ImageDto
                {
                    Id = i.Id,
                    Url = i.Url,
                    FileName = i.FileName,
                    AltText = i.AltText,
                    IsMainImage = i.IsMainImage,
                    ProductDetailId = i.ProductDetailId,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<ImageDto?> GetByIdAsync(Guid id)
        {
            return await _context.Images
                .Include(i => i.ProductDetail)
                .Where(i => i.Id == id)
                .Select(i => new ImageDto
                {
                    Id = i.Id,
                    Url = i.Url,
                    FileName = i.FileName,
                    AltText = i.AltText,
                    IsMainImage = i.IsMainImage,
                    ProductDetailId = i.ProductDetailId,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }
        public async Task<List<ImageDto>> GetImagesByProductDetailIdAsync(Guid productDetailId)
        {
            return await _context.Images
                .Where(i => i.ProductDetailId == productDetailId)
                .OrderByDescending(i => i.IsMainImage) // MainImage hiển thị trước nếu cần
                .Select(i => new ImageDto
                {
                    Id = i.Id,
                    Url = i.Url,
                    FileName = i.FileName,
                    AltText = i.AltText,
                    IsMainImage = i.IsMainImage,
                    ProductDetailId = i.ProductDetailId,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync();
        }

        private async Task<ImageDto> MapToDto(Guid id)
        {
            return await GetByIdAsync(id) ?? throw new Exception("Không tìm thấy hình ảnh sau khi tạo.");
        }
    }
}
