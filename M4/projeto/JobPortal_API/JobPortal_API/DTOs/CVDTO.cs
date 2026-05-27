using JobPortal_API.Models.Enums;
using System.Text.Json.Serialization;

namespace JobPortal_API.DTOs
{
    public class CVDTO
    {
        /// <summary>
        /// Identificador único do Currículo.
        /// Nota de Fluxo: Gerado automaticamente pela base de dados. Deve ser ignorado no POST, mas é obrigatório no PUT (URL e Body devem coincidir) e retornado no GET.
        /// </summary>
        public int IdCV { get; set; }
        /// <summary>
        /// Chave estrangeira do Candidato dono do CV.
        /// Nota de Fluxo: No POST e PUT, o sistema ignora o valor enviado pelo Front-end e força o ID extraído diretamente do Token JWT por segurança.
        /// </summary>
        public int IdCandidatoCv { get; set; }
        public string Nome { get; set; }
        public ConcelhoEnum Concelho { get; set; } // ALTERAÇÃO 26/05
        public EscolaridadeEnum Escolaridade { get; set; }  // ALTERAÇÃO 26/05
        public string? ExpProfissional { get; set; }
        public string? Competencias { get; set; }
        public string? Interesses { get; set; }
    }
}
