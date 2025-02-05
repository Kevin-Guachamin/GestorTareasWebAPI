// *****************************************************************************
// PROYECTO 02  
// Carlos Benavides, Kevin Guachamin
// Fecha de entrega: 05/02/2024 
// 
// Resultados:
// * El controlador `TareasController` permite la gestión de tareas a través de operaciones CRUD,
//   asegurando que los administradores puedan gestionar todas las tareas, mientras que los miembros
//   solo pueden ver y actualizar las suyas.
// * La autenticación y autorización se manejan mediante `AuthMiddleware`, restringiendo correctamente
//   el acceso a las tareas y asegurando que los usuarios solo realicen acciones permitidas.
//
// Conclusiones:
// * La implementación de un controlador de tareas basado en Web API facilita la gestión y asignación
//   de tareas en un entorno colaborativo, asegurando que cada usuario acceda solo a la información correspondiente.
// * La restricción de permisos en cada endpoint previene modificaciones no autorizadas, mejorando la
//   seguridad y confiabilidad del sistema.
//
// Recomendaciones:
// * Se recomienda mejorar el rendimiento al manejar grandes volúmenes de datos implementando paginación en 
//   las consultas de tareas, optimizando así el tiempo de respuesta de la API.
// * Es recomendable incluir notificaciones en tiempo real para alertar a los usuarios sobre cambios en sus tareas,
//   mejorando la experiencia del usuario y la eficiencia del sistema.
// *****************************************************************************

using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using GestorTareasWebAPI.DAL;
using GestorTareasWebAPI.Middleware;
using GestorTareasWebAPI.Models;

namespace GestorTareasWebAPI.Controllers
{
    public class TareasController : ApiController
    {
        private readonly GestorTareas db = new GestorTareas();

        // Habilita CORS para permitir peticiones desde el frontend
        [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]

        // Obtiene todas las tareas si el usuario es administrador o solo las tareas del usuario autenticado si es miembro
        [HttpGet]
        [Route("api/tareas")]
        public async Task<IHttpActionResult> GetTareas()
        {
            // Autenticar usuario
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null)
                return Unauthorized();

            // Si el usuario es administrador, obtiene todas las tareas
            if (await AuthMiddleware.IsAdmin(user))
            {
                var tareas = db.Tareas.Include(t => t.Usuario)
                    .Select(t => new
                    {
                        t.Id,
                        t.Titulo,
                        t.Descripcion,
                        t.Estado,
                        t.FechaLimite,
                        t.UsuarioId,
                        Usuario = new
                        {
                            t.Usuario.Nombre,
                            t.Usuario.Apellido,
                            t.Usuario.Correo
                        }
                    }).ToList();

                return Ok(tareas);
            }
            // Si el usuario es miembro, solo obtiene sus propias tareas
            else if (await AuthMiddleware.IsMember(user))
            {
                var tareas = db.Tareas.Include(t => t.Usuario)
                    .Where(t => t.UsuarioId == user.Id)
                    .Select(t => new
                    {
                        t.Id,
                        t.Titulo,
                        t.Descripcion,
                        t.Estado,
                        t.FechaLimite,
                        t.UsuarioId,
                        Usuario = new
                        {
                            t.Usuario.Nombre,
                            t.Usuario.Apellido,
                            t.Usuario.Correo
                        }
                    }).ToList();

                return Ok(tareas);
            }

            return Unauthorized();
        }

        // Obtiene una tarea específica si el usuario es administrador o si la tarea pertenece al usuario autenticado
        [HttpGet]
        [Route("api/tareas/{id}")]
        public async Task<IHttpActionResult> GetTarea(int id)
        {
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null)
                return Unauthorized();

            // Busca la tarea en la base de datos
            var tarea = db.Tareas.Include(t => t.Usuario)
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.Titulo,
                    t.Descripcion,
                    t.Estado,
                    t.FechaLimite,
                    t.UsuarioId,
                    Usuario = new
                    {
                        t.Usuario.Nombre,
                        t.Usuario.Apellido,
                        t.Usuario.Correo
                    }
                })
                .FirstOrDefault();

            if (tarea == null)
                return NotFound();

            // Solo un administrador o el dueño de la tarea puede acceder a ella
            if (await AuthMiddleware.IsAdmin(user) || (await AuthMiddleware.IsMember(user) && tarea.UsuarioId == user.Id))
            {
                return Ok(tarea);
            }

            return Unauthorized();
        }

        // Crea una nueva tarea (Solo Administradores pueden hacerlo)
        [HttpPost]
        [Route("api/tareas")]
        public async Task<IHttpActionResult> CrearTarea(Tarea tarea)
        {
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null || !await AuthMiddleware.IsAdmin(user))
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            db.Tareas.Add(tarea);
            db.SaveChanges();

            var locationUrl = $"{Request.RequestUri}/{tarea.Id}";
            return Created(locationUrl, tarea);
        }

        // Modifica una tarea existente. Administradores pueden modificar todo, los miembros solo el estado.
        [HttpPut]
        [Route("api/tareas/{id}")]
        public async Task<IHttpActionResult> UpdateTarea(int id, Tarea tarea)
        {
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null)
                return Unauthorized();

            var existingTarea = db.Tareas.Include(t => t.Usuario).FirstOrDefault(t => t.Id == id);
            if (existingTarea == null)
                return NotFound();

            // Si es administrador, puede modificar cualquier campo de la tarea
            if (await AuthMiddleware.IsAdmin(user))
            {
                existingTarea.Titulo = tarea.Titulo;
                existingTarea.Descripcion = tarea.Descripcion;
                existingTarea.Estado = tarea.Estado;
                existingTarea.FechaLimite = tarea.FechaLimite;
                existingTarea.UsuarioId = tarea.UsuarioId;
            }
            // Si es miembro, solo puede modificar el estado de la tarea
            else if (await AuthMiddleware.IsMember(user) && existingTarea.UsuarioId == user.Id)
            {
                existingTarea.Estado = tarea.Estado;
            }
            else
            {
                return Unauthorized();
            }

            db.Entry(existingTarea).State = EntityState.Modified;
            db.SaveChanges();

            var tareaProyectada = new
            {
                existingTarea.Id,
                existingTarea.Titulo,
                existingTarea.Descripcion,
                existingTarea.Estado,
                existingTarea.FechaLimite,
                existingTarea.UsuarioId,
                Usuario = new
                {
                    existingTarea.Usuario.Nombre,
                    existingTarea.Usuario.Apellido,
                    existingTarea.Usuario.Correo
                }
            };

            return Ok(tareaProyectada);
        }

        // Elimina una tarea (Solo Administradores pueden hacerlo)
        [HttpDelete]
        [Route("api/tareas/{id}")]
        public async Task<IHttpActionResult> DeleteTarea(int id)
        {
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null || !await AuthMiddleware.IsAdmin(user))
                return Unauthorized();

            var tarea = db.Tareas.Include(t => t.Usuario).FirstOrDefault(t => t.Id == id);
            if (tarea == null)
                return NotFound();

            db.Tareas.Remove(tarea);
            db.SaveChanges();

            var tareaProyectada = new
            {
                tarea.Id,
                tarea.Titulo,
                tarea.Descripcion,
                tarea.Estado,
                tarea.FechaLimite,
                tarea.UsuarioId,
                Usuario = new
                {
                    tarea.Usuario.Nombre,
                    tarea.Usuario.Apellido,
                    tarea.Usuario.Correo
                }
            };

            return Ok(tareaProyectada);
        }
    }
}
