using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MVC.Handlers
{
    public class JwtHelper
    {
        public static ClaimsPrincipal? GetPrincipalFromSession(HttpContext httpContext)
        {
            var token = httpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
    }
}
