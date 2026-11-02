using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace SGMA.WebAPI.Security;

public static class ClaimsPrincipalExtensions
{
    public const string RoleClaim = "role";

    // Usuario responsable de la acción. Solo se llama en acciones protegidas, así que el claim siempre está
    public static int GetIdUsuario(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
