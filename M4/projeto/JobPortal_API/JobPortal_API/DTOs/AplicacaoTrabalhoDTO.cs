using System.Text.Json.Serialization;

namespace JobPortal_API.DTOs
{
    public class AplicacaoTrabalhoDTO
    {
        [JsonIgnore] // Esconde o ID da aplicação (gerado pelo banco) - Isso faz o campo sumir do Swagger e do JSON de entrada
        public int IdAplicacao { get; set; }
        public int IdOferta { get; set; }
        [JsonIgnore] // ALTERAÇÃO: Esconde o ID do Candidato (injetado via Token JWT)
        public int IdCandidato { get; set; }
        public DateTime DataAplicacao { get; set; } = DateTime.Now;
        [JsonIgnore] // ALTERAÇÃO: Esconde o status (controlado apenas pela Empresa no futuro)
        public string? aplicacaoAceite { get; set; } = null;
    }
}
