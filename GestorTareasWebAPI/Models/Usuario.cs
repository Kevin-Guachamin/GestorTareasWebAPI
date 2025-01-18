using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestorTareasWebAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Apellido { get; set; }

        [Required, EmailAddress]
        public string Correo { get; set; }

        [Required]
        public string Contrasenia { get; set; }

        [ForeignKey("Rol")]
        public int RolId { get; set; }

        public Rol Rol { get; set; }
    }
}
