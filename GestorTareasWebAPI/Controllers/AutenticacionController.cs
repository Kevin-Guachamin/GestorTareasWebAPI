// *****************************************************************************
// PROYECTO 02  
// Carlos Benavides, Kevin Guachamin
// Fecha de entrega: 05/02/2024 
// 
// Resultados:
// * El controlador `AutenticacionController` permite autenticar a los usuarios y generar un token JWT 
//   que se devuelve en la respuesta junto con el rol del usuario.
// * La validación de credenciales se realiza utilizando BCrypt para verificar la contraseña almacenada,
//   garantizando un proceso de autenticación seguro.
//
// Conclusiones:
// * La implementación de autenticación mediante JWT proporciona un mecanismo seguro para la gestión 
//   de sesiones en la aplicación, permitiendo autenticación sin almacenamiento de sesiones en el servidor.
// * El uso de BCrypt para almacenar y verificar contraseñas mejora la seguridad del sistema, evitando 
//   el almacenamiento de contraseñas en texto plano.
//
// Recomendaciones:
// * Se recomienda incluir información del rol en los claims del token JWT para que el cliente pueda 
//   validar permisos sin hacer múltiples solicitudes al servidor.
// * Es recomendable manejar un tiempo de expiración adecuado en el token JWT y considerar 
//   la implementación de un sistema de refresco de tokens para mejorar la seguridad y experiencia de usuario.
// *****************************************************************************

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using GestorTareasWebAPI.DAL;
using GestorTareasWebAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace GestorTareasWebAPI.Controllers
{
    public class AutenticacionController : ApiController
    {
        // Clave secreta para firmar el token JWT, obtenida de la configuración
        private readonly string SecretKey = System.Configuration.ConfigurationManager.AppSettings["JwtSecretKey"];
        private readonly GestorTareas db = new GestorTareas();

        [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]

        // POST: /api/autenticacion/login (Autenticar usuario y generar token JWT)
        [HttpPost]
        [Route("api/autenticacion/login")]
        public IHttpActionResult Login(Login model)
        {
            try
            {
                // Verificar que la clave secreta tenga el tamaño adecuado
                if (string.IsNullOrEmpty(SecretKey) || SecretKey.Length < 32)
                {
                    throw new InvalidOperationException("La clave secreta debe tener al menos 32 caracteres.");
                }

                // Validar que el modelo recibido es válido
                if (!ModelState.IsValid)
                    return BadRequest("Datos inválidos.");

                // Buscar el usuario en la base de datos por su correo electrónico
                var user = db.Usuarios.Include("Rol").FirstOrDefault(u => u.Correo == model.Correo);
                if (user == null)
                    return Unauthorized();

                // Verificar la contraseña con BCrypt
                if (!BCrypt.Net.BCrypt.Verify(model.Contrasenia, user.Contrasenia))
                    return Unauthorized();

                // Crear el manejador del token JWT
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(SecretKey);

                // Configurar los datos del token sin incluir el rol
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(new[]
                    {
                        new System.Security.Claims.Claim("id", user.Id.ToString()), // Identificador del usuario
                    }),
                    Expires = DateTime.UtcNow.AddDays(7), // Duración del token
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                // Generar el token y convertirlo a string
                var token = tokenHandler.CreateToken(tokenDescriptor);
                string tokenString = tokenHandler.WriteToken(token);

                // Retornar el token y el rol del usuario en la respuesta
                return Ok(new
                {
                    token = tokenString,
                    rol = user.Rol.Nombre // Se devuelve el rol, pero no se incluye en el token
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
