using System.ComponentModel.DataAnnotations;

namespace JobPortal_API.Models
{
    public class Escolaridade
    {
        [Key]
        public int IdEscolaridade { get; set; }

        [Required]  
        public string Tipo { get; set; }    
    }
}
