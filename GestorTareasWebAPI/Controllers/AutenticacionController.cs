using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Web.Http;
using GestorTareasWebAPI.DAL;
using GestorTareasWebAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace GestorTareasWebAPI.Controllers
{
    public class AutenticacionController : ApiController
    {
        private readonly string SecretKey = System.Configuration.ConfigurationManager.AppSettings["JwtSecretKey"];
        private readonly GestorTareas db = new GestorTareas();

        [HttpPost]
        [Route("api/autenticacion/login")]
        public IHttpActionResult Login(Login model)
        {
            try
            {
                // Validar si la clave secreta es válida
                if (string.IsNullOrEmpty(SecretKey) || SecretKey.Length < 32)
                {
                    throw new InvalidOperationException("La clave secreta debe tener al menos 32 caracteres.");
                }

                // Validar si el modelo es válido
                if (!ModelState.IsValid)
                    return BadRequest("Datos inválidos.");

                // Buscar el usuario en la base de datos
                var user = db.Usuarios.Include("Rol").FirstOrDefault(u => u.Correo == model.Correo);
                if (user == null)
                    return Unauthorized();

                // Verificar la contraseña usando BCrypt
                if (!BCrypt.Net.BCrypt.Verify(model.Contrasenia, user.Contrasenia))
                    return Unauthorized();

                // Generar el token JWT
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(SecretKey);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(new[]
                    {
                    new System.Security.Claims.Claim("id", user.Id.ToString())
                }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return Ok(new { token = tokenHandler.WriteToken(token) });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }

}
