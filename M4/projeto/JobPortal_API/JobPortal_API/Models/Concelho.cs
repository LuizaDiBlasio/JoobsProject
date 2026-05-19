using System.ComponentModel.DataAnnotations;

namespace JobPortal_API.Models
{
    public class Concelho
    {
        [Key]
        public int IdConcelho { get; set; }
        
        [Required]
        public string NomeConcelho { get; set; }

       
    }
}
