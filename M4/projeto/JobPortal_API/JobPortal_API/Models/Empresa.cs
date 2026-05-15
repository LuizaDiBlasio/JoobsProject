using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal_API.Models
{
    public class Empresa
    {
        [Key]
        public int IdEmpresa { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public string UserId { get; set; }
       
        public ApplicationUser User { get; set; }

        public string Nome { get; set; }


        //public string? Localidade { get; set; } - normalização da tabela

        [Key]
        public int IdLocalidade { get; set; } 

        public Localidade Localidade { get; set; }  

        public string Email { get; set; }

        public int? Telefone { get; set; }

        public int? NoFuncionarios { get; set; }

        public string? ZonaAtuacao { get; set; }

        public string? LinkedIn { get; set; }

        public string? Facebook { get; set; }

        public virtual ICollection<LogoEmpresa>? LogoEmpresa { get; set; }
        public virtual ICollection<OfertaEmprego>? OfertaEmprego { get; set; }
    }
}
