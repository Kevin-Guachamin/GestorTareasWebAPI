using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using GestorTareasWebAPI.DAL;
using GestorTareasWebAPI.Middleware;
using GestorTareasWebAPI.Models;

namespace GestorTareasWebAPI.Controllers
{
    public class UsuariosController : ApiController
    {
        private readonly GestorTareas db = new GestorTareas();

        [HttpGet]
        public async Task<IHttpActionResult> GetUsuarios()
        {
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null || !await AuthMiddleware.IsAdmin(user))
                return Unauthorized();

            return Ok(db.Usuarios.Include("Rol").ToList());
        }

        [HttpGet]
        [Route("api/usuarios/{id}")]
        public async Task<IHttpActionResult> GetUsuario(int id)
        {
            // Autenticación del usuario y verificación de permisos
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null || !await AuthMiddleware.IsAdmin(user))
                return Unauthorized();

            // Buscar el usuario por ID
            var usuario = await db.Usuarios.Include("Rol").FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }


        [HttpPost]
        public async Task<IHttpActionResult> PostUsuario(Usuario usuario)
        {
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null || !await AuthMiddleware.IsAdmin(user))
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

            // Hashear la contraseña antes de guardar
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



        [HttpPut]
        public async Task<IHttpActionResult> PutUsuario(int id, Usuario usuario)
        {
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null || !await AuthMiddleware.IsAdmin(user))
                return Unauthorized();

            if (!ModelState.IsValid || id != usuario.Id)
                return BadRequest();

            db.Entry(usuario).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        [HttpDelete]
        public async Task<IHttpActionResult> DeleteUsuario(int id)
        {
            var user = await AuthMiddleware.Authenticate(Request, db);
            if (user == null || !await AuthMiddleware.IsAdmin(user))
                return Unauthorized();

            var usuario = db.Usuarios.Find(id);
            if (usuario == null)
                return NotFound();

            db.Usuarios.Remove(usuario);
            db.SaveChanges();
            return Ok(usuario);
        }
    }
}
