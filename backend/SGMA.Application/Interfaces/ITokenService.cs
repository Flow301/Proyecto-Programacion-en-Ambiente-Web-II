using SGMA.Domain.Entities;

namespace SGMA.Application.Interfaces;

// La implementación vive en SGMA.WebAPI/Security, que ya trae el paquete JwtBearer
public interface ITokenService
{
    // El usuario debe venir con su Rol cargado: el nombre del rol va en el token
    (string Token, DateTime Expira) Generar(Usuario usuario);
}
