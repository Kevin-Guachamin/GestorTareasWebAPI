using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using GestorTareasWebAPI.DAL;
using GestorTareasWebAPI.Middleware;
using GestorTareasWebAPI.Models;

namespace GestorTareasWebAPI.Controllers
{
    public class TareasController : ApiController
    {
        private readonly GestorTareas db = new GestorTareas();

        // GET: /api/tareas (Administrador obtiene todas las tareas, Miembro solo las suyas)
        [HttpGet]
        [Route("api/tareas")]
        public async Task<IHttpActionResult> GetTareas()
        {
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null)
                return Unauthorized();

            if (await AuthMiddleware.IsAdmin(user))
            {
                // Incluye explícitamente UsuarioId junto con los datos del usuario
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
            else if (await AuthMiddleware.IsMember(user))
            {
                // Miembro puede obtener solo sus tareas
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

        // GET: /api/tareas/{id} (Administrador y Miembro pueden ver una tarea específica asignada)
        [HttpGet]
        [Route("api/tareas/{id}")]
        public async Task<IHttpActionResult> GetTarea(int id)
        {
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null)
                return Unauthorized();

            // Incluye explícitamente UsuarioId junto con los datos del usuario
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

            if (await AuthMiddleware.IsAdmin(user) || (await AuthMiddleware.IsMember(user) && tarea.UsuarioId == user.Id))
            {
                return Ok(tarea);
            }

            return Unauthorized();
        }



        // POST: /api/tareas (Solo Administrador puede crear tareas)
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

            // Devuelve el recurso creado sin usar "DefaultApi"
            var locationUrl = $"{Request.RequestUri}/{tarea.Id}";
            return Created(locationUrl, tarea);
        }

        // PUT: /api/tareas/{id} (Administrador puede editar todo, Miembro solo el estado)
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

            if (await AuthMiddleware.IsAdmin(user))
            {
                // Administrador puede editar cualquier campo
                existingTarea.Titulo = tarea.Titulo;
                existingTarea.Descripcion = tarea.Descripcion;
                existingTarea.Estado = tarea.Estado;
                existingTarea.FechaLimite = tarea.FechaLimite;
                existingTarea.UsuarioId = tarea.UsuarioId;
            }
            else if (await AuthMiddleware.IsMember(user) && existingTarea.UsuarioId == user.Id)
            {
                // Miembro solo puede cambiar el estado de la tarea
                existingTarea.Estado = tarea.Estado;
            }
            else
            {
                return Unauthorized();
            }

            db.Entry(existingTarea).State = EntityState.Modified;
            db.SaveChanges();

            // Proyecta la tarea con los atributos solicitados
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

        // DELETE: /api/tareas/{id} (Solo Administrador puede eliminar tareas)
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

            // Proyecta la tarea eliminada con los atributos solicitados
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

        // GET: /api/tareas/filter?estado={estado} (Filtra tareas por estado)
        [HttpGet]
        [Route("api/tareas/filter")]
        public async Task<IHttpActionResult> GetTareasByEstado(string estado)
        {
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null)
                return Unauthorized();

            if (await AuthMiddleware.IsAdmin(user))
            {
                var tareas = db.Tareas.Include(t => t.Usuario)
                    .Where(t => t.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase))
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
            else if (await AuthMiddleware.IsMember(user))
            {
                var tareas = db.Tareas.Include(t => t.Usuario)
                    .Where(t => t.UsuarioId == user.Id && t.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase))
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

    }
}
