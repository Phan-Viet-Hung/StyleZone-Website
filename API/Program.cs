using API.Configuration;
using API.Domain.Request.ColorRequest;
using API.Domain.Request.SizeRequest;
using API.Domain.Service.IService;
using API.Domain.Service;
using API.Domain.Validate.IExcelValidator;
using API.Domain.Validate;
using API.DomainCusTomer.Config;
using API.DomainCusTomer.Services.IServices;
using API.DomainCusTomer.Services;
using API.Service;
using API.Services;
using DAL_Empty.Models;
using DomainAPI.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text;
using API.Domain.Service.IService.ICustomerService;

// =====================================
// 1. Create Builder
// =====================================
var builder = WebApplication.CreateBuilder(args);

// =====================================
// 2. Localization
// =====================================
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("vi") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("vi");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
    options.RequestCultureProviders.Insert(1, new CookieRequestCultureProvider());
    options.RequestCultureProviders.Insert(2, new AcceptLanguageHeaderRequestCultureProvider());
});

// =====================================
// 3. Controllers + JSON
// =====================================
builder.Services.AddControllers()
  .AddDataAnnotationsLocalization(options =>
  {
      options.DataAnnotationLocalizerProvider = (type, factory) =>
          factory.Create("ValidationMessages", typeof(Program).Assembly.FullName);
  })
  .AddJsonOptions(opt =>
  {
      opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
  });

// =====================================
// 4. JWT Authentication
// =====================================
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt?.Issuer ?? builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = jwt?.Audience ?? builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt?.SecretKey ?? builder.Configuration["JwtSettings:SecretKey"])
            ),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

// =====================================
// 5. Session & Cookie
// =====================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(10);
});
builder.Services.Configure<CookieOptions>(options =>
{
    options.Expires = DateTime.Now.AddDays(7);
    options.HttpOnly = false;
    options.Secure = true;
    options.SameSite = SameSiteMode.Lax;
    options.Path = "/";
    options.IsEssential = true;
});
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.None;
    options.Secure = CookieSecurePolicy.None;
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// =====================================
// 6. Swagger
// =====================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "StyleZone API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập JWT token vào đây. Ví dụ: Bearer <token>",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =====================================
// 7. CORS
// =====================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost7030", policy =>
    {
        policy.WithOrigins("https://localhost:7030")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// =====================================
// 8. DbContext
// =====================================
builder.Services.AddDbContext<DbContextApp>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================================
// 9. Dependency Injection Services
// =====================================
// Admin services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IModeOfPaymentService, ModeOfPaymentService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductDetailService, ProductDetailService>();
builder.Services.AddScoped<IColorService, ColorService>();
builder.Services.AddScoped<ISizeService, SizeService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IOriginService, OriginService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();
//builder.Services.AddScoped<IPaymentCallbackHandler, MomoCallbackHandler>();
//builder.Services.AddScoped<API.Domain.Services.IServices.IMomoService, API.Domain.Services.MomoServicer>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IExcelValidator<ProductDetail>, ProductDetailValidator>();
builder.Services.AddScoped<ExcelImporter>();

// Customer services
builder.Services.AddScoped<ITheThaoCustomerServices, TheThaoCusTomerSerVices>();
builder.Services.AddScoped<IThoiTrangCustomerServices, ThoiTrangCustomerServices>();
builder.Services.AddScoped<INamCustomer, NamCustomerServices>();
builder.Services.AddScoped<INuCustomer, NuCustomerservices>();
builder.Services.AddScoped<IDetailCustomerServices, DetailCustomerServices>();
builder.Services.AddScoped<ILoginAccountCustomerServices, LoginAccountCustomerServices>();
builder.Services.AddScoped<ICartCustomerService, CartCustomerService>();
builder.Services.AddHttpClient<IGhnService, GhnSerVices>();
builder.Services.AddScoped<IThanhtoanCustomer, ThanhToanCustomer>();
builder.Services.AddScoped<ISeachCustomerService, SeachCustomerService>();
builder.Services.AddScoped<ITinTucService, TinTucService>();
builder.Services.AddScoped<ITrangChuCustomerService, TrangChuCustomerService>();
builder.Services.AddScoped<IDonMuaCustomerServices, DonMuaCustomerService>();
builder.Services.AddScoped<ICartCustomerIDServices, CartCustomerIDServices>();
builder.Services.AddScoped<IThanhtoanCartIdServices, ThanhtoanCartIdServices>();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("Mail"));
builder.Services.AddSingleton<EmailCustomerServicer>();
builder.Services.AddSingleton<OtpHelperServices>();
builder.Services.AddScoped<JwtTokenHelper>();

// Common
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Logging.AddConsole();
builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Mail"));

// =====================================
// 10. Build App
// =====================================
var app = builder.Build();

// Load SMTP password from env
var smtpSettings = app.Services.GetRequiredService<IOptions<SmtpSettings>>().Value;
if (!string.IsNullOrEmpty(smtpSettings.Pass) && smtpSettings.Pass.StartsWith("env:"))
{
    var envVar = smtpSettings.Pass.Replace("env:", "");
    smtpSettings.Pass = Environment.GetEnvironmentVariable(envVar)
        ?? throw new Exception($"Missing environment variable: {envVar}");
}

// =====================================
// 11. Seed Data
// =====================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DbContextApp>();
    await SeedColorsRequest.SeedColorsAsync(dbContext);
    await SeedSizesRequest.SeedSizesAsync(dbContext);
}

// =====================================
// 12. Middleware
// =====================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StyleZone API V1");
    });
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.UseRouting();

// CORS phải đặt trước Auth & Authorization
app.UseCors("AllowLocalhost7030");

app.UseCookiePolicy();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
