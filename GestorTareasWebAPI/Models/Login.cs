using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace GestorTareasWebAPI.Models
{
    public class Login
    {
        [Required, EmailAddress]
        public string Correo { get; set; }

        [Required]
        public string Contrasenia { get; set; }
    }
}
