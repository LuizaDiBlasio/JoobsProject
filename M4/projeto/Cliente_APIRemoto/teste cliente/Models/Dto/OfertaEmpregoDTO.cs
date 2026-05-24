using teste_cliente.Models.Enums;

namespace teste_cliente.Models.Dto
{
    public class OfertaEmpregoDTO
    {
        public int IdOferta { get; set; }
        public int IdEmpresa { get; set; }

        public Empresa Empresa { get; set; }

        public string Titulo { get; set; }

        public float? Salario { get; set; }

        public string LogoEmpresaBase64 { get; set; }

        public ConcelhoEnum Concelho { get; set; }
        public TipoContratoEnum TipoContrato { get; set; }

        public string? Requisitos { get; set; }
        public bool? VagaDisponivel { get; set; }

        public string? Descricao { get; set; }

        public int Contagem { get; set; } = 0;

        public JornadaEnum Jornada { get; set; }

        public RegimeTrabalhoEnum RegimeTrabalho { get; set; }



        public virtual ICollection<AplicacaoTrabalho>? AplicacaoTrabalho { get; set; }
    }
}
