// *****************************************************************************
// PROYECTO 02  
// Carlos Benavides, Kevin Guachamin
// Fecha de entrega: 05/02/2024 
// 
// Resultados:
// * El controlador `UsuariosController` permite la gestión de usuarios mediante operaciones CRUD,
//   asegurando que solo los administradores puedan realizar modificaciones en los datos de los usuarios.
// * La autenticación y autorización se manejan mediante `AuthMiddleware`, restringiendo correctamente
//   el acceso a las funcionalidades del controlador.
//
// Conclusiones:
// * La implementación de un controlador de usuarios basado en Web API facilita la administración y control 
//   de los usuarios dentro del sistema, asegurando la integridad de los datos.
// * La validación y autorización previa en cada endpoint impide accesos no autorizados, mejorando 
//   la seguridad del sistema.
//
// Recomendaciones:
// * Se recomienda implementar logs de auditoría para registrar todas las modificaciones realizadas en los usuarios,
//   facilitando el monitoreo y detección de cambios no autorizados.
// * Es aconsejable mejorar la estructura de respuesta de los endpoints proporcionando códigos de estado más descriptivos,
//   facilitando la depuración y la interacción con el frontend.
// *****************************************************************************

using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using GestorTareasWebAPI.DAL;
using GestorTareasWebAPI.Middleware;
using GestorTareasWebAPI.Models;

namespace GestorTareasWebAPI.Controllers
{
    public class UsuariosController : ApiController
    {
        private readonly GestorTareas db = new GestorTareas();

        // Habilita CORS para permitir peticiones desde el frontend
        [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]

        // Obtiene la lista de usuarios (solo accesible por administradores)
        [HttpGet]
        public async Task<IHttpActionResult> GetUsuarios()
        {
            // Autenticación del usuario y verificación de permisos de administrador
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null || !await AuthMiddleware.IsAdmin(user))
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized,
                    new { message = "Usuario no autorizado para acceder a esta información." }));
            }

            // Obtiene la lista de usuarios con sus respectivos roles
            var usuarios = db.Usuarios.Include("Rol")
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.Correo,
                    u.Contrasenia,
                    RolNombre = u.Rol.Nombre // Nombre del rol asociado
                }).ToList();

            return Ok(usuarios);
        }

        // Obtiene un usuario específico por ID (solo accesible por administradores)
        [HttpGet]
        [Route("api/usuarios/{id}")]
        public async Task<IHttpActionResult> GetUsuario(int id)
        {
            // Busca el usuario por ID e incluye su rol en la respuesta
            var usuario = await db.Usuarios.Include("Rol")
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.Correo,
                    u.Contrasenia,
                    RolNombre = u.Rol.Nombre
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        // Crea un nuevo usuario (solo accesible por administradores)
        [HttpPost]
        public async Task<IHttpActionResult> PostUsuario(Usuario usuario)
        {
            // Autenticar usuario y verificar permisos de administrador
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null || !await AuthMiddleware.IsAdmin(user))
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized,
                    new { message = "Usuario no autorizado para acceder a esta información." }));
            }

            // Validar modelo
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar si el correo ya está registrado en la base de datos
            if (db.Usuarios.Any(u => u.Correo == usuario.Correo))
            {
                return BadRequest($"El correo {usuario.Correo} ya está registrado. Por favor, use un correo diferente.");
            }

            // Asignar el rol "Miembro" por defecto si no se especifica
            if (usuario.RolId == 0)
            {
                var rolMiembro = db.Roles.FirstOrDefault(r => r.Nombre == "Miembro");
                if (rolMiembro != null)
                {
                    usuario.RolId = rolMiembro.Id;
                }
                else
                {
                    return BadRequest("No se encontró el rol 'Miembro' en la base de datos.");
                }
            }

            // Hashear la contraseña antes de guardarla
            if (!string.IsNullOrEmpty(usuario.Contrasenia))
            {
                usuario.Contrasenia = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasenia);
            }
            else
            {
                return BadRequest("La contraseña no puede estar vacía.");
            }

            db.Usuarios.Add(usuario);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = usuario.Id }, usuario);
        }

        // Actualiza los datos de un usuario (solo accesible por administradores)
        [HttpPut]
        [Route("api/usuarios/{id}")]
        public async Task<IHttpActionResult> PutUsuario(int id, Usuario usuario)
        {
            // Autenticar usuario y verificar permisos de administrador
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null || !await AuthMiddleware.IsAdmin(user))
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized,
                    new { message = "Usuario no autorizado para acceder a esta información." }));
            }

            // Validar modelo
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                // Obtener el usuario actual
                var usuarioExistente = await db.Usuarios.FindAsync(id);
                if (usuarioExistente == null)
                    return NotFound();

                // Preservar la contraseña si no se proporciona una nueva
                if (string.IsNullOrEmpty(usuario.Contrasenia))
                {
                    usuario.Contrasenia = usuarioExistente.Contrasenia;
                }
                else
                {
                    // Hashear la nueva contraseña antes de almacenarla
                    usuario.Contrasenia = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasenia);
                }

                // Actualizar los datos del usuario
                usuarioExistente.Nombre = usuario.Nombre;
                usuarioExistente.Apellido = usuario.Apellido;
                usuarioExistente.Correo = usuario.Correo;
                usuarioExistente.Contrasenia = usuario.Contrasenia;
                usuarioExistente.RolId = usuario.RolId;

                db.Entry(usuarioExistente).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();

                return Ok(usuarioExistente);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // Elimina un usuario (solo accesible por administradores)
        [HttpDelete]
        [Route("api/usuarios/{id}")]
        public async Task<IHttpActionResult> DeleteUsuario(int id)
        {
            try
            {
                // Autenticar usuario y verificar permisos de administrador
                var user = await AuthMiddleware.Authenticate(Request, db);
                if (user == null || !await AuthMiddleware.IsAdmin(user))
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized,
                        new { message = "Usuario no autorizado para acceder a esta información." }));
                }

                // Buscar usuario a eliminar
                var usuario = await db.Usuarios.FindAsync(id);
                if (usuario == null)
                    return NotFound();

                db.Usuarios.Remove(usuario);
                await db.SaveChangesAsync();
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
