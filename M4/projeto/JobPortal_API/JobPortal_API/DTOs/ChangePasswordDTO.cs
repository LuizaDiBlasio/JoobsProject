using System.ComponentModel;

namespace JobPortal_API.DTOs
{
    public class ChangePasswordDTO
    {
        /// <summary>
        /// ID do candidato a solicitar a alteração de senha.
        /// Nota de Fluxo: Campo obrigatório no corpo da requisição para validação cruzada com o parâmetro ID da URL e as Claims do Token JWT.
        /// </summary>
        [Description("ID do Candidato usado na validação cruzada de rota")]
        public int IdCandidato { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
