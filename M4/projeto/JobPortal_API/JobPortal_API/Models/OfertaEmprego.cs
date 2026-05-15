using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal_API.Models
{
    public class OfertaEmprego
    {
        [Key]
        public int IdOferta { get; set; }

        [ForeignKey("IdEmpresa")]
        public int IdEmpresa { get; set; }
      
        public Empresa Empresa { get; set; }

        public string Titulo { get; set; }

        public float? Salario { get; set; }

        //public string? Jornada { get; set; } - Propriedade calculada com booleana IsFullTime

        //public string? Localização { get; set; } - Normalização da Tabela

        [ForeignKey("IdLocalidade")]
        public int? IdLocalidade { get; set; }

        public Localidade Localidade { get; set; }

        //public string? RegimeTrabalho { get; set; } - Propriedade calculada com boolean IsPresencial

        // public string? TipoContrato { get; set; } - Normalização da tabela

        [ForeignKey("IdTipoContrato")]
        public int? IdTipoContrato { get; set; }

        public TipoContrato? TipoContrato { get; set; }

        public string? Requisitos { get; set; }
        public bool? VagaDisponivel { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Descricao { get; set; }

        public int Contagem { get; set; } = 0;

        public bool? IsFullTime { get; set; }

        public string Jornada => IsFullTime switch
        {
            true => "Full time",
            false => "Part time",
            null => "Flexível"
        };

        public bool? IsPresencial { get; set; }

        public string RegimeTrabalho => IsPresencial switch
        {
            true => "Presencial",
            false => "Remoto",
            null => "Híbrido"
        };

        public virtual ICollection<AplicacaoTrabalho>? AplicacaoTrabalho { get; set; }
    }
}
