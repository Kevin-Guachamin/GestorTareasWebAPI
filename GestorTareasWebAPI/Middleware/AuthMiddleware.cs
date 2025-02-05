// *****************************************************************************
// PROYECTO 02  
// Carlos Benavides, Kevin Guachamin
// Fecha de entrega: 05/02/2024 
// 
// Resultados:
// * El middleware `AuthMiddleware` permite la autenticación de usuarios mediante tokens JWT, 
//   verificando la validez del token y extrayendo la información del usuario autenticado.
// * La validación de roles mediante los métodos `IsAdmin` e `IsMember` garantiza el acceso 
//   restringido a funcionalidades específicas de la API.
//
// Conclusiones:
// * La implementación de autenticación basada en JWT proporciona un mecanismo seguro y escalable 
//   para gestionar sesiones en la API sin necesidad de almacenar información en el servidor.
// * La inclusión de validaciones de rol en el middleware permite establecer controles de acceso 
//   eficientes y evitar vulnerabilidades relacionadas con privilegios.
//
// Recomendaciones:
// * Se recomienda incluir una validación adicional en `Authenticate` para manejar tokens expirados, 
//   asegurando que la respuesta al cliente indique el motivo de la invalidación del token.
// * Es aconsejable agregar un mecanismo de revocación de tokens para mejorar la seguridad en caso 
//   de que un token comprometido deba ser invalidado antes de su expiración.
// *****************************************************************************

using System;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GestorTareasWebAPI.DAL;
using GestorTareasWebAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace GestorTareasWebAPI.Middleware
{
    public static class AuthMiddleware
    {
        // Clave secreta para la firma del token, obtenida de la configuración
        private static readonly string SecretKey = System.Configuration.ConfigurationManager.AppSettings["JwtSecretKey"];

        // Método para autenticar al usuario mediante JWT
        public static async Task<Usuario> Authenticate(HttpRequestMessage request, GestorTareas db)
        {
            // Verifica si la cabecera "Authorization" está presente
            if (!request.Headers.Contains("Authorization")) return null;

            var tokenHeader = request.Headers.GetValues("Authorization").FirstOrDefault();
            if (tokenHeader == null || !tokenHeader.StartsWith("Bearer ")) return null;

            // Extrae el token JWT quitando el prefijo "Bearer "
            var token = tokenHeader.Substring(7);
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(SecretKey);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                // Valida el token y extrae la identidad del usuario
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                var userId = int.Parse(principal.Claims.First(c => c.Type == "id").Value);

                // Busca el usuario en la base de datos junto con su rol
                return await db.Usuarios.Include("Rol").FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch
            {
                return null; // Retorna null si la validación del token falla
            }
        }

        // Método para verificar si el usuario tiene el rol de Administrador
        public static async Task<bool> IsAdmin(Usuario user)
        {
            // Verificación para evitar NullReferenceException
            if (user == null || user.Rol == null)
                return false;

            return user.Rol.Nombre == "Administrador";
        }

        // Método para verificar si el usuario tiene el rol de Miembro
        public static async Task<bool> IsMember(Usuario user)
        {
            // Verificación para evitar NullReferenceException
            if (user == null || user.Rol == null)
                return false;

            return user.Rol.Nombre == "Miembro";
        }
    }
}
