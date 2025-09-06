using System.Net.Http.Headers;
using API.DomainCusTomer.DTOs.MoMo;
using API.DomainCusTomer.DTOs.MomocustomerId;
using API.DomainCusTomer.Services;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using MVC.Handlers;


internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
        builder.Services.AddScoped<IMomoService, MomoServicer>();

        builder.Services.Configure<MomoOptionModelId>(builder.Configuration.GetSection("MomoAPI_Customer"));
        builder.Services.AddScoped<IMomoCustomerIdServices, MomoCustomerIdServices>();
        //builder.Services.AddScoped<ICartCustomerService, CartCustomerService>();
        builder.Services.AddAuthentication(options =>
        {

            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddGoogle(options =>
        {
            options.ClientId = builder.Configuration["GoogleKeys:ClientId"];
            options.ClientSecret = builder.Configuration["GoogleKeys:ClientSecret"];
            options.CallbackPath = "/signin-google";

            options.Events = new OAuthEvents
            {
                OnRemoteFailure = context =>
                {
                    // ?? Redirect v? Home/Index kèm error
                    context.Response.Redirect("/Home/Index?error=" + Uri.EscapeDataString(context.Failure?.Message ?? "unknown"));
                    context.HandleResponse();
                    return Task.CompletedTask;
                }
            };
        });

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddTransient<AuthHeaderHandler>();
        builder.Services.AddHttpClient();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            options.Cookie.Name = ".StyleZone.CustomerSession";
        });
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7257/api/"); // ??a ch? base c?a API
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }).AddHttpMessageHandler<AuthHeaderHandler>();
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

        });
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(180);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        builder.Services.AddHttpContextAccessor();

        var app = builder.Build();


        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");

            app.UseHsts();
            app.UseDeveloperExceptionPage();
        }


        app.UseHttpsRedirection();
        app.Use(async (context, next) =>
        {
            context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";
            await next();
        });

        app.UseStaticFiles();
        app.UseSession();
        app.UseRouting();
        app.UseSession();
        app.UseAuthorization();
        app.MapControllerRoute(
            name: "areas",
        pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        //app.MapControllerRoute(
        //    name: "default",
        //    pattern: "{controller=TheThaoCustomer}/{action=TheThaoCustomer}/{id?}");
        app.Run();
    }
}