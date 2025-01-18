using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using GestorTareasWebAPI.Models;

namespace GestorTareasWebAPI.DAL
{
    public class GestorTareas : DbContext
    {
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Tarea> Tareas { get; set; }

        public GestorTareas() : base("GestorTareas") { }
    }
}
