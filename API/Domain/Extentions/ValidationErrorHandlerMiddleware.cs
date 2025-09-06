using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Domain.Extentions
{
    // Middleware xử lý lỗi validation kiểu dữ liệu trả về dạng JSON chuẩn, chuyển thông báo sang tiếng Việt
    public class ValidationErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await _next(context);

            // Chỉ xử lý khi lỗi 400 và content-type là problem+json (lỗi validation model)
            if (context.Response.StatusCode == 400 &&
                context.Response.ContentType != null &&
                context.Response.ContentType.Contains("application/problem+json"))
            {
                memStream.Seek(0, SeekOrigin.Begin);

                var originalBodyText = await new StreamReader(memStream).ReadToEndAsync();

                var problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(originalBodyText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });

                string defaultMessage = "Dữ liệu đầu vào không hợp lệ. Vui lòng kiểm tra lại kiểu dữ liệu.";

                var newErrors = new Dictionary<string, string[]>();

                if (problemDetails?.Errors != null && problemDetails.Errors.Any())
                {
                    foreach (var err in problemDetails.Errors)
                    {
                        var newMessages = err.Value.Select(v =>
                        {
                            // Bắt lỗi dạng giá trị không hợp lệ mặc định
                            if (v.Contains("The value '' is invalid.") || (v.Contains("The value") && v.Contains("is not valid")))
                            {
                                return "Giá trị nhập không đúng kiểu dữ liệu hoặc không hợp lệ.";
                            }
                            return v;
                        }).ToArray();

                        newErrors.Add(err.Key, newMessages);
                    }
                }
                else
                {
                    // Trường hợp không có lỗi chi tiết
                    newErrors.Add("Lỗi dữ liệu", new[] { defaultMessage });
                }

                var errorResponse = new
                {
                    message = defaultMessage,
                    errors = newErrors
                };

                context.Response.ContentType = "application/json";
                context.Response.ContentLength = null;

                // Xoá nội dung cũ
                memStream.SetLength(0);

                // Ghi lại response mới
                await JsonSerializer.SerializeAsync(memStream, errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                memStream.Seek(0, SeekOrigin.Begin);
                await memStream.CopyToAsync(originalBodyStream);

                context.Response.Body = originalBodyStream;
                return; // Kết thúc middleware tại đây
            }

            // Nếu không phải lỗi 400 hoặc không phải content-type phù hợp thì trả lại nguyên response
            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }


    // Extension method để đăng ký middleware dễ dàng hơn
    public static class ValidationErrorHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseValidationErrorHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ValidationErrorHandlerMiddleware>();
        }
    }
}
