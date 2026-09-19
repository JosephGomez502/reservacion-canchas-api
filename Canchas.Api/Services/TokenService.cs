using Canchas.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Canchas.Api.Services
{
    public class TokenResultado
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
    }

    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenResultado GenerarToken(
            Usuario usuario,
            IEnumerable<string> roles)
        {
            var key = _configuration["JwtSettings:Key"]
                ?? throw new InvalidOperationException(
                    "No se encontró JwtSettings:Key.");

            var issuer = _configuration["JwtSettings:Issuer"]
                ?? throw new InvalidOperationException(
                    "No se encontró JwtSettings:Issuer.");

            var audience = _configuration["JwtSettings:Audience"]
                ?? throw new InvalidOperationException(
                    "No se encontró JwtSettings:Audience.");

            var expirationMinutes = int.Parse(
                _configuration["JwtSettings:ExpirationMinutes"] ?? "60"
            );

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    usuario.NombreCompleto
                ),

                new Claim(
                    ClaimTypes.Email,
                    usuario.Correo
                ),

                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()
                )
            };

            foreach (var rol in roles)
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        rol
                    )
                );
            }

            var securityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key)
                );

            var credentials =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256
                );

            var expiration =
                DateTime.UtcNow.AddMinutes(expirationMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            return new TokenResultado
            {
                Token = new JwtSecurityTokenHandler()
                    .WriteToken(token),

                ExpiraEn = expiration
            };
        }
    }
}