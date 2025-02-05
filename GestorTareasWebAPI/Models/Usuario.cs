// *****************************************************************************
// PROYECTO 02  
// Carlos Benavides, Kevin Guachamin
// Fecha de entrega: 05/02/2024 
// 
// Resultados:
// * La clase `Usuario` define la estructura de los usuarios en la base de datos, incluyendo 
//   validaciones para los campos de nombre, apellido, correo y contraseña.
// * La asociación con la clase `Rol` permite establecer relaciones entre los usuarios y 
//   sus roles dentro del sistema.
//
// Conclusiones:
// * La implementación de validaciones en los atributos del modelo mejora la integridad de los datos, 
//   evitando registros incorrectos en la base de datos.
// * La relación entre `Usuario` y `Rol` permite gestionar permisos de acceso de manera estructurada, 
//   facilitando el control de usuarios en el sistema.
//
// Recomendaciones:
// * Se recomienda encriptar la contraseña antes de almacenarla en la base de datos para 
//   mejorar la seguridad y evitar el almacenamiento de contraseñas en texto plano.
// * Es aconsejable definir una longitud máxima para la contraseña en el modelo para 
//   prevenir ataques de desbordamiento y mejorar la gestión de credenciales en el sistema.
// *****************************************************************************

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

        // Validación del campo Nombre: obligatorio, solo letras y espacios, máximo 50 caracteres
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        public string Nombre { get; set; }

        // Validación del campo Apellido: obligatorio, solo letras y espacios, máximo 50 caracteres
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios.")]
        [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
        public string Apellido { get; set; }

        // Validación del campo Correo: obligatorio, formato de correo válido, máximo 100 caracteres
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
        public string Correo { get; set; }

        // Validación del campo Contraseña: obligatorio, mínimo 6 caracteres
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Contrasenia { get; set; }

        // Relación con la tabla Rol (clave foránea)
        [ForeignKey("Rol")]
        [Required(ErrorMessage = "El RolId es obligatorio.")]
        public int RolId { get; set; }

        // Propiedad de navegación para la relación con la clase Rol
        public Rol Rol { get; set; }
    }
}
