using System.ComponentModel;
using System.Text.Json.Serialization;

namespace JobPortal_API.DTOs
{
    public class AplicacaoTrabalhoDTO
    {
        /// <summary>
        /// ATENÇÃO: Campo gerado automaticamente pela Base de Dados. 
        /// Ignorar no POST (Criação). Necessário no PUT (Edição) se enviado no corpo.
        /// </summary>
        [Description("Identificador único da aplicação. Gerado automaticamente pelo banco no POST.")]
        public int IdAplicacao { get; set; }

        /// <summary>
        /// ID da Oferta de Emprego à qual o candidato se está a candidatar.
        /// </summary>
        [Description("ID da vaga/oferta de emprego (Obrigatório).")]
        public int IdOferta { get; set; }

        /// <summary>
        /// ATENÇÃO: No POST/PUT este campo é ignorado e preenchido automaticamente via Token JWT.
        /// </summary>
        [Description("Injetado automaticamente pelo Back-end via Token JWT.")]
        public int IdCandidato { get; set; }

        /// <summary>
        /// Data em que a candidatura foi realizada. Preenchida automaticamente com o horário atual.
        /// </summary>
        [Description("Data da candidatura. Gerada automaticamente como o horário atual.")]
        public DateTime DataAplicacao { get; set; } = DateTime.Now;

        /// <summary>
        /// Estado da candidatura (Aceite/Recusado/Pendente). 
        /// ATENÇÃO: No POST este campo nasce como null e só deve ser alterado pela Empresa no futuro.
        /// </summary>
        [Description("Status da candidatura. Controlado apenas pela Empresa. Nasce como null.")]
        public string? aplicacaoAceite { get; set; } = null;
    }
}
