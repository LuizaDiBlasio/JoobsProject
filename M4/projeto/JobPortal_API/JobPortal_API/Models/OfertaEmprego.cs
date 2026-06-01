using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortal_API.Models.Enums;

namespace JobPortal_API.Models
{
    public class OfertaEmprego
    {
        [Key]
        public int IdOferta { get; set; }

        public int IdEmpresa { get; set; }

        [ForeignKey("IdEmpresa")]
        public Empresa Empresa { get; set; }

        public string Titulo { get; set; }

        public float? Salario { get; set; }


        //public string? Localização { get; set; } 

        public ConcelhoEnum Concelho { get; set; }


        // public string? TipoContrato { get; set; } 
        public TipoContratoEnum TipoContrato { get; set; }

        public string? Requisitos { get; set; }

        public bool? VagaDisponivel { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Descricao { get; set; }

        public int Contagem { get; set; } = 0;


        //public string? Jornada { get; set; } 
        public JornadaEnum Jornada { get; set; }


        //public string? RegimeTrabalho { get; set; } 
        public RegimeTrabalhoEnum RegimeTrabalho { get; set; }



        public virtual ICollection<AplicacaoTrabalho>? AplicacaoTrabalho { get; set; }
    }
}
