using DAL_Empty.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.DomainCusTomer.Config
{
    public class JwtTokenHelper
    {
        private readonly JwtSettings _jwt;
        public JwtTokenHelper(IOptions<JwtSettings> options) => _jwt = options.Value;

        public string GenerateToken(Customer acc)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, acc.UserName!),
                new Claim(ClaimTypes.Email, acc.Email!),

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
