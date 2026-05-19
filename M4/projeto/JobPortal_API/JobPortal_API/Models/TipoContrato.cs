using System.ComponentModel.DataAnnotations;

namespace JobPortal_API.Models
{
    public class TipoContrato
    {
        [Key]
        public int IdTipoContrato { get; set; }

        [Required]
        public string Tipo { get; set; }    
        
    }
}
