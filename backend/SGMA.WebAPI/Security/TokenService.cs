using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SGMA.Application.Interfaces;
using SGMA.Domain.Entities;

namespace SGMA.WebAPI.Security;

/// <summary>
/// Genera el JWT del login. Clave en User Secrets (Jwt:Key); emisor, audiencia y duración en appsettings.json.
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime Expira) Generar(Usuario usuario)
    {
        var expira = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpiraMinutos"));
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        // Claims con nombre corto (sub, name, role): Program.cs los lee así, sin mapearlos
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, usuario.Login),
                new Claim(ClaimsPrincipalExtensions.RoleClaim, usuario.Rol.Nombre)
            ]),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            Expires = expira,
            SigningCredentials = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256)
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expira);
    }
}
