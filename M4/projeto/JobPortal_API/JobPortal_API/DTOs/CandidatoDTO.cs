using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JobPortal_API.DTOs
{
    public class CandidatoDTO
    {
        /// <summary>
        /// Identificador único do candidato.
        /// Nota de Fluxo: Gerado de forma automática pela base de dados. Deve ser omitido/ignorado nos payloads de criação (POST) e edição (PUT), mas torna-se obrigatório nos contratos de leitura (GET) para consumo do Front-end.
        /// </summary>
        [Description("ID único do Candidato - Omitir em POST/PUT, obrigatório em GET")]
        public int IdCandidato { get; set; }
        public string Nome { get; set; }
        /// <summary>
        /// Email do candidato sincronizado com as credenciais do Identity.
        /// Nota de Fluxo: Usado como chave de pesquisa no fluxo de consulta pública por email.
        /// </summary>
        public string Email { get; set; }
        public int? Telefone { get; set; }
        public string? Morada { get; set; }
        [Column(TypeName = "Date")]
        public DateTime? DataNasc { get; set; }
        public string? LinkedIn { get; set; }
        public string? Facebook { get; set; }

        //public int LocalUserId { get; set; } = 0;
    }
}
