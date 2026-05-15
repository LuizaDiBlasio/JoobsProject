using System.ComponentModel.DataAnnotations;

namespace JobPortal_API.Models
{
    public class Localidade
    {
        [Key]
        public int IdLocalidade { get; set; }

        [Required]
        public string NomeLocalidade { get; set; }  
    }
}
