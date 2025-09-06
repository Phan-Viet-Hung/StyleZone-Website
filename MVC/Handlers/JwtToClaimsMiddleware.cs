using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MVC.Handlers
{
    public class JwtToClaimsMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtToClaimsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);

                    var claims = new List<Claim>();

                    foreach (var claim in jwtToken.Claims)
                    {
                        claims.Add(claim);
                    }

                    var identity = new ClaimsIdentity(claims, "jwt");
                    var principal = new ClaimsPrincipal(identity);

                    // 👇 Gắn vào HttpContext.User để MVC có thể dùng [Authorize]
                    context.User = principal;
                }
                catch
                {
                    // nếu token lỗi => bỏ qua
                }
            }

            await _next(context);
        }
    }
}
