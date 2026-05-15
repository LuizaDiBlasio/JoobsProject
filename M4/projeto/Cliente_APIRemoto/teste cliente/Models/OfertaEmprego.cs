using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Permissions;

namespace teste_cliente.Models
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

        //public string? Jornada { get; set; }
        public string? Localização { get; set; }

        //public string? RegimeTrabalho { get; set; }
        public string? TipoContrato { get; set; }
        public string? Requisitos { get; set; }
        public bool? VagaDisponivel { get; set; }
        public string? Descricao { get; set; }
        public int Contagem { get; set; } = 0;
        public string LogoEmpresaBase64 { get; set; }

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
