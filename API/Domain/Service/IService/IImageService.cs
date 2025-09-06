using API.Domain.DTOs;
using API.Domain.Request.ImageRequest;
using Microsoft.AspNetCore.Mvc;

namespace API.Domain.Service.IService
{
    public interface IImageService
    {
        Task<List<ImageDto>> UploadMultipleImagesAsync(UploadMultipleImagesRequest request);
        Task<bool> UpdateImageAsync(UpdateImageRequest request);
        Task<List<ImageDto>> GetImagesByProductDetailIdAsync(Guid productDetailId);
        Task<bool> SetMainImageAsync(Guid imageId, Guid productDetailId);
        Task<List<ImageDto>> GetAllAsync();
        Task<ImageDto?> GetByIdAsync(Guid id);
        Task<bool> DeleteAllImagesByProductDetailIdAsync(Guid productDetailId);
    }
}
