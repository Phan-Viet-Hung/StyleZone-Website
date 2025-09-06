namespace API.DomainCusTomer.DTOs.CartICustomer
{
    public static class CookieHelper
    {
        public static void Set(HttpContext ctx, string key, string value, int expireDays = 30)
        {
            var options = new CookieOptions
            {
                MaxAge = TimeSpan.FromDays(7),
                HttpOnly = false,
                Secure = false,           
                SameSite = SameSiteMode.Lax, 
                IsEssential = true,
                Path = "/"
            };

            ctx.Response.Cookies.Append(key, value, options);
        }

        public static string Get(HttpContext ctx, string key)
        {
            ctx.Request.Cookies.TryGetValue(key, out var value);
            return value;
        }

        public static void Remove(HttpContext ctx, string key)
        {
            ctx.Response.Cookies.Delete(key);
        }
    }
}
