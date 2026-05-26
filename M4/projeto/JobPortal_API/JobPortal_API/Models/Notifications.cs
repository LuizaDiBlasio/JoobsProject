using JobPortal_API.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal_API.Models
{
    public class Notifications
    {
        public int IdNotification { get; set; }
        [Key]
        public string UserId { get; set; }
        public string? Notification { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
 
        [ForeignKey("IdCandidato")]
        public Candidato? Candidato { get; set; }
        public int IdEmpresa { get; set; }

        [ForeignKey("IdEmpresa")]
        public Empresa? Empresa { get; set; }
    }
}