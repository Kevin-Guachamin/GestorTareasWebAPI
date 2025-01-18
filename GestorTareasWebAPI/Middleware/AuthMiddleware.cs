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
        private static readonly string SecretKey = System.Configuration.ConfigurationManager.AppSettings["JwtSecretKey"];

        public static async Task<Usuario> Authenticate(HttpRequestMessage request, GestorTareas db)
        {
            if (!request.Headers.Contains("Authorization")) return null;

            var tokenHeader = request.Headers.GetValues("Authorization").FirstOrDefault();
            if (tokenHeader == null || !tokenHeader.StartsWith("Bearer ")) return null;

            var token = tokenHeader.Substring(7); // Quita "Bearer "
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

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                var userId = int.Parse(principal.Claims.First(c => c.Type == "id").Value);

                // Carga el usuario y su rol
                return await db.Usuarios.Include("Rol").FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<bool> IsAdmin(Usuario user)
        {
            // Validación para evitar NullReferenceException
            if (user == null || user.Rol == null)
                return false;

            return user.Rol.Nombre == "Administrador";
        }

        public static async Task<bool> IsMember(Usuario user)
        {
            // Validación para evitar NullReferenceException
            if (user == null || user.Rol == null)
                return false;

            return user.Rol.Nombre == "Miembro";
        }
    }

}
