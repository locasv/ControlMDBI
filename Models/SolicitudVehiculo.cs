﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlMDBI.Models
{
    public class SolicitudVehiculo
    {
        [Key]
        public int IdSolicitudVehiculo { get; set; }

        [Required(ErrorMessage = "El usuario que realiza la solicitud es obligatorio.")]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }
        //La subgerencia o unidad a la que pertece el empleado se asigna automaticamente porque ya esta en la tabla de empleado que tiene relacion con el usuario


        [Required(ErrorMessage = "El vehiculo es obligatorio.")]
        public int IdVehiculo { get; set; }
        [ForeignKey("IdVehiculo")]
        public Vehiculo? Vehiculo { get; set; }

        [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
        [DataType(DataType.DateTime, ErrorMessage = "La fecha de salida no es válida.")]
        //que muestre la hora en pm y am
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm tt}", ApplyFormatInEditMode = false)]
        [Display(Name = "Fecha de Salida")]
        public DateTime FechaSalida { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La fecha de regreso es obligatoria.")]
        [DataType(DataType.DateTime, ErrorMessage = "La fecha de regreso no es válida.")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm tt}", ApplyFormatInEditMode = false)]
        [Display(Name = "Fecha de Regreso")]
        public DateTime FechaRegreso { get; set; } =  DateTime.Now;

        [Required(ErrorMessage = "El recorrido de la solicitud es obligatorio.")]
        [StringLength(200, ErrorMessage = "El recorrido no puede exceder los {1} caracteres.")]
        public string Recorrido { get; set; }


        [Required(ErrorMessage = "La fecha de solicitud es obligatoria.")]
        [DataType(DataType.DateTime, ErrorMessage = "La fecha de solicitud no es válida.")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm tt}", ApplyFormatInEditMode = false)]
        [Display(Name = "Fecha de Solicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;


        public ICollection<AprobacionVehiculo>? AprobacionVehiculos { get; set; }

    }
}
