using JobPortal_API.Models.Enums;

namespace JobPortal_API.DTOs
{
    public class HistoricoCandidaturaDTO
    {
        public int IdAplicacao { get; set; }
        public DateTime DataAplicacao { get; set; }
        public string? AplicacaoAceite { get; set; }

        public int IdOferta { get; set; }
        public string Titulo { get; set; }
        public ConcelhoEnum Concelho { get; set; }
        public RegimeTrabalhoEnum RegimeTrabalho { get; set; }
        public TipoContratoEnum TipoContrato { get; set; }
        public float? Salario { get; set; }
        public JornadaEnum Jornada { get; set; }

        public string NomeEmpresa { get; set; }
    }
}
