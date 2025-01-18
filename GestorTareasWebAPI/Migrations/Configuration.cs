namespace GestorTareasWebAPI.Migrations
{
    using GestorTareasWebAPI.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<GestorTareasWebAPI.DAL.GestorTareas>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(GestorTareasWebAPI.DAL.GestorTareas context)
        {
            Console.WriteLine("Ejecutando Seed...");

            // Crear roles predeterminados
            context.Roles.AddOrUpdate(r => r.Nombre,
                new Rol { Nombre = "Administrador" },
                new Rol { Nombre = "Miembro" }
            );

            Console.WriteLine("Roles creados o actualizados.");

            // Crear un administrador predeterminado si no existe
            if (!context.Usuarios.Any(u => u.Correo == "admin@example.com"))
            {
                var rolAdmin = context.Roles.FirstOrDefault(r => r.Nombre == "Administrador");
                if (rolAdmin != null)
                {
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin123"); // Contraseña encriptada

                    context.Usuarios.AddOrUpdate(new Usuario
                    {
                        Nombre = "Admin",
                        Apellido = "Principal",
                        Correo = "admin@example.com",
                        Contrasenia = hashedPassword, // Contraseña encriptada
                        RolId = rolAdmin.Id
                    });

                    Console.WriteLine("Usuario administrador creado.");
                }
                else
                {
                    Console.WriteLine("Error: No se encontró el rol 'Administrador'.");
                }
            }

            base.Seed(context);
        }



    }
}
