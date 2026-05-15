using System.Text.Json.Serialization;

namespace JobPortal_API.DTOs
{
    public class AplicacaoTrabalhoDTO
    {
        [JsonIgnore] // Isso faz o campo sumir do Swagger e do JSON de entrada
        public int IdAplicacao { get; set; }
        public int IdOferta { get; set; }      
        public int IdCandidato { get; set; }
        public DateTime DataAplicacao { get; set; } = DateTime.Now;
        public string? aplicacaoAceite { get; set; } = null;
    }
}
