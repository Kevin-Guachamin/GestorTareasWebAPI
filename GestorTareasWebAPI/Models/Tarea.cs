// *****************************************************************************
// PROYECTO 02  
// Carlos Benavides, Kevin Guachamin
// Fecha de entrega: 05/02/2024 
// 
// Resultados:
// * La clase `Tarea` define la estructura de las tareas en la base de datos, 
//   estableciendo validaciones en los campos de título, descripción, estado y fecha límite.
// * Se implementa una validación personalizada `FutureDateAttribute` para garantizar 
//   que las tareas tengan una fecha límite en el futuro.
//
// Conclusiones:
// * La inclusión de validaciones en el modelo de `Tarea` mejora la calidad de los datos 
//   y evita la inserción de información inconsistente en la base de datos.
// * La relación entre `Tarea` y `Usuario` permite asignar responsabilidades de manera clara,
//   facilitando la gestión de tareas en el sistema.
//
// Recomendaciones:
// * Se recomienda extender la validación del campo `Estado` para permitir configuraciones dinámicas 
//   en lugar de valores fijos, mejorando la flexibilidad del sistema.
// * Es aconsejable implementar un mecanismo de notificaciones para alertar a los usuarios 
//   cuando sus tareas están próximas a la fecha límite.
// *****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestorTareasWebAPI.Models
{
    public class Tarea
    {
        public int Id { get; set; }

        // Validación del campo Título: obligatorio, máximo 100 caracteres
        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(100, ErrorMessage = "El título no puede superar los 100 caracteres.")]
        public string Titulo { get; set; }

        // Validación del campo Descripción: obligatorio, máximo 500 caracteres
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string Descripcion { get; set; }

        // Validación del estado: obligatorio, valores permitidos "Pendiente", "En progreso" o "Completada"
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [RegularExpression(@"^(Pendiente|En progreso|Completada)$", ErrorMessage = "El estado debe ser 'Pendiente', 'En progreso' o 'Completada'.")]
        public string Estado { get; set; } // Estados válidos

        // Validación del campo Fecha Límite: obligatorio y debe ser una fecha futura
        [Required(ErrorMessage = "La fecha límite es obligatoria.")]
        [DataType(DataType.DateTime)]
        [FutureDate(ErrorMessage = "La fecha límite debe ser una fecha futura.")]
        public DateTime FechaLimite { get; set; }

        // Relación con la tabla Usuario (clave foránea)
        [ForeignKey("Usuario")]
        [Required(ErrorMessage = "El UsuarioId es obligatorio.")]
        public int UsuarioId { get; set; }

        // Propiedad de navegación para la relación con la clase Usuario
        public Usuario Usuario { get; set; }
    }

    // Validación personalizada para asegurarse de que la fecha límite sea futura
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime fecha && fecha < DateTime.Now)
            {
                return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}
