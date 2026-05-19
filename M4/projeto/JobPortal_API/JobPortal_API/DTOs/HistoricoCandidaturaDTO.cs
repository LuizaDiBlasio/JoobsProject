namespace JobPortal_API.DTOs
{
    public class HistoricoCandidaturaDTO
    {
        public int IdAplicacao { get; set; }
        public DateTime DataAplicacao { get; set; }
        public string? AplicacaoAceite { get; set; }

        public int IdOferta { get; set; }
        public string Titulo { get; set; }
        public string NomeConcelho { get; set; }
        public string? RegimeTrabalho { get; set; }
        public string? TipoContrato { get; set; }
        public float? Salario { get; set; }
        public string? Jornada { get; set; }

        public string NomeEmpresa { get; set; }
    }
}
