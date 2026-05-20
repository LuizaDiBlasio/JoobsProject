using System.ComponentModel;
using System.Text.Json.Serialization;

namespace JobPortal_API.DTOs
{
    public class EmpresaDTO
    {
        /// <summary>
        /// Identificador único da empresa. 
        /// Nota de Fluxo: Este campo é gerado automaticamente pelo banco de dados. Deve ser omitido/ignorado no payload de criação (POST) e edição (PUT), mas é obrigatório e retornado nos fluxos de leitura (GET) para consumo do Front-end e validação de filtros.
        /// </summary>
        [Description("ID único da Empresa - Ignorar em mutações (POST/PUT), obrigatório em consultas (GET)")]
        public int IdEmpresa { get; set; }
        public string Nome { get; set; }
        public string? Localidade { get; set; }
        public string Email { get; set; }
        public int? Telefone { get; set; }
        public int? NoFuncionarios { get; set; }
        public string? ZonaAtuacao { get; set; }
        public string? LinkedIn { get; set; }
        public string? Facebook { get; set; }
    }
}
